using Xunit;
using NSubstitute;
using Plagiarism;
using PlagiarismRepository;
using ScheduleExam;

namespace ScheduleExam.Tests
{
    public class FunctionTests
    {
        private readonly IIncidentRepository _repository;
        private readonly Function _function;

        public FunctionTests()
        {
            // Set env variable for Powertools Metrics 
            Environment.SetEnvironmentVariable("TABLE_NAME", "IncidentsTable");
            Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", "Plagiarism");
            _repository = Substitute.For<IIncidentRepository>();
            _function = new Function(_repository);
        }

        [Fact]
        public void FunctionHandler_ValidIncident_SchedulesExam()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident { IncidentId = incidentId };
            var existingIncident = new Incident { IncidentId = incidentId, Exams = new List<Exam>() };
            _repository.GetIncidentById(incident.IncidentId).Returns(existingIncident);

            // Act
            var result = _function.FunctionHandler(incident, null);

            // Assert
            Assert.Single(result.Exams);
            Assert.Equal(DateTime.Now.Date.AddDays(7), result.Exams[0].ExamDeadline.Date);
            _repository.Received(1).SaveIncident(Arg.Any<Incident>());
        }

        [Fact]
        public void FunctionHandler_IncidentNotFound_ThrowsException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident { IncidentId = incidentId };
            _repository.GetIncidentById(incident.IncidentId).Returns((Incident)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _function.FunctionHandler(incident, null));
        }

        [Fact]
        public void FunctionHandler_ThreeExamsAlreadyTaken_ThrowsException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident { IncidentId = incidentId };
            var existingIncident = new Incident 
            { 
                IncidentId = incidentId, 
                Exams = new List<Exam> { new Exam(), new Exam(), new Exam() }
            };
            _repository.GetIncidentById(incident.IncidentId).Returns(existingIncident);

            // Act & Assert
            Assert.Throws<StudentExceededAllowableExamRetries>(() => _function.FunctionHandler(incident, null));
        }

        [Fact]
        public void FunctionHandler_NullIncident_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _function.FunctionHandler(null, null));
        }

        [Fact]
        public void FunctionHandler_EmptyIncidentId_ThrowsArgumentException()
        {
            // Arrange
            var incident = new Incident { IncidentId = Guid.Empty };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _function.FunctionHandler(incident, null));
        }
    }
}