// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Tracing;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Metrics;

using Plagiarism;
using SendGrid;
using SendGrid.Helpers.Mail;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SendNotificationTask;

public class Function
{
    private readonly string _apiKey;
    private readonly string _toEmail;
    private readonly string _fromEmail;
    private readonly string _testingCentreUrl;

    public Function()
    {
        Tracing.RegisterForAllServices();

        _apiKey = EnsureEnvironmentVariable("SENDGRID_API_KEY");
        _toEmail = EnsureEnvironmentVariable("TO_EMAIL");
        _fromEmail = EnsureEnvironmentVariable("FROM_EMAIL");
        _testingCentreUrl = EnsureEnvironmentVariable("TESTING_CENTRE_URL");
    }

    private static string EnsureEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(value))
        {
            throw new ApplicationException($"Environment variable '{name}' is not set.");
        }

        return value;
    }


    /// <summary>
    /// Function to send student email about their next scheduled exam.
    /// </summary>
    /// <param name="wrapper">Instance of IncidentWrapper class which contains the task token from Step Functions</param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Logging(LogEvent = true)]
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    public async Task FunctionHandler(IncidentWrapper wrapper, ILambdaContext context)
    {
        var nextExam = wrapper.Input.Exams.FirstOrDefault();
        if (nextExam is null)
        {
            throw new ExamNotFoundException();
        }

        await SendEmailAsync(
            nextExam,
            wrapper.Input.StudentId,
            wrapper.TaskToken,
            wrapper.Input.IncidentId.ToString("D"),
            wrapper.Input.Exams.Count);

        Logger.LogInformation("Successfully delivered exam notification");
    }

    [Tracing(SegmentName = "SendGrid API call")]
    private Task SendEmailAsync(Exam nextExam, string studentId, string token, string incidentId, int examCount)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_fromEmail);
        var to = new EmailAddress(_toEmail);
        var subject = $"AWS Step Functions Plagiarism Demo Exam Notification for {studentId}";
        var plainTextContent = CreatePlainTextContent(nextExam, studentId, token, incidentId, examCount);
        var htmlContent = CreateHtmlContent(nextExam, studentId, token, incidentId, examCount);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        try
        {
            async void Subsegment(Subsegment subsegment)
            {
                var response = await client.SendEmailAsync(msg);
                Logger.LogInformation(response.StatusCode);

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    nextExam.NotificationSent = true;
                }
            }

            Tracing.WithSubsegment("SendGrid", Subsegment);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to send email with message: {e.Message}", e.Message);
            throw;
        }

        return Task.CompletedTask;
    }


    // /// <summary>
    // /// Sends email using SendGrid client
    // /// </summary>
    // /// <param name="nextExam">Exam details</param>
    // /// <param name="studentId">Student Id</param>
    // /// <param name="token">Step Function Callback Token</param>
    // /// <param name="incidentId"></param>
    // /// <param name="examCount"></param>
    // /// <returns></returns>
    // private async Task SendEmail(Exam nextExam, string studentId, string token, string incidentId, int examCount)
    // {
    //     try
    //     {
    //         var client = new SendGridClient(_apiKey);
    //         var from = new EmailAddress(_fromEmail, "AWS Step Functions Plagiarism Demo Administrator");
    //         var subject = $"AWS Step Functions Plagiarism Demo Exam Notification for {studentId}";
    //         var to = new EmailAddress(_toEmail);
    //
    //         var plainTextContent = CreatePlainTextContent(nextExam, studentId, token, incidentId, examCount);
    //
    //         var htmlContent = CreateHtmlContent(nextExam, studentId, token, incidentId, examCount);
    //
    //         var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
    //         Logger.LogInformation(msg.Serialize());
    //
    //         AWSXRayRecorder.Instance.BeginSubsegment("Sendgrid", DateTime.Now);
    //         var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
    //         AWSXRayRecorder.Instance.EndSubsegment();
    //
    //         Logger.LogInformation(response.StatusCode);
    //
    //         if (response.StatusCode == HttpStatusCode.Accepted)
    //         {
    //             nextExam.NotificationSent = true;
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Logger.LogInformation(e);
    //         throw;
    //     }
    // }

    private string CreateHtmlContent(Exam nextExam, string studentId, string token, string incidentId, int examCount)
    {
        var htmlContent =
            $"<p>Dear Student (ID: {studentId}),</p>" +
            "<p>You have been suspected of plagiarism. You must pass an exam, or you will be referred for administrative action.</p>" +
            $"<p>You have until <strong>{nextExam.ExamDeadline}</strong> to complete your Plagiarism Violation exam.</p> " +
            $"<p>This is your <strong>{examCount} of 3</strong> attempts. The passmark is 70%.</p>" +
            "<p>Thank you.</p>" +
            $"<p><a href=\"{_testingCentreUrl}?TaskToken={token}&ExamId={nextExam.ExamId}&IncidentId={incidentId}\"><strong>Click here to start your exam</strong></a></p>" +
            $"<p>If the URL does not work, copy and paste this into the address bar of your browser <br/> {_testingCentreUrl}?TaskToken={token}&ExamId={nextExam.ExamId}&IncidentId={incidentId}</p>";
        return htmlContent;
    }

    private string CreatePlainTextContent(Exam nextExam, string studentId, string token, string incidentId,
        int examCount)
    {
        var plainTextContent =
            $"Dear Student (ID: {studentId})," +
            $"\n" +
            "You have been suspected of plagiarism. You must pass an exam, or you will be referred for administrative action." +
            "\n" +
            $"You have until {nextExam.ExamDeadline} to complete your Plagiarism Violation exam." +
            "\n" +
            $"This is your {examCount} of 3 attempts. The passmark is 70%." +
            "\n" +
            "Thank you." +
            "\n" +
            "Please copy and paste this link into your browser to start your exam." +
            "\n" +
            $"{_testingCentreUrl}?TaskToken={token}&ExamId={nextExam.ExamId}&IncidentId={incidentId}";
        return plainTextContent;
    }
}