using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ECON102.Parser
{
    public class SampleExamFileExtract
    {
        private string _rawtxt;
        private string _fileName;
        private static string[] qFormat = { "A)", "B)", "C)", "D)", "E)" };

        public SampleExamFileExtract(string fileName_, int startpage_ = 1, int endPage_ = 0)
        {
            _fileName = fileName_;

            StringBuilder sBuilder = new StringBuilder();

            using (PdfReader pdfReader = new PdfReader(fileName_))
            {
                if (endPage_ == 0)
                    endPage_ = pdfReader.NumberOfPages;
                // Loop through each page of the document
                for (var page = 1; page <= endPage_; page++)
                {
                    ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();

                    var currentText = PdfTextExtractor.GetTextFromPage(
                        pdfReader,
                        page,
                        strategy);

                    currentText =
                        Encoding.UTF8.GetString(Encoding.Convert(
                            Encoding.Default,
                            Encoding.UTF8,
                            Encoding.Default.GetBytes(currentText)));

                    sBuilder.Append(currentText);
                }
            }

            _rawtxt = sBuilder.ToString();
        }

        public IList<Question> GetQuestions(int firstQuestion = 1, int lastQuestion = 1)
        {
            string[] rawLines = _rawtxt.Split('\n');

            Dictionary<int, Question> lineToQ = new Dictionary<int, Question>();
            List<Question> qList = new List<Question>();

            for (int i = 0; i < rawLines.Length; i++)
            {
                if (rawLines[i].Contains(firstQuestion + ". "))
                {
                    Question tmpQuestion = new Question() { QuestionNumber = firstQuestion };
                    lineToQ[i] = tmpQuestion;

                    rawLines[i] = rawLines[i].Replace(firstQuestion + ". ", "");
                    firstQuestion++;
                }
            }

            foreach (int key in lineToQ.Keys)
            {
                for (int i = key; i < rawLines.Length; i++)
                {
                    if (ContainsAnswers(rawLines[i]))
                        break;
                    else
                        lineToQ[key].QText += rawLines[i];
                }
            }

            return lineToQ.Values.ToList();
        }

        private bool ContainsAnswers(string rawString)
        {
            foreach (var s in qFormat)
            {
                if (rawString.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
