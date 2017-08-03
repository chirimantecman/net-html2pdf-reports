using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

using html2pdf_report_generator.datamodel;
using html2pdf_report_generator.util;
using System.Data;

namespace html2pdf_report_generator.parser
{
    class Processor
    {
        /**************************************************************
         * CONSTANTS.
         *************************************************************/
        private const string LOOP_PLACEHOLDER_DEF_REGEX = 
            @"^\{%\s+looptag\s+(?<name>\w+)\s+%\}$";



        /**************************************************************
         * MEMBERS.
         *************************************************************/
        public string templateFilePath { get; set; }
        public List<TemplateTag> loopTags { get; set; }
        public Dictionary<string, DataSourceDefinition> dataSources 
        {
            get;
            set; 
        }
        public Dictionary<string, DataSetDefinition> dataSets
        {
            get;
            set;
        }



        /*************************************************************
         * CONSTRUCTORS.
         *************************************************************/

        /*
         * Default constructor.
         */
        public Processor() {}


        /*
         * Constructor that takes the template file path.
         */
        public Processor(string filePath)
        {
            templateFilePath = filePath;
        }


        /*
         * Constructor that takes the template file path, data source
         * dictionary and dataset dictionary.
         */
        public Processor(string filePath, 
            Dictionary<string, DataSourceDefinition> dataSourceDefs,
            Dictionary<string, DataSetDefinition> dataSetDefs)
        {
            templateFilePath = filePath;
            dataSources = dataSourceDefs;
            dataSets = dataSetDefs;
        }



        /**************************************************************
         * PUBLIC METHODS.
         *************************************************************/

