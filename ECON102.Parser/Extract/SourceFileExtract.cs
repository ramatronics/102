using System.Collections.Generic;

namespace ECON102.Parser.Extract
{
    public class SourceFileExtract : BaseFileExtract
    {
        public SourceFileExtract(string fileName_, int startpage_ = 1, int endPage_ = 0)
            : base(fileName_, startpage_, endPage_)
        {
        }

        public override IList<Question> GetQuestions(int firstQuestion = 1, int lastQuestion = 1)
        {
            return base.GetQuestions(") ", firstQuestion, lastQuestion);
        }
    }
}