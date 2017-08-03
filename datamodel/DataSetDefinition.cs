using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace html2pdf_report_generator.datamodel
{
    /*
     * DataSetType - This enum defines the types of dataset sources
     * currently recognized.
     */
    enum DataSetType
    {
        TABLE,
        VIEW,
        STORED_PROCEDURE,
        FUNCTION
    }


    /*
     * ParameterDataType - This enum defines data types for dataset
     * parameters.  These should correspond with actual datatypes
     * in the data source.
     */
    enum ParameterDataType
    {
        BIT,
        TINYINT,
        INT,
        BIGINT,
        DECIMAL,
        FLOAT,
        REAL,
        CHAR,
        VARCHAR,
        TEXT,
        NCHAR,
        NVARCHAR,
        NTEXT,
        DATE,
        DATETIME,
        TIME
    }


    /*
     * DataSetParameter - represents a parameter of a data set.
     */
    class DataSetParameter
    {
        private const string DEF_NAME = "";
        private const ParameterDataType DEF_TYPE = ParameterDataType.INT;
        private const int DEF_SIZE = 0;

        public string name { get; set; }
        public ParameterDataType dataType { get; set; }
        public int size { get; set; }


        /*
         * Default constructor.
         */
        public DataSetParameter()
        {
            this.name = DEF_NAME;
            this.dataType = DEF_TYPE;
            this.size = DEF_SIZE;
        }


        /**************************************************************
         * PUBLIC METHODS.
         *************************************************************/

        /*
         * 
         */
        public Boolean isDefault()
        {
            return (name == DEF_NAME) 
                && (dataType == DEF_TYPE) 
                && (size == DEF_SIZE);
        }


        /*
         * 
         */
        public DataSetParameter parseDefinition(string def)
        {
            char[] delimiter = {','};
            string[] param = def.Split(delimiter,
                StringSplitOptions.RemoveEmptyEntries);
            if (param.Length != 3)
                throw new ArgumentException("The parameter definition string "
                    + "has an invalid number of elements.");
            name = param[0];
            switch (param[1].ToUpper())
            {
                case "BIT":
                    dataType = ParameterDataType.BIT;
                    break;
                case "TINYINT":
                    dataType = ParameterDataType.TINYINT;
                    break;
                case "INT":
                    dataType = ParameterDataType.INT;
                    break;
                case "BIGINT":
                    dataType = ParameterDataType.BIGINT;
                    break;
                case "CHAR":
                    dataType = ParameterDataType.CHAR;
                    break;
                case "VARCHAR":
                    dataType = ParameterDataType.VARCHAR;
                    break;
                case "TEXT":
                    dataType = ParameterDataType.TEXT;
                    break;
                case "NCHAR":
                    dataType = ParameterDataType.NCHAR;
                    break;
                case "NVARCHAR":
                    dataType = ParameterDataType.NVARCHAR;
                    break;
                case "NTEXT":
                    dataType = ParameterDataType.NTEXT;
                    break;
                case "DATE":
                    dataType = ParameterDataType.DATE;
                    break;
                case "DATETIME":
                    dataType = ParameterDataType.DATETIME;
                    break;
                case "TIME":
                    dataType = ParameterDataType.TIME;
                    break;
                default:
                    throw new ArgumentException("The data type part of the "
                    + "parameter definition is invalid.");
            }
            try 
            {
                size = int.Parse(param[2]);
            }
            catch(Exception ex)
            {
                throw new ArgumentException("The size part of the parameter "
                    + "definition is invalid.");
            }
            return this;
        }
    }


    /*
     * DataSetDefinition - represents a set of information to obtain
     * data from an SQL DataSourceDefinition object.  It could be a
     * table, view, stored procedure or function.
     */
    class DataSetDefinition
    {
        /**************************************************************
         * CONSTANTS.
         *************************************************************/
        private const string DATASET_DEF_REGEX =
            @"^\s*\{%\s+dataset\s+(?<definition>.+)\s+%\}\s*$";



        /**************************************************************
         * MEMBERS AND PROPERTIES.
         *************************************************************/
        public string dataSource { get; set; }
        public DataSetType datasetType { get; set; }
        public string name { get; set; }
        public string obj { get; set; }
        public Dictionary<string, DataSetParameter> parameters { get; set; }


        /**/
        public DataSetDefinition()
        {
            this.parameters = new Dictionary<string,DataSetParameter>();
        }



        /**************************************************************
         * PUBLIC METHODS.
         *************************************************************/

        /*
         * isDataSetDefinition - Returns true if the line contains a
         * valid data set definition block.  By valid we mean here
         * that it appears on one line, by itself and has an opening
         * and closing tag.  The inner validity is left to the actual
         * extraction methods.
         * 
         * The line can contain leading and trailing whitespace. And
         * have the following format:
         * 
         * {% dataset <definition> %}
         * 
         * Definition cannot contain other {% %} commands.
         */
        public static Boolean isDataSetDefinition(string line)
        {
            DataSetDefinition aux = new DataSetDefinition();
            return aux.extractDefinition(line) != String.Empty;
        }


        /*
         * parseDataSetDefinition - expects a string that holds a
         * dataset definition.  The expected format is:
         * 
         * datasource="<ds>" name="<nm>" type="<tp>"
         *    object="<obj>" [param="<prm>" ...]
         * 
         * Where:
         *    <ds>: name of a defined datasource.
         *    <nm>: unique name for the data set.
         *    <tp>: the type of data set, should be one of:
         *          - table
         *          - view
         *          - stored procedure
         *          - function
         *    <obj>: the name of the data set item (eg., the name of
         *           the table)
         *    <prm>: a dataset parameter definition in the form:
         *    
         *             <name>,<type>[,<size>]
         *    
         *           Where:
         *           - name: parameter name
         *           - type: one of bit, tinyint, int, bigint, decimal,
         *                   float, real, char, varchar, text, nchar,
         *                   nvarchar, ntext, date, datetime, time
         *           - size: size in bytes of the parameter, optional.
         *    
         *    These items can appear in any order. A definition can
         *    have 0 or more parameters.
         *    
         *    THE WHOLE DEFINITION MUST BE ON ONE LINE.
         */
        public DataSetDefinition parseDefinition(string line)
        {
            Dictionary<string, string> auxDict = extractTags(
                extractDefinition(line));

            // datasource:
            this.dataSource = util.ParseUtils.checkMandatoryKey(auxDict,
                "datasource");

            // name:
            this.name = util.ParseUtils.checkMandatoryKey(auxDict, "name");

            // type:
            parseType(auxDict);  

            // object:
            this.obj = util.ParseUtils.checkMandatoryKey(auxDict, "object");

            // parameters:
            parseParameters(auxDict);
            return this;
        }



        /**************************************************************
         * PRIVATE METHODS.
         *************************************************************/

        /*
         * extractDefinition - tests the input line for a match with
         * the predefined default Regex for datasets.  If a match is
         * found the inner definition is extracted (see the method
         * parseDefinition).  If no match is found, it String.Empty is
         * returned.
         */
        private string extractDefinition(string line)
        {
            string aux = util.ParseUtils.removeLineEndings(line, true);
            Regex pattern = new Regex(DATASET_DEF_REGEX);
            return pattern.Match(aux).Groups["definition"].Value;
        }


        /*
         * extractTags - gets the inner definition of a dataset 
         * definition line and ectracts the tag information from it.
         */
        private Dictionary<string, string> extractTags(string def)
        {
            //FIXME: Try to move this code to a more general, reusable case.
            string aux = util.ParseUtils.removeLineEndings(def, true);
            Regex pattern = new Regex(@"(?<name>.+?)\s*=\s*""(?<value>.*?)""");
            Dictionary<string, string> auxDict = new Dictionary<string, string>();
            int i = 0;
            foreach (Match m in pattern.Matches(aux))
            {
                string key = m.Groups["name"].Value.Trim();
                string value = m.Groups["value"].Value.Trim();
                if (key.Equals("param"))
                    key = key + (++i).ToString();
                if (!auxDict.ContainsKey(key))
                    auxDict.Add(key, value);
            }
            return auxDict;
        }


        /*
         * parseParameters - attempts to parse parameters from a 
         * dictionary holding the key/value duples from extracted
         * dataset definition tags.
         * 
         * Parameter keys go from param1 to paramN, N may be 0, in
         * which case there are no parameters defined.
         * 
         * The results from this method are directly stored in the
         * parameters property of the object.
         */
        private void parseParameters(Dictionary<string, string> dict)
        {
            Boolean ok = true;
            int i = 0;
            string auxParamDef;
            while (ok && i < 100)
            {
                dict.TryGetValue("param" + (++i).ToString(), out auxParamDef);
                if (ok = (auxParamDef != null && auxParamDef != ""))
                {
                    DataSetParameter auxParam = new DataSetParameter();
                    auxParam.parseDefinition(auxParamDef);
                    parameters.Add("param" + i.ToString(), auxParam);
                }
            }
        }


        /*
         * parseType - attempts to parse the dataset type from a 
         * dictionary holding the key/value duples from extracted
         * dataset definition tags.
         * 
         * The results from this method are directly stored in the
         * datasetType property of the object.
         */
        private void parseType(Dictionary<string, string> dict)
        {
            if (!dict.ContainsKey("type"))
                throw new FormatException("The dataset definition is missing "
                    + "the mandatory 'type' attribute");
            else
            {
                switch (dict["type"])
                {
                    case "table":
                        this.datasetType = DataSetType.TABLE;
                        break;
                    case "view":
                        this.datasetType = DataSetType.VIEW;
                        break;
                    case "stored procedure":
                        this.datasetType = DataSetType.STORED_PROCEDURE;
                        break;
                    case "function":
                        this.datasetType = DataSetType.FUNCTION;
                        break;
                    default:
                        throw new ArgumentException("Invalid value for "
                            + "dataset type tag.");
                }
            }
        }
    }
}
