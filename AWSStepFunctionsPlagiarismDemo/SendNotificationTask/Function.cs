using System;
using System.Linq;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using IncidentState;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SendNotificationTask
{
  public class Function
  {
    private readonly IAmazonSimpleNotificationService _simpleNotificationService;
/*
    private AmazonSimpleNotificationServiceClient _client;
*/
    private string _topicArn;

    public Function()
    {
      AWSSDKHandler.RegisterXRayForAllServices();
      _simpleNotificationService = new AmazonSimpleNotificationServiceClient();
      _topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN");
    }
    
    public Function(IAmazonSimpleNotificationService simpleNotificationService)
    {
      _simpleNotificationService = simpleNotificationService;
    }

    /// <summary>
    /// Function to send student email about their next scheduled exam.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public State FunctionHandler(State state, ILambdaContext context)
    {

      var nextExam = state.Exams.FirstOrDefault();

      if (nextExam != null)
      {
        var message = $"Dear Student ID {state.StudentId}, you have until {nextExam.ExamDate} to complete you Plagiarism Violation test. Thank you.";
        var subject = $"Exam Notification for {state.StudentId}";

        var response = _simpleNotificationService.PublishAsync(new PublishRequest
        {
          Subject = subject,
          Message = message,
          TopicArn = _topicArn
        });

        if (response.Result.HttpStatusCode == HttpStatusCode.OK)
        {
          nextExam.NotifcationSent = true;
        }
        
      }
      else
      {
        throw new ExamNotFoundException();
      }

      return state;
    }
  }
}