        /*
         * parseLoopTags - parses the file indicated by the
         * templateFilePath property for loop tags and extracts the
         * information in the tags for later processing.
         * 
         * For now nested loop tags are ignored.
         */
        public string parseLoopTags()
        {
            StreamReader file = null;
            StreamWriter tempFile = null;
            loopTags = new List<TemplateTag>();
            Boolean inLoop = false;
            try
            {
                string line;
                file = new StreamReader(templateFilePath);
                string tempFilePath = Path.GetTempFileName();
                tempFile = new StreamWriter(tempFilePath);
                while ((line = file.ReadLine()) != null)
                {
                    TemplateTag tag = new TemplateTag(line);
                    if (!inLoop)
                    {
                        if (tag.isLoopStartTemplateTag())
                        {
                            inLoop = true;
                            string name = initAndStoreLoopTag(tag);
                            tempFile.WriteLine("{% looptag " + name + " %}");
                        }
                        else
                        {
                            tempFile.WriteLine(line.Trim());
                        }
                    }
                    else
                    {
                        if (tag.isLoopEndTemplateTag())
                            inLoop = false;
                        else
                            loopTags.Last().innerHtml += line.Trim();
                    }
                }
                return tempFilePath;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                throw ex;
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
            finally
            {
                if (file != null)
                    file.Close();
                if (tempFile != null)
                    tempFile.Close();
            }
        }


        /*
         * 
         */
        public string parseLoopTags(string filePath)
        {
            templateFilePath = filePath;
            return parseLoopTags();
        }


        /*
         * processLoopTags - takes the parsed loop tags and generates
         * the new HTML to be inserted.  It stores the new HTML in the
         * replacement property and also returns the path to a new 
         * temporary file with the new replaced content.
         */
        public string processLoopTags()
        {
            StreamReader file = null;
            StreamWriter tempFile = null;
            SqlConnection conn = null;
            try
            {
                string line;
                string name;
                file = new StreamReader(templateFilePath);
                string tempFilePath = Path.GetTempFileName();
                tempFile = new StreamWriter(tempFilePath);
                while ((line = file.ReadLine()) != null)
                {
                    if ((name = extractLoopName(line)) != String.Empty)
                    {
                        TemplateTag loopTag = loopTags.Find(
                            item => item.name == name);
                        if (loopTag.name != null)
                        {
                            if (dataSets.ContainsKey(loopTag.dataSet))
                            {
                                conn = openConnection(loopTag);
                                SqlDataReader rs = getDatasetRecords(conn, loopTag);
                                if (rs != null && rs.HasRows)
                                {
                                    loopTag = replaceLoopTagsWithValues(rs, loopTag);
                                    tempFile.WriteLine(loopTag.replacement.Trim());
                                }
                            }
                        }
                    }
                    else
                        tempFile.WriteLine(line);
                }
                return tempFilePath;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                throw ex;
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
            finally
            {
                if (file != null)
                    file.Close();
                if (tempFile != null)
                    tempFile.Close();
                if (conn != null)
                    conn.Close();
            }
        }


        /*
         * processLoopTags - takes the parsed loop tags and generates
         * the new HTML to be inserted.
         */
        public string processLoopTags(string filePath)
        {
            templateFilePath = filePath;
            return processLoopTags();
        }


        /*
         * processSingleTags - parses the file identified by the
         * templateFilePath property, processes and generates
         * the new HTML to be inserted.  It stores the new HTML in the
         * replacement property and also returns the path to a new 
         * temporary file with the new replaced content.
         */
        public string processSingleTags()
        {
            StreamReader file = null;
            StreamWriter tempFile = null;
            SqlConnection conn = null;
            try
            {
                string line;
                file = new StreamReader(templateFilePath);
                string tempFilePath = Path.GetTempFileName();
                tempFile = new StreamWriter(tempFilePath);
                while ((line = file.ReadLine()) != null)
                {
                    TemplateTag tag = new TemplateTag(line);
                    if (tag.hasSingleTemplateTags())
                    {
                        foreach (Match m in tag.extractSingleTemplateTags())
                        {
                            TemplateTag aux = new TemplateTag(tag.raw);
                            aux.parseSingleTemplateTag(m.Value);
                            if (dataSets.ContainsKey(aux.dataSet))
                            {
                                conn = openConnection(aux);
                                SqlDataReader rs = getDatasetRecords(conn, aux);
                                if (rs != null)
                                {
                                    aux = replaceSingleTagsWithValues(rs, aux);
                                    tag.raw = aux.replacement;
                                    tag.replacement = aux.replacement;
                                }
                            }
                        }
                        tempFile.WriteLine(tag.replacement.Trim());
                    }
                    else
                        tempFile.WriteLine(line);
                }
                return tempFilePath;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            catch (DirectoryNotFoundException ex)
            {
                throw ex;
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
            finally
            {
                if (file != null)
                    file.Close();
                if (tempFile != null)
                    tempFile.Close();
                if (conn != null)
                    conn.Close();
            }
        }


        /*
         * processSingleTags - parses the file identified by the
         * templateFilePath property, processes and generates
         * the new HTML to be inserted.
         */
        public string processSingleTags(string filePath)
        {
            templateFilePath = filePath;
            return processSingleTags();
        }



        /**************************************************************
         * PRIVATE METHODS.
         *************************************************************/
        
        /*
         * openConnection - attempts to get a connection according
         * to the information in the data source definition.  If
         * succesful, it returns an open SqlConnection object, if
         * not it returns null.
         */
        private SqlConnection openConnection(TemplateTag tag)
        {
            if (!dataSets.ContainsKey(tag.dataSet))
                return null;
            if (!dataSources.ContainsKey(dataSets[tag.dataSet].dataSource))
                return null;
            DataSourceDefinition dataSourceDef =
                dataSources[dataSets[tag.dataSet].dataSource];
            try
            {
                SqlConnectionStringBuilder builder =
                    new SqlConnectionStringBuilder(
                        dataSourceDef.connectionString);
                SqlConnection conn = new SqlConnection(
                    builder.ConnectionString);
                conn.Open();
                return conn;
            }
            catch (KeyNotFoundException ex)
            {
                return null;
            }
            catch (FormatException ex)
            {
                return null;
            }
            catch (ArgumentException ex)
            {
                return null;
            }
            catch (InvalidOperationException ex)
            {
                return null;
            }
            catch (SqlException ex)
            {
                return null;
            }
        }


        /*
         * initAndStoreLoopTag - convenience method to parse a validated
         * loop start tag and store the information in the list of
         * loop tags of the object.
         * 
         * It returns the new random name of the loop tag, which is used
         * for replacing the generated HTML in place of the inserted
         * placeholder:
         * 
         * {% looptag <name> %}
         */
        private string initAndStoreLoopTag(TemplateTag tag)
        {
            tag.parseLoopStartTemplateTag();
            tag.name = Path.GetRandomFileName().Replace(".", "");
            loopTags.Add(tag);
            return tag.name;
        }


        /*
         * extractLoopName - attempts to extract the name of a loop tag
         * from a valid loop tag placeholder.  It returns the name
         * found or String.Empty if not found (invalid placeholder).
         */
        private string extractLoopName(string line)
        {
            string aux = util.ParseUtils.removeLineEndings(line, true);
            Regex pattern = new Regex(LOOP_PLACEHOLDER_DEF_REGEX);
            return pattern.Match(aux).Groups["name"].Value;
        }


        /*
         * 
         */
        private SqlDataReader getDatasetRecords(SqlConnection conn,
            TemplateTag tag)
        {
            SqlCommand cmd = new SqlCommand();
            if (dataSets.ContainsKey(tag.dataSet))
            {
                cmd.CommandText = dataSets[tag.dataSet].obj;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                return cmd.ExecuteReader();
            }
            return null;
        }


        /*
         * 
         */
        private TemplateTag replaceLoopTagsWithValues(SqlDataReader rs,
            TemplateTag tag)
        {
            string simpleTagRegex = @"\{\{\s+" + tag.innerVar
                + @"\.(?<field>\w+)\s+\}\}";
            Regex pattern = new Regex(simpleTagRegex);
            while (rs.Read())
            {
                string aux = tag.innerHtml;
                foreach (Match m in pattern.Matches(aux))
                {
                    if (DatabaseUtils.HasColumn(rs, m.Groups["field"].Value))
                    {
                        aux = aux.Replace(m.Value,
                            rs[m.Groups["field"].Value].ToString());
                    }
                }
                tag.replacement += aux;
            }
            return tag;
        }


        /*
         * 
         */
        private TemplateTag replaceSingleTagsWithValues(SqlDataReader rs,
            TemplateTag tag)
        {
            string simpleTagRegex = @"\{\{\s+" + tag.dataSet
                + @"\.(?<field>" + tag.innerVar + @")\s+\}\}";
            Regex pattern = new Regex(simpleTagRegex);
            if (rs.HasRows)
            {
                rs.Read();
                string aux = tag.raw;
                foreach (Match m in pattern.Matches(aux))
                {
                    if (DatabaseUtils.HasColumn(rs, m.Groups["field"].Value))
                    {
                        aux = aux.Replace(m.Value,
                            rs[m.Groups["field"].Value].ToString());
                    }
                }
                tag.replacement = aux;
            }
            return tag;
        }
    }
}
