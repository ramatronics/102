using ECON102.Parser.Extract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ECON102.Parser
{
    //Have to clean this up later, it's pretty messy... ain't nobody got time for dat during exams tho
    internal class Program
    {
        private static string WORK_DIR = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "data");
        private static string DELTA_BASE = Path.Combine(WORK_DIR, @"delta");
        private static string SOURCE_BASE = Path.Combine(WORK_DIR, @"source");
        private static string OUT_FOLDER = Path.Combine(WORK_DIR, @"output");

        private static void Main(string[] args)
        {

            //Extracting questions from pdf files (Exams & Practice PDFs)
            /******************************************************************************************/
            Dictionary<int, List<QuestionSet>> sectorToQuestions = new Dictionary<int, List<QuestionSet>>();

            sectorToQuestions[1] = new List<QuestionSet>();
            sectorToQuestions[1].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "1"), 51), 1));
            sectorToQuestions[1].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "2"), 27), 2));
            sectorToQuestions[1].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "20"), 25), 20));
            sectorToQuestions[1].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "21"), 22), 21));

            sectorToQuestions[2] = new List<QuestionSet>();
            sectorToQuestions[2].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "23"), 27), 23));
            sectorToQuestions[2].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "24"), 21), 24));
            sectorToQuestions[2].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "25"), 20), 25));
            sectorToQuestions[2].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "26"), 23), 26));

            sectorToQuestions[3] = new List<QuestionSet>();
            sectorToQuestions[3].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "27"), 31), 27));
            sectorToQuestions[3].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "28"), 22), 28));
            sectorToQuestions[3].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "29"), 18), 29));
            sectorToQuestions[3].Add(new QuestionSet(new SourceFileExtract(BuildFileName_PDF(SOURCE_BASE, "30"), 20), 30));

            int practiceQCount = 0;
            int reusedQCount = 0;
            int totalQCount = 0;

            //Hardcoded params for now
            string examQDelta = Path.Combine(DELTA_BASE, "exam");
            string[] examQFiles = new string[] { "27,28,29,30", "s1", "s2", "ss1", "ss2" };
            int[] examQFileStartPage = new int[] { 1, 2, 2, 1, 1 };
            int[] examQFileEndPage = new int[] { 15, 14, 12, 10, 14 };

            Dictionary<string, IList<Question>> usedQs = new Dictionary<string, IList<Question>>();
            for (int i = 0; i < examQFiles.Length; i++)
            {
                ExamFileExtract extract = new ExamFileExtract(BuildFileName_PDF(examQDelta, examQFiles[i]), examQFileStartPage[i], examQFileEndPage[i]);
                IList<Question> qs = extract.GetQuestions();

                usedQs[examQFiles[i]] = qs;
                practiceQCount += qs.Count;
            }

            //Hardcoded params for now
            string practiceQDelta = Path.Combine(DELTA_BASE, "practice");
            string[] practiceQFiles = new string[] { "1,2", "20,21", "23", "24", "25", "26", "27,28" };
            int[] practiceQFileStartPage = new int[] { 1, 1, 1, 1, 1, 1, 1 };
            int[] practiceQFileEndPage = new int[] { 11, 8, 4, 4, 4, 5, 13 };

            for (int i = 0; i < practiceQFiles.Length; i++)
            {
                ExamFileExtract extract = new ExamFileExtract(BuildFileName_PDF(practiceQDelta, practiceQFiles[i]), practiceQFileStartPage[i], practiceQFileEndPage[i]);
                IList<Question> qs = extract.GetQuestions();

                if (usedQs.ContainsKey(practiceQFiles[i]) == false)
                    usedQs[practiceQFiles[i]] = qs;

                practiceQCount += qs.Count;
            }
            /******************************************************************************************/

            //Comparing questions to see which ones haven't been used in exams yet..... O(n^5) lol
            StringBuilder unusedQuestions = new StringBuilder();
            foreach (var key in usedQs.Keys)
            {
                foreach (var question in usedQs[key])
                {
                    if (string.IsNullOrEmpty(question.QText.Replace(" ", "")))
                        continue;
                    
                    //Remove question numbers from raw question string (helps with comparing later)
                    if (!string.IsNullOrEmpty(question.QText))
                    {
                        if (char.IsDigit(question.QText[0]))
                            question.QText = question.QText.Remove(0, 1);

                        if (char.IsDigit(question.QText[0]))
                            question.QText = question.QText.Remove(0, 1);
                    }


                    //Compare with our library of questions
                    bool qFound = false;
                    foreach (var sKey in sectorToQuestions.Keys)
                    {
                        foreach (var questionSet in sectorToQuestions[sKey])
                        {
                            for (int i = 0; i < questionSet.Questions.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(questionSet.Questions[i].QText) && !string.IsNullOrEmpty(question.QText.ToLower()))
                                {
                                    //Prepare strings to be compared
                                    string repoQ = questionSet.Questions[i].QText.ToLower().Replace(" ", "");
                                    string usedQ = question.QText.ToLower().Replace(" ", "");

                                    if (repoQ.Contains(usedQ))
                                    {
                                        questionSet.Questions[i].Used = true;
                                        reusedQCount++;
                                        qFound = true;
                                    }
                                }
                            }
                        }
                    }

                    //If we still haven't found the question, add it to the unused Q set
                    if (!qFound)
                        unusedQuestions.AppendLine(question.QText);
                }
            }

            //Outputting the analysis to help know which questions to study dawg
            Dictionary<int, Tuple<int, int>> qCount = new Dictionary<int, Tuple<int, int>>();
            StringBuilder deltaResults = new StringBuilder();
       
            foreach (var sKey in sectorToQuestions.Keys)
            {
                deltaResults.AppendLine("================" + sKey + "================");

                int sVal = 0;
                int sUsed = 0;
                foreach (var questionSet in sectorToQuestions[sKey])
                {
                    sVal += questionSet.Questions.Count;

                    string txtLine = "Chapter " + questionSet.ChapterId + ": ";

                    for (int i = 0; i < questionSet.Questions.Count; i++)
                    {
                        if (questionSet.Questions[i].Used)
                        {
                            txtLine += questionSet.Questions[i].QuestionNumber.ToString() + ",";
                            sUsed++;
                        }
                    }

                    totalQCount += questionSet.Questions.Count;
                    deltaResults.AppendLine(txtLine);
                }
                qCount[sKey] = new Tuple<int, int>(sVal, sUsed);

                deltaResults.AppendLine("=============================================");
            }

            if (Directory.Exists(OUT_FOLDER) == false)
                Directory.CreateDirectory(OUT_FOLDER);

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Path.Combine(OUT_FOLDER, "Results.txt")))
                sw.WriteLine(deltaResults.ToString());

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Path.Combine(OUT_FOLDER, "UntrackableQs.txt")))
                sw.WriteLine(unusedQuestions.ToString());
        }

        private static string BuildFileName_PDF(string baseFolder_, string fileName_)
        {
            return Path.Combine(baseFolder_, fileName_) + ".pdf";
        }
    }
}