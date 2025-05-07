using CreditSystem.Application.Interfaces;
using CreditSystem.Application.Services;
using CreditSystem.Domain;
using CreditSystem.Domain.ValueObjects;
using Moq;

namespace CreditSystem.Application.Tests.Services
{
    public class CreditServiceTests
    {
        private readonly Mock<IRepository<CreditRequest>> _repositoryMock;
        private readonly Mock<ICreditScoreProvider> _creditScoreProviderMock;
        private readonly Mock<IMessagingService> _messagingServiceMock;
        private readonly CreditService _creditService;

        public CreditServiceTests()
        {
            _repositoryMock = new Mock<IRepository<CreditRequest>>();
            _creditScoreProviderMock = new Mock<ICreditScoreProvider>();
            _messagingServiceMock = new Mock<IMessagingService>();
            _creditService = new CreditService(
                _repositoryMock.Object,
                _creditScoreProviderMock.Object,
                _messagingServiceMock.Object
            );
        }

        [Fact]
        public async Task RequestCreditAsync_ShouldAddCreditRequestAndPublishMessage()
        {
            // Arrange
            var applicantName = "John Doe";
            var applicantEmail = "john.doe@example.com";
            var requestedAmount = 5000m;
            var creditRequest = CreditRequest.Create(applicantName, applicantEmail, requestedAmount);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<CreditRequest>()))
                .Returns(Task.CompletedTask);

            _messagingServiceMock.Setup(m => m.PublishMessageAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _creditService.RequestCreditAsync(applicantName, applicantEmail, requestedAmount);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicantName, result.ApplicantName);
            Assert.Equal(applicantEmail, result.ApplicantEmail);
            Assert.Equal(requestedAmount, result.RequestedAmount);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<CreditRequest>()), Times.Once);
            _messagingServiceMock.Verify(m => m.PublishMessageAsync("credit-requests", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task EvaluateCreditRequestAsync_ShouldApproveRequest_WhenConditionsAreMet()
        {
            // Arrange
            var requestId = 1;
            var creditRequest = CreditRequest.Create("John Doe", "john.doe@example.com", 5000m);
            // Não altere o estado da solicitação aqui, ele já será "Pending" por padrão.

            _repositoryMock.Setup(r => r.GetByIdAsync(requestId))
                .ReturnsAsync(creditRequest);

            _creditScoreProviderMock.Setup(c => c.GetCreditScoreAsync(creditRequest.ApplicantEmail))
                .ReturnsAsync(new CreditScore(750)); // Good credit score

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<CreditRequest>()))
                .Returns(Task.CompletedTask);

            _messagingServiceMock.Setup(m => m.PublishMessageAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            await _creditService.EvaluateCreditRequestAsync(requestId);

            // Assert
            Assert.Equal(EnumCreditRequestStatus.Approved, creditRequest.Status);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditRequest>()), Times.Once);
            _messagingServiceMock.Verify(m => m.PublishMessageAsync("credit-decisions", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task EvaluateCreditRequestAsync_ShouldRejectRequest_WhenConditionsAreNotMet()
        {
            // Arrange
            var requestId = 1;
            var creditRequest = CreditRequest.Create("John Doe", "john.doe@example.com", 15000m);
            // Não altere o estado da solicitação aqui, ele já será "Pending" por padrão.

            _repositoryMock.Setup(r => r.GetByIdAsync(requestId))
                .ReturnsAsync(creditRequest);

            _creditScoreProviderMock.Setup(c => c.GetCreditScoreAsync(creditRequest.ApplicantEmail))
                .ReturnsAsync(new CreditScore(600)); // Fair credit score

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<CreditRequest>()))
                .Returns(Task.CompletedTask);

            _messagingServiceMock.Setup(m => m.PublishMessageAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            await _creditService.EvaluateCreditRequestAsync(requestId);

            // Assert
            Assert.Equal(EnumCreditRequestStatus.Rejected, creditRequest.Status);
            Assert.NotNull(creditRequest.RejectionReason);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CreditRequest>()), Times.Once);
            _messagingServiceMock.Verify(m => m.PublishMessageAsync("credit-decisions", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCreditRequestsAsync_ShouldReturnAllRequests()
        {
            // Arrange
            var creditRequests = new List<CreditRequest>
            {
                CreditRequest.Create("John Doe", "john.doe@example.com", 5000m),
                CreditRequest.Create("Jane Smith", "jane.smith@example.com", 10000m)
            };

            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(creditRequests);

            // Act
            var result = await _creditService.GetAllCreditRequestsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCreditRequestByIdAsync_ShouldReturnRequest_WhenFound()
        {
            // Arrange
            var requestId = 1;
            var creditRequest = CreditRequest.Create("John Doe", "john.doe@example.com", 5000m);

            _repositoryMock.Setup(r => r.GetByIdAsync(requestId))
                .ReturnsAsync(creditRequest);

            // Act
            var result = await _creditService.GetCreditRequestByIdAsync(requestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result?.ApplicantName);

            _repositoryMock.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        }

        [Fact]
        public async Task GetCreditRequestByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var requestId = 1;

            _repositoryMock.Setup(r => r.GetByIdAsync(requestId))
                .ReturnsAsync((CreditRequest?)null);

            // Act
            var result = await _creditService.GetCreditRequestByIdAsync(requestId);

            // Assert
            Assert.Null(result);

            _repositoryMock.Verify(r => r.GetByIdAsync(requestId), Times.Once);
        }
    }
}
