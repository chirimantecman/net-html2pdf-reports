using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using html2pdf_report_generator.datamodel;

namespace html2pdf_report_generator
{
    class Program
    {
        static void Main(string[] args)
        {
            string def = "name=\"hola rana\" datasource = \"ds01\" "
                + "object=\"sp_la_rana\" type=\"stored procedure\" "
                + "param=\"p1,int,4\" param=\"p2,nvarchar,50\"";
            DataSetDefinition dsd = new DataSetDefinition();
            dsd.parseDefinition(def);
        }
    }
}
