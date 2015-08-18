using System.Collections.Generic;

namespace ECON102.Parser.Extract
{
    //Extract questions from past midterms/finals (different format than practice questions, uses '.' after question number
    public class ExamFileExtract : BaseFileExtract
    {
        public ExamFileExtract(string fileName_, int startpage_ = 1, int endPage_ = 0)
            : base(fileName_, startpage_, endPage_)
        {
        }

        public override IList<Question> GetQuestions(int firstQuestion = 1, int lastQuestion = 1)
        {
            return base.GetQuestions(". ", firstQuestion, lastQuestion);
        }
    }
}