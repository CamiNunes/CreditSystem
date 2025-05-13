using CreditSystem.Application.Interfaces;
using CreditSystem.Application.Services;
using CreditSystem.Domain;
using CreditSystem.Domain.ValueObjects;
using CreditSystem.Infrastructure.Data;
using CreditSystem.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CreditSystem.Tests.Integration
{
    public class CreditServiceIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICreditService _creditService;

        public CreditServiceIntegrationTests()
        {
            // Setup in-memory SQLite database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(new SqliteConnection("DataSource=:memory:"))
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.OpenConnection();
            _dbContext.Database.EnsureCreated();

            // Mock dependencies
            var repository = new Repository<CreditRequest>(_dbContext);

            var creditScoreProviderMock = new Mock<ICreditScoreProvider>();
            creditScoreProviderMock
                .Setup(provider => provider.GetCreditScoreAsync(It.IsAny<string>()))
                .ReturnsAsync(new CreditScore(750));

            var messagingServiceMock = new Mock<IMessagingService>();
            messagingServiceMock
                .Setup(service => service.PublishMessageAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Inject dependencies into the service
            _creditService = new CreditService(repository, creditScoreProviderMock.Object, messagingServiceMock.Object);
        }

        [Fact]
        public async Task RequestCreditAsync_ShouldCreateNewCreditRequest()
        {
            // Arrange
            var applicantName = "John Doe";
            var applicantEmail = "john.doe@example.com";
            var requestedAmount = 1000m;

            // Act
            var creditRequest = await _creditService.RequestCreditAsync(applicantName, applicantEmail, requestedAmount);

            // Assert
            Assert.NotNull(creditRequest);
            Assert.Equal(applicantName, creditRequest.ApplicantName);
            Assert.Equal(applicantEmail, creditRequest.ApplicantEmail);
            Assert.Equal(requestedAmount, creditRequest.RequestedAmount);
            Assert.Equal(EnumCreditRequestStatus.Pending, creditRequest.Status);

            // Verify it was saved in the database
            var savedRequest = await _dbContext.CreditRequests.FindAsync(creditRequest.Id);
            Assert.NotNull(savedRequest);
        }

        public void Dispose()
        {
            _dbContext.Database.CloseConnection();
            _dbContext.Dispose();
        }
    }
}