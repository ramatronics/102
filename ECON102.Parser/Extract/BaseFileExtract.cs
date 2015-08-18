using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECON102.Parser.Extract
{
    public abstract class BaseFileExtract
    {
        public readonly string[] QOPT_FORMAT = { "A)", "B)", "C)", "D)", "E)" };

        protected string _rawtxt;
        protected string _fileName;

        public BaseFileExtract(string fileName_, int startpage_ = 1, int endPage_ = 0)
        {
            _fileName = fileName_;
            StringBuilder sBuilder = new StringBuilder();

            using (PdfReader pdfReader = new PdfReader(fileName_))
            {
                if (endPage_ == 0)
                    endPage_ = pdfReader.NumberOfPages;

                // Loop through each page of the document
                for (var page = startpage_; page <= endPage_; page++)
                {
                    ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    sBuilder.Append(currentText);
                }
            }

            _rawtxt = sBuilder.ToString();
        }

        public abstract IList<Question> GetQuestions(int firstQuestion = 1, int lastQuestion = 1);

        protected IList<Question> GetQuestions(string questionSuffix, int firstQuestion = 1, int lastQuestion = 1)
        {
            string qSearchFlag = firstQuestion + questionSuffix;
            string[] rawLines = _rawtxt.Split('\n');

            Dictionary<int, Question> lineToQ = new Dictionary<int, Question>();

            for (int i = 0; i < rawLines.Length; i++)
            {
                if (rawLines[i].Contains(qSearchFlag))
                {
                    Question tmpQuestion = new Question() { QuestionNumber = firstQuestion };
                    lineToQ[i] = tmpQuestion;

                    rawLines[i] = rawLines[i].Replace(qSearchFlag, "");
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

        protected bool ContainsAnswers(string rawString)
        {
            foreach (var s in QOPT_FORMAT)
                if (rawString.Contains(s))
                    return true;

            return false;
        }
    }
}