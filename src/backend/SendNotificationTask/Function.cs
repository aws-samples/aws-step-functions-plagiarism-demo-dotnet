using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Plagiarism;
using SendGrid;
using SendGrid.Helpers.Mail;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SendNotificationTask
{
    public class Function
    {
        private readonly string _apiKey;
        private readonly string _toEmail;
        private readonly string _fromEmail;
        private readonly string _testingCentreUrl;

        public Function()
        {
            _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            _toEmail = Environment.GetEnvironmentVariable("TO_EMAIL");
            _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
            _testingCentreUrl = Environment.GetEnvironmentVariable("TESTING_CENTRE_URL");

            AWSSDKHandler.RegisterXRayForAllServices();
        }

        /// <summary>
        /// Function to send student email about their next scheduled exam.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(IncidentWrapper wrapper, ILambdaContext context)
        {
            var nextExam = wrapper.Input.Exams.FirstOrDefault();
            var studentId = wrapper.Input.StudentId;
            var token = wrapper.TaskToken;
            var incidentId = wrapper.Input.IncidentId.ToString("D");
            
            if (nextExam != null)
            {
                SendEmail(nextExam, studentId, token, incidentId).Wait();
                context.Logger.Log("Done");
            }
            else
            {
                throw new ExamNotFoundException();
            }
        }

        /// <summary>
        /// Sends email using SendGrid client
        /// </summary>
        /// <param name="nextExam">Exam details</param>
        /// <param name="studentId">Student Id</param>
        /// <param name="token">Step Function Callback Token</param>
        /// <param name="incidentId"></param>
        /// <returns></returns>
        private async Task SendEmail(Exam nextExam, string studentId, string token, string incidentId)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_fromEmail, "AWS Step Functions Plagiarism Demo Administrator");
                var subject = $"AWS Step Functions Plagiarism Demo Exam Notification for {studentId}";
                var to = new EmailAddress(_toEmail);

                var plainTextContent =
                    $"Dear Student (ID: {studentId}), you have until {nextExam.ExamDeadline} to complete your Plagiarism Violation exam. Thank you." +
                    "\n" +
                    "Please copy and paste this link into your browser to start your exam." +
                    "\n" +
                    $"http://localhost:3000?TaskToken={token}&ExamId={nextExam.ExamId}&IncidentId={incidentId}";

                var htmlContent =
                    $"<p>Dear Student (ID: {studentId}),</p>" +
                    $"<p>you have until <strong>{nextExam.ExamDeadline}</strong> to complete your Plagiarism Violation exam.</p> " +
                    "<p>Thank you.</p>" +
                    $"<p><a href=\"{_testingCentreUrl}?TaskToken={token}&ExamId={nextExam.ExamId}&IncidentId={incidentId}\"><strong>Click here to start your exam</strong></a></p>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                Console.WriteLine(msg.Serialize());
                
                AWSXRayRecorder.Instance.BeginSubsegment("Sendgrid", DateTime.Now);
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
                AWSXRayRecorder.Instance.EndSubsegment();

                Console.WriteLine(response.StatusCode);

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    nextExam.NotificationSent = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}