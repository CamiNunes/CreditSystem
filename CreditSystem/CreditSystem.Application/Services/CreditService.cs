using CreditSystem.Application.Interfaces;
using CreditSystem.Domain;

namespace CreditSystem.Application.Services;

// Princípio SOLID: Single Responsibility - Lida apenas com a lógica de negócios relacionada a crédito
public class CreditService : ICreditService
{
    private readonly IRepository<CreditRequest> _repository;
    private readonly ICreditScoreProvider _creditScoreProvider;
    private readonly IMessagingService _messagingService;

    // Princípio SOLID: Dependency Injection - Injetamos as dependências
    public CreditService(
        IRepository<CreditRequest> repository,
        ICreditScoreProvider creditScoreProvider,
        IMessagingService messagingService)
    {
        _repository = repository;
        _creditScoreProvider = creditScoreProvider;
        _messagingService = messagingService;
    }

    public async Task<CreditRequest> RequestCreditAsync(string applicantName, string applicantEmail, decimal requestedAmount)
    {
        var creditRequest = CreditRequest.Create(applicantName, applicantEmail, requestedAmount);
        await _repository.AddAsync(creditRequest);

        // Princípio SOLID: Open/Closed - Podemos adicionar novos comportamentos sem modificar esta classe
        await _messagingService.PublishMessageAsync("credit-requests", new
        {
            RequestId = creditRequest.Id,
            ApplicantEmail = creditRequest.ApplicantEmail,
            Amount = creditRequest.RequestedAmount
        });

        return creditRequest;
    }

    public async Task EvaluateCreditRequestAsync(int requestId)
    {
        var request = await _repository.GetByIdAsync(requestId);
        if (request == null)
            throw new ArgumentException("Credit request not found");

        if (request.Status != EnumCreditRequestStatus.Pending)
            throw new InvalidOperationException("Credit request has already been processed");

        var creditScore = await _creditScoreProvider.GetCreditScoreAsync(request.ApplicantEmail);

        // Lógica simples de avaliação
        if (creditScore.IsGood() && request.RequestedAmount <= 10000)
        {
            request.Approve();
        }
        else if (creditScore.IsFair() && request.RequestedAmount <= 5000)
        {
            request.Approve();
        }
        else
        {
            request.Reject("Credit score or requested amount didn't meet requirements");
        }

        await _repository.UpdateAsync(request);

        // Notificar sobre a decisão
        await _messagingService.PublishMessageAsync("credit-decisions", new
        {
            RequestId = request.Id,
            ApplicantEmail = request.ApplicantEmail,
            Status = request.Status.ToString(),
            Amount = request.RequestedAmount,
            RejectionReason = request.RejectionReason
        });
    }

    public async Task<IEnumerable<CreditRequest>> GetAllCreditRequestsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CreditRequest?> GetCreditRequestByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}