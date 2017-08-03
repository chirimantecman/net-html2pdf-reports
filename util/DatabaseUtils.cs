using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace html2pdf_report_generator.util
{
    public static class DatabaseUtils
    {
        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, 
                    StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
