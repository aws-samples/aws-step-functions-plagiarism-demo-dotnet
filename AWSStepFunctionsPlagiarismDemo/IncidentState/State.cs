using System;
using System.Collections.Generic;

namespace IncidentState
{
    public class State
    {
        public string StudentId { get; set; }
        public Guid IncidentId { get; set; }
        public DateTime IncidentDate { get; set; }
        public List<Exam> Exams { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public bool IncidentResolved { get; set; }
        public bool AdminActionRequired { get; set; }
    }

    public class Exam
    {
        public Guid ExamId { get; set; }
        public DateTime ExamDate { get; set; }
        public int Score { get; set; }
        public ExamResult Result { get; set; }
        public bool NotifcationSent { get; set; }

        public Exam(Guid examId, DateTime examDate, int score)
        {
            ExamId = examId;
            ExamDate = examDate;
            Score = score;
            NotifcationSent = false;
        }

    }

    public enum ExamResult
    {
        Pass,
        Fail,
        DidNotSitExam
    }

}
