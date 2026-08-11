// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Plagiarism;

public class IncidentWrapper
{
    public Incident Input { get; set; }
    public string TaskToken { get; set; }
}

public class Incident
{
    public Incident()
    {
    }

    public Incident(string studentId, DateTime incidentDate)
    {
        StudentId = studentId;
        IncidentDate = incidentDate;
        IncidentId = Guid.NewGuid();
        Exams = new List<Exam>();
        IncidentResolved = false;
        AdminActionRequired = false;
        ResolutionDate = null;
    }
    
    public string StudentId { get; set; }

    // Hash key must be declared explicitly: the AWS SDK v4 DynamoDBContext no
    // longer fetches table metadata (DescribeTable) by default.
    [DynamoDBHashKey]
    public Guid IncidentId { get; set; }
    public DateTime IncidentDate { get; set; }
    public List<Exam> Exams { get; set; }
    public DateTime? ResolutionDate { get; set; }
    public bool IncidentResolved { get; set; }
    public bool AdminActionRequired { get; set; }
}

public class Exam
{
    public Exam()
    {
        
    }
    
    private ExamResult _examResult;
    public Guid ExamId { get; set; }
    public DateTime ExamDeadline { get; set; }
    public int Score { get; set; }

    /// <summary>
    /// Minimum score required to pass the exam.
    /// </summary>
    public const int PassMark = 67;

    public ExamResult Result
    {
        get
        {
            if (Score >= PassMark)
            {
                return _examResult = ExamResult.Pass;
            }

            return _examResult = ExamResult.Fail;
        }

        set => _examResult = value;
    }

    public bool NotificationSent { get; set; }

    public Exam(Guid examId, DateTime examDeadline, int score)
    {
        ExamId = examId;
        ExamDeadline = examDeadline;
        Score = score;
        NotificationSent = false;
    }

    public override string ToString()
    {
        return
            $"ExamId: {ExamId}, ExamDate: {ExamDeadline}, Score: {Score}, Result: {Result}, NotificationSent: {NotificationSent}";
    }
}

public enum ExamResult
{
    Pass,
    Fail,
    DidNotSitExam
}