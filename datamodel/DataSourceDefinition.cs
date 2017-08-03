using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace html2pdf_report_generator.datamodel
{
    /*
     * DataSourceDefinition - represents a set of information for
     * conecting to a datasource.  Currently the only implemented
     * datasource type is SQL Server via SQL Server security.
     */
    class DataSourceDefinition
    {
        /**************************************************************
         * CONSTANTS.
         *************************************************************/
        private const string DATASOURCE_DEF_REGEX =
            @"^\s*\{%\s+datasource\s+(?<definition>.+)\s+%\}\s*$";


        /**************************************************************
         * MEMBERS.
         *************************************************************/
        public string name { get; set; }
        public string server { get; set; }
        public string database { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string connectionString {
            get
            {
                return "Data Source=" + server
                    + "; Initial Catalog=" + database
                    + "; User ID=" + user
                    + "; Password=" + password;
            }
        }


        /**************************************************************
         * PUBLIC METHODS.
         *************************************************************/

        /*
         * isDataSourceDefinition - Returns true if the line contains a
         * valid data source definition block.  By valid we mean here
         * that it appears on one line, by itself and has an opening
         * and closing tag.  The inner validity is left to the actual
         * extraction methods.
         * 
         * The line can contain leading and trailing whitespace. And
         * have the following format:
         * 
         * {% datasource <definition> %}
         * 
         * Definition cannot contain other {% %} commands.
         */
        public static Boolean isDataSourceDefinition(string line)
        {
            DataSourceDefinition aux = new DataSourceDefinition();
            return aux.extractDefinition(line) != String.Empty;
        }


        /*
         * parseDefinition - expects a string that holds a data source
         * definition.  The expected format is:
         * 
         * {% datasource name="<nm>" server="<srv>" database="<db>"
         *      user="<usr>" password="<pwd>" %}
         * 
         * Where:
         *    <nm>: name for this definition, used in dataset
         *          definitions.
         *    <srv>: name of the datasource server.
         *    <db>: default database.
         *    <usr>: db username.
         *    <pwd>: SQL Server user password.
         *      
         *    The inner items can appear in any order.
         *    
         *    THE WHOLE DEFINITION MUST BE ON ONE LINE. 
         */
        public DataSourceDefinition parseDefinition(string line)
        {
            Dictionary<string, string> auxDict = extractTags(
                extractDefinition(line));
            name = util.ParseUtils.checkMandatoryKey(auxDict, "name");
            server = util.ParseUtils.checkMandatoryKey(auxDict, "server");
            database = util.ParseUtils.checkMandatoryKey(auxDict, "database");
            user = util.ParseUtils.checkMandatoryKey(auxDict, "user");
            password = util.ParseUtils.checkMandatoryKey(auxDict, "password");
            return this;
        }



        /**************************************************************
         * PRIVATE METHODS.
         *************************************************************/
        
        /*
         * extractTags - generates a dictionary from expected tags in a
         * datasource definition string. The expected format is as
         * follows:
         *               
         * name="<nm>" server="<srv>" database="<db>" user="<usr>"
         *      password="<pwd>"
         * 
         * Where:
         *      <nm>: name for this definition, used in dataset
         *            definitions.
         *      <srv>: name of the datasource server.
         *      <db>: default database.
         *      <usr>: db username.
         *      <pwd>: SQL Server user password.
         * 
         * THE WHOLE DEFINITION MUST APPEAR ON ONE LINE.
         * 
         * The order of elements in the definition is not important.
         */
        private Dictionary<string, string> extractTags(string def)
        {
            //FIXME: Try to move this code to a more general, reusable case.
            string aux = util.ParseUtils.removeLineEndings(def, true);
            Regex pattern = new Regex(@"(?<name>.+?)\s*=\s*""(?<value>.*?)""");
            Dictionary<string, string> auxDict = new Dictionary<string, string>();
            foreach (Match m in pattern.Matches(aux))
            {
                string key = m.Groups["name"].Value.Trim();
                string value = m.Groups["value"].Value.Trim();
                if (!auxDict.ContainsKey(key))
                    auxDict.Add(key, value);
            }
            return auxDict;
        }


        /*
         * extractDefinition - tests the input line for a match with
         * the predefined default Regex for data sources.  If a match
         * is found the inner definition is extracted (see the method
         * parseDefinition).  If no match is found, it String.Empty is
         * returned.
         */
        private string extractDefinition(string line)
        {
            string aux = util.ParseUtils.removeLineEndings(line, true);
            Regex pattern = new Regex(DATASOURCE_DEF_REGEX);
            return pattern.Match(aux).Groups["definition"].Value;
        }
    }
}
