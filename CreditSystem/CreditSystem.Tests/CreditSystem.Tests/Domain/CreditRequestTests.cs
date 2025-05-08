using CreditSystem.Domain;

namespace CreditSystem.Tests.Domain
{
    public class CreditRequestTests
    {
        [Fact]
        public void Approve_ShouldSetStatusToApproved_AndClearRejectionReason()
        {
            // Arrange
            var creditRequest = CreditRequest.Create("John Doe", "john.doe@example.com", 1000m);

            // Act
            creditRequest.Approve();

            // Assert
            Assert.Equal(EnumCreditRequestStatus.Approved, creditRequest.Status);
            Assert.Null(creditRequest.RejectionReason);
        }

        [Fact]
        public void Reject_ShouldSetStatusToRejected_AndSetRejectionReason()
        {
            // Arrange
            var creditRequest = CreditRequest.Create("John Doe", "john.doe@example.com", 1000m);
            var rejectionReason = "Insufficient credit score";

            // Act
            creditRequest.Reject(rejectionReason);

            // Assert
            Assert.Equal(EnumCreditRequestStatus.Rejected, creditRequest.Status);
            Assert.Equal(rejectionReason, creditRequest.RejectionReason);
        }

        [Fact]
        public void Reject_ShouldThrowArgumentException_WhenReasonIsEmpty()
        {
            // Arrange
            var creditRequest = CreditRequest.Create("John Doe", "john.doe@example.com", 1000m);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => creditRequest.Reject(string.Empty));
        }

        [Fact]
        public void Create_ShouldReturnNewCreditRequest_WithValidParameters()
        {
            // Arrange
            var applicantName = "John Doe";
            var applicantEmail = "john.doe@example.com";
            var requestedAmount = 1000m;

            // Act
            var creditRequest = CreditRequest.Create(applicantName, applicantEmail, requestedAmount);

            // Assert
            Assert.Equal(applicantName, creditRequest.ApplicantName);
            Assert.Equal(applicantEmail, creditRequest.ApplicantEmail);
            Assert.Equal(requestedAmount, creditRequest.RequestedAmount);
            Assert.Equal(EnumCreditRequestStatus.Pending, creditRequest.Status);
            Assert.True((DateTime.UtcNow - creditRequest.RequestDate).TotalSeconds < 1); // Ensure the date is recent
        }

        [Theory]
        [InlineData(null, "john.doe@example.com", 1000)]
        [InlineData("", "john.doe@example.com", 1000)]
        [InlineData("John Doe", null, 1000)]
        [InlineData("John Doe", "", 1000)]
        [InlineData("John Doe", "john.doe@example.com", 0)]
        [InlineData("John Doe", "john.doe@example.com", -100)]
        public void Create_ShouldThrowArgumentException_WithInvalidParameters(string applicantName, string applicantEmail, decimal requestedAmount)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CreditRequest.Create(applicantName, applicantEmail, requestedAmount));
        }
    }
}