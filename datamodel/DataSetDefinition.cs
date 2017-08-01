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
        private DataSourceDefinition dataSource;
        private DataSetType datasetType;
        private string name;
        private string obj;
        private Dictionary<string, DataSetParameter> parameters;


        /**/
        public DataSetDefinition()
        {
            this.parameters = new Dictionary<string,DataSetParameter>();
        }


        /**************************************************************
         * PRIVATE METHODS.
         *************************************************************/

        /*
         * parseDataSetDefinition - expects a string that holds a
         * dataset definition.  The expected format is:
         * 
         * data_source="<ds>" name="<nm>" type="<tp>"
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
        public DataSetDefinition parseDefinition(string def)
        {
            Dictionary<string, string> auxDict = extractTags(def);
            // name:
            if (!auxDict.ContainsKey("name"))
                throw new FormatException("The dataset definition is missing "
                    + "the mandatory 'name' attribute");
            else
                this.name = auxDict["name"];

            // type:
            if (!auxDict.ContainsKey("type"))
                throw new FormatException("The dataset definition is missing "
                    + "the mandatory 'type' attribute");
            else
            {
                switch (auxDict["type"])
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

            // object:
            if (!auxDict.ContainsKey("object"))
                throw new FormatException("The dataset definition is missing "
                    + "the mandatory 'object' attribute");
            else
                this.obj = auxDict["object"];

            // parameters:
            Boolean ok = true;
            int i = 0;
            string auxParamDef;
            while (ok && i < 100)
            {
                auxDict.TryGetValue("param" + (++i).ToString(), out auxParamDef);
                if (ok = (auxParamDef != null && auxParamDef != ""))
                {
                    DataSetParameter auxParam = new DataSetParameter();
                    auxParam.parseDefinition(auxParamDef);
                    parameters.Add("param"+i.ToString(), auxParam);
                }
            }
            return this;
        }


        /*
         * 
         */
        private Dictionary<string, string> extractTags(string def)
        {
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
    }
}
