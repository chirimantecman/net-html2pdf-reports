using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using html2pdf_report_generator.datamodel;

namespace html2pdf_report_generator.parser
{
    /*
     * PreProcessor - This class attempts to read and parse the
     * template file specified in the constructor or set.  The
     * file is specified by the whole path identification property
     * templateFilePath.
     * 
     * The parsing done at the pre-processor level is to get the data
     * source and data set information, and check it's validity. Once
     * parsed, the PreProcessor object will hold this information.
     */
    class PreProcessor
    {
        /**************************************************************
         * CONSTANTS.
         *************************************************************/
        private int MAX_DATASOURCES = 10;
        private int MAX_DATASETS = 50;



        /**************************************************************
         * MEMBERS.
         *************************************************************/
        public Dictionary<string, DataSourceDefinition> dataSources
        { 
            get; set; 
        }
        public Dictionary<string, DataSetDefinition> dataSets { get; set; }
        public string templateFilePath { get; set; }



        /**************************************************************
         * CONSTRUCTORS.
         *************************************************************/

        /*
         * Default constructor.
         */
        public PreProcessor()
        {
            dataSources = new Dictionary<string, DataSourceDefinition>();
            dataSets = new Dictionary<string, DataSetDefinition>();
        }


        /*
         * Constructor that takes the template file path.
         */
        public PreProcessor(string filePath)
        {
            templateFilePath = filePath;
            dataSources = new Dictionary<string, DataSourceDefinition>();
            dataSets = new Dictionary<string, DataSetDefinition>();
        }



        /**************************************************************
         * PUBLIC METHODS.
         *************************************************************/

        /*
         * parseTemplateFile - parses the templated file specified in
         * the templateFilePath property, searching for data source and
         * data set definitions.  If valid definitions are found, they
         * are stored in this object.
         * 
         * Data source definition lines must appear before data set
         * definition lines.
         */
        public string parseTemplateFile()
        {
            int sourceCount = 0;
            int setCount = 0;
            StreamReader file = null;
            string tempFilePath = null;
            StreamWriter tempFile = null;
            try
            {
                string line;
                file = new StreamReader(templateFilePath);
                tempFilePath = Path.GetTempFileName();
                tempFile = new StreamWriter(tempFilePath);
                // Parse template file.
                while ((line = file.ReadLine()) != null)
                {
                    if (DataSourceDefinition.isDataSourceDefinition(line)
                        && sourceCount++ <= MAX_DATASOURCES)
                    {
                        DataSourceDefinition dataSourceDef =
                            new DataSourceDefinition();
                        dataSourceDef.parseDefinition(line);
                        dataSources.Add(dataSourceDef.name, dataSourceDef);
                    }
                    else if (DataSetDefinition.isDataSetDefinition(line)
                        && setCount++ <= MAX_DATASETS)
                    {
                        DataSetDefinition dataSetDef =
                            new DataSetDefinition();
                        dataSetDef.parseDefinition(line);
                        if (!dataSources.ContainsKey(dataSetDef.dataSource))
                            throw new ArgumentException("The specified "
                                + "data source has not been defined: "
                                + dataSetDef.dataSource);
                        dataSets.Add(dataSetDef.name, dataSetDef);
                    }
                    else
                    {
                        tempFile.WriteLine(line);
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
            finally
            {
                if (file != null)
                    file.Close();
                if (tempFile != null)
                    tempFile.Close();
            }
        }


        /*
         * parseTemplateFile - parses the templated file specified in
         * the filePath argument, searching for data source and
         * data set definitions.  If valid definitions are found, they
         * are stored in this object.
         */
        public string parseTemplateFile(string filePath)
        {
            templateFilePath = filePath;
            return parseTemplateFile();
        }
    }
}
