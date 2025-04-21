namespace CreditSystem.Domain
{
    // Princípio SOLID: Single Responsibility - Esta classe tem apenas a responsabilidade de representar uma solicitação de crédito
    public class CreditRequest
    {
        public int Id { get; private set; }
        public string ApplicantName { get; private set; }
        public string ApplicantEmail { get; private set; }
        public decimal RequestedAmount { get; private set; }
        public DateTime RequestDate { get; private set; }
        public EnumCreditRequestStatus Status { get; private set; }
        public string? RejectionReason { get; private set; }

        // Princípio SOLID: Open/Closed - O status é controlado por métodos específicos, não por setters públicos
        public void Approve()
        {
            Status = EnumCreditRequestStatus.Approved;
            RejectionReason = null;
        }

        public void Reject(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason cannot be empty");

            Status = EnumCreditRequestStatus.Rejected;
            RejectionReason = reason;
        }

        // Factory method pattern para criação
        public static CreditRequest Create(string applicantName, string applicantEmail, decimal requestedAmount)
        {
            if (string.IsNullOrWhiteSpace(applicantName))
                throw new ArgumentException("Applicant name cannot be empty");

            if (string.IsNullOrWhiteSpace(applicantEmail))
                throw new ArgumentException("Applicant email cannot be empty");

            if (requestedAmount <= 0)
                throw new ArgumentException("Requested amount must be positive");

            return new CreditRequest
            {
                ApplicantName = applicantName,
                ApplicantEmail = applicantEmail,
                RequestedAmount = requestedAmount,
                RequestDate = DateTime.UtcNow,
                Status = EnumCreditRequestStatus.Pending
            };
        }
    }
}
