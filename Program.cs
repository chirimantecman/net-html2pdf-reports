using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfSharp.Pdf;

using TheArtOfDev.HtmlRenderer.PdfSharp;

using html2pdf_report_generator.parser;
using PdfSharp;
using System.Text.RegularExpressions;

namespace html2pdf_report_generator
{
    class Program
    {
        private static string templatePath;
        private static string templateFile;
        private static string outputPath;

        static void Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;
            if ((templatePath = appSettings["template-path"]) != null
                && (templateFile = appSettings["template-file"]) != null
                && (outputPath = appSettings["output-path"]) != null)
            {
                PreProcessor preProcessor = new PreProcessor();
                string tempFile01 = preProcessor.parseTemplateFile(
                    templatePath + templateFile);
                Processor processor = new Processor(tempFile01,
                    preProcessor.dataSources, preProcessor.dataSets);
                string tempFile02 = processor.parseLoopTags(tempFile01);
                string tempFile03 = processor.processLoopTags(tempFile02);
                string tempFile04 = processor.processSingleTags(tempFile03);
                StreamReader htmlFile = null;
                try
                {
                    htmlFile = new StreamReader(tempFile04);
                    PdfDocument pdfOutput = PdfGenerator.GeneratePdf(
                        htmlFile.ReadToEnd(), PageSize.Letter);
                    DateTime generationTime = DateTime.Now;
                    pdfOutput.Save(
                        outputPath + Regex.Replace(templateFile, @"\.\w+$",
                        generationTime.ToString("-yyyyMMdd-HHmmss") + ".pdf"));
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
                catch (OutOfMemoryException ex)
                {
                    throw ex;
                }
                finally
                {
                    if (htmlFile != null)
                        htmlFile.Close();
                }
            }
        }
    }
}
