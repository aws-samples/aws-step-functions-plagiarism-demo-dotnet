using System;

namespace IncidentState
{
  public class ExamNotFoundException : Exception
  {
    public ExamNotFoundException()
    { 
    }
    public ExamNotFoundException(string message) : base((string) message)
    {
    }
  }
}
