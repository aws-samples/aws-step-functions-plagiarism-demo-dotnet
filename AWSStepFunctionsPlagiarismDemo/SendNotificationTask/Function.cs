using System;
using System.Linq;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using IncidentState;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SendNotificationTask
{
  public class Function
  {
    private AmazonSimpleNotificationServiceClient _client;
    private string _topicArn;

    public Function()
    {
      _client = new AmazonSimpleNotificationServiceClient(RegionEndpoint.APSoutheast2);
      _topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN");
    }

    /// <summary>
    /// Function to send student email about their next scheduled exam.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public State FunctionHandler(State state, ILambdaContext context)
    {

      var nextExam = state.Exams.LastOrDefault();

      if (nextExam != null)
      {
        var message = $"Dear Student ID {state.StudentId}, you have until {nextExam.ExamDate} to complete you Plagiarism Violation test. Thank you.";
        var subject = $"Exam Notification for {state.StudentId}";

        _client.PublishAsync(new PublishRequest
        {
          Subject = subject,
          Message = message,
          TopicArn = _topicArn
        });

      }
      else
      {
        throw new ExamNotFoundException();
      }



      return state;
    }
  }
}
