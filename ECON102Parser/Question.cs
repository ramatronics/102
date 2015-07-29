using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECON102.Parser
{
    public class QuestionSet
    {
        public int ChapterId { get; private set; }
        public string ChapterName { get; private set; }
        public IList<Question> Questions { get; private set; }

        public QuestionSet(SourceFileExtract srcFiles_, int chapterId)
        {
            Questions = srcFiles_.GetQuestions();
            ChapterId = chapterId;
        }
    }

    public class UsedQuestionFlag
    {
        public int SectorId { get; set; }

        public string Chapter { get; set; }

        public int QNumber { get; set; }
    }

    public class Question
    {
        public bool Used { get; set; }

        public int QuestionNumber { get; set; }

        public string QText { get; set; }

        public Dictionary<string, string> Answers { get; set; }
    }
}
