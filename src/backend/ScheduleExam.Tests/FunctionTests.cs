using Xunit;
using NSubstitute;
using Plagiarism;
using PlagiarismRepository;

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
        public async Task FunctionHandler_ValidIncident_SchedulesExam()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident { IncidentId = incidentId };
            var existingIncident = new Incident { IncidentId = incidentId, Exams = new List<Exam>() };
            _repository.GetIncidentByIdAsync(incident.IncidentId).Returns(existingIncident);

            // Act
            var result = await _function.FunctionHandler(incident, null);

            // Assert
            Assert.Single(result.Exams);
            Assert.Equal(DateTime.UtcNow.Date.AddDays(7), result.Exams[0].ExamDeadline.Date);
            await _repository.Received(1).SaveIncidentAsync(Arg.Any<Incident>());
        }

        [Fact]
        public async Task FunctionHandler_IncidentNotFound_ThrowsException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident { IncidentId = incidentId };
            _repository.GetIncidentByIdAsync(incident.IncidentId).Returns((Incident)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _function.FunctionHandler(incident, null));
        }

        [Fact]
        public async Task FunctionHandler_ThreeExamsAlreadyTaken_ThrowsException()
        {
            // Arrange
            var incidentId = Guid.NewGuid();
            var incident = new Incident { IncidentId = incidentId };
            var existingIncident = new Incident 
            { 
                IncidentId = incidentId, 
                Exams = new List<Exam> { new Exam(), new Exam(), new Exam() }
            };
            _repository.GetIncidentByIdAsync(incident.IncidentId).Returns(existingIncident);

            // Act & Assert
            await Assert.ThrowsAsync<StudentExceededAllowableExamRetries>(() => _function.FunctionHandler(incident, null));
        }

        [Fact]
        public async Task FunctionHandler_NullIncident_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _function.FunctionHandler(null, null));
        }

        [Fact]
        public async Task FunctionHandler_EmptyIncidentId_ThrowsArgumentException()
        {
            // Arrange
            var incident = new Incident { IncidentId = Guid.Empty };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _function.FunctionHandler(incident, null));
        }
    }
}
