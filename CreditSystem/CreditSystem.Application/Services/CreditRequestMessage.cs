namespace CreditSystem.Application.Services
{
    internal class CreditRequestMessage
    {
        private int requestId;
        private string applicantEmail;
        private decimal amount;

        public CreditRequestMessage(int RequestId, string ApplicantEmail, decimal Amount)
        {
            requestId = RequestId;
            applicantEmail = ApplicantEmail;
            amount = Amount;
        }
    }
}