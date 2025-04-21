using CreditSystem.Domain;

namespace CreditSystem.Application.Interfaces
{
    public interface ICreditService
    {
        Task<CreditRequest> RequestCreditAsync(string applicantName, string applicantEmail, decimal requestedAmount);
        Task EvaluateCreditRequestAsync(int requestId);
        Task<IEnumerable<CreditRequest>> GetAllCreditRequestsAsync();
        Task<CreditRequest?> GetCreditRequestByIdAsync(int id);
    }
}
