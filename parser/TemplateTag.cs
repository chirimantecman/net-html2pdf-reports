using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace html2pdf_report_generator.parser
{
    class TemplateTag
    {
        /**************************************************************
         * CONSTANTS.
         *************************************************************/
        private const string SINGLE_TAG_DEF_REGEX =
            @"\{\{\s+(?<dataset>\w+)\.(?<tag>\w+)\s+\}\}";
        private const string LOOP_START_TAG_DEF_REGEX =
            @"^\s*\{%\s+for\s+(?<inner>\w+\s+in\s+\w+)\s+%\}\s*$";
        private const string LOOP_START_INNER_TAGS_DEF_REGEX =
            @"^(?<element>\w+)\s+in\s+(?<collection>\w+)$";
        private const string LOOP_END_TAG_DEF_REGEX =
            @"^\s*\{%\s+endfor\s+%\}\s*$";



        /**************************************************************
         * MEMBERS.
         *************************************************************/
        public string name { get; set; }
        public string raw { get; set; }
        public string tag { get; set; }
        public string dataSet { get; set; }
        public string innerVar { get; set; }
        public string innerHtml { get; set; }
        public string replacement { get; set; }



        /**************************************************************
         * CONSTRUCTORS.
         *************************************************************/

        /*
         * Default constructor.
         */
        public TemplateTag() {}


        /*
         * Constructor that takes the initial raw tag.
         */
        public TemplateTag(string rw)
        {
            raw = rw;
        }


        /**************************************************************
         * PUBLIC METHODS.
         *************************************************************/

        /*
         * hasSingleTemplateTags - returns true if the stored raw tag
         * has at least one valid template tag of the single type in
         * it.
         * 
         * Single tags are of the form:
         * 
         * {{ <tag> }}
         * 
         * Where:
         *    <tag>: a tag reference of the form
         *           <dataset name>.<field name>
         */
        public Boolean hasSingleTemplateTags()
        {
            return extractSingleTemplateTags(raw).Count > 0;
        }


        /*
         * hasSingleTemplateTags - returns true if the input line has
         * at least one valid template tag of the single type in it.
         */
        public Boolean hasSingleTemplateTags(string line)
        {
            raw = line;
            return hasSingleTemplateTags();
        }


        /*
         * extractSingleTemplateTags - tests the input line for matches
         * with the predefined default Regex for single tags.  If one
         * or more matches are found the trimmed tags are extracted and
         * returned as a List<string>.  If no match is found, an empty
         * List is returned.
         */
        public MatchCollection extractSingleTemplateTags()
        {
            List<string> tags = new List<string>();
            string aux = util.ParseUtils.removeLineEndings(raw, true);
            Regex pattern = new Regex(SINGLE_TAG_DEF_REGEX);
            return pattern.Matches(aux);
        }


        /*
         * extractSingleTemplateTags - tests the input line for matches
         * with the predefined default Regex for single tags.  If one
         * or more matches are found the trimmed tags are extracted and
         * returned as a List<string>.  If no match is found, an empty
         * List is returned.
         */
        public MatchCollection extractSingleTemplateTags(string line)
        {
            raw = line;
            return extractSingleTemplateTags();
        }


        /*
         * isLoopStartTemplateTag - returns true if the input line is
         * a valid template tag for the start of loop constructs.
         * 
         * Loop start tags are of the form:
         * 
         * {% for <element> in <collection> %}
         * 
         * Where:
         *    <element>: a local variable name for the loop content.
         *    <collection>: a valid data set name.
         */
        public Boolean isLoopStartTemplateTag(string line)
        {
            raw = line;
            return extractLoopStartTemplateTags() != String.Empty;
        }


        /*
         * isLoopStartTemplateTag - returns true if the stored tag 
         * property is a valid template tag for the start of loop
         * constructs.
         * 
         * Loop start tags are of the form:
         * 
         * {% for <element> in <collection> %}
         * 
         * Where:
         *    <element>: a local variable name for the loop content.
         *    <collection>: a valid data set name.
         */
        public Boolean isLoopStartTemplateTag()
        {
            return extractLoopStartTemplateTags() != String.Empty;
        }


        /*
         * isLoopEndTemplateTag - returns true if the input line is
         * a valid template tag for the end of loop constructs.
         * 
         * Loop end tags are of the form:
         * 
         * {% endfor %}
         */
        public Boolean isLoopEndTemplateTag(string line)
        {
            string aux = util.ParseUtils.removeLineEndings(line, true);
            Regex pattern = new Regex(LOOP_END_TAG_DEF_REGEX);
            tag = raw;
            return pattern.IsMatch(aux);
        }


        /*
         * isLoopEndTemplateTag - returns true if raw tag property is
         * a valid template tag for the end of loop constructs.
         * 
         * Loop end tags are of the form:
         * 
         * {% endfor %}
         */
        public Boolean isLoopEndTemplateTag()
        {
            return isLoopEndTemplateTag(raw);
        }


        /*
         * parseLoopStartTemplateTag - parses a template tag as a loop
         * start tag, extracts the inner tags and stores them in the
         * object in properties dataset and innerVar.
         * 
         * It also returns the object.
         */
        public TemplateTag parseLoopStartTemplateTag(string line)
        {
            tag = line;
            return parseSingleTemplateTag();
        }


        /*
         * parseLoopStartTemplateTag - parses the tag property as a loop
         * start tag, extracts the inner tags and stores them in the
         * object in properties dataset and innerVar.
         * 
         * It also returns the object.
         */
        public TemplateTag parseLoopStartTemplateTag()
        {
            string aux = util.ParseUtils.removeLineEndings(tag, true);
            Regex pattern = new Regex(LOOP_START_TAG_DEF_REGEX);
            string innerTags = pattern.Match(aux).Groups["inner"].Value.Trim();
            Regex innerPattern = new Regex(LOOP_START_INNER_TAGS_DEF_REGEX);
            if (innerPattern.IsMatch(innerTags))
            {
                innerVar = innerPattern.Match(
                    innerTags).Groups["element"].Value;
                dataSet = innerPattern.Match(
                    innerTags).Groups["collection"].Value;
            }
            return this;
        }


        /*
        * parseSingleTemplateTag - parses a template tag as a single
        * tag, extracts the inner data and stores it in the object in
        * properties dataset and innerVar.
        * 
        * It also returns the object.
        */
        public TemplateTag parseSingleTemplateTag(string line)
        {
            string aux = util.ParseUtils.removeLineEndings(line, true);
            Regex pattern = new Regex(SINGLE_TAG_DEF_REGEX);
            if (pattern.IsMatch(aux))
            {
                innerVar = pattern.Match(aux).Groups["tag"].Value;
                dataSet = pattern.Match(aux).Groups["dataset"].Value;
            }
            return this;
        }


        /*
        * parseSingleTemplateTag - parses the tag property as a single
        * tag, extracts the inner data and stores it in the object in
        * properties dataset and innerVar.
        * 
        * It also returns the object.
        */
        public TemplateTag parseSingleTemplateTag()
        {
            return parseSingleTemplateTag(tag);
        }



        /**************************************************************
         * PRIVATE METHODS.
         *************************************************************/

        /*
         * extractLoopStartTemplateTags - tests the raw tag property
         * for matches with the predefined default Regex for start of
         * loop constructs.  If a matches is found the trimmed tag is
         * extracted and returned as a string and also stored.  If no
         * match is found, String.Empty is returned and stored.
         */
        private string extractLoopStartTemplateTags()
        {
            string aux = util.ParseUtils.removeLineEndings(raw, true);
            Regex pattern = new Regex(LOOP_START_TAG_DEF_REGEX);
            tag = pattern.Match(aux).Value.Trim();
            return tag;
        }
    }
}
