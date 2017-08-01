using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace html2pdf_report_generator.util
{
    class ParseUtils
    {
        /*
         * Removes all line endings from a string, optionally replaces
         * them with whitespace.
         */
        public static string removeLineEndings(string line, Boolean whitespace=false)
        {
            string LINE_SEPARATOR = ((char)0x2028).ToString();
            string PARAGRAPH_SEPARATOR = ((char)0x2029).ToString();
            if (String.IsNullOrEmpty(line))
                return line;
            string replacement = whitespace ? " " : string.Empty;
            return line.Replace("\r\n", replacement
                ).Replace("\n", replacement
                ).Replace("\r", replacement
                ).Replace(LINE_SEPARATOR, replacement
                ).Replace(PARAGRAPH_SEPARATOR, replacement);
        }
    }
}
