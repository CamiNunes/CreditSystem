namespace CreditSystem.Contracts.DTOs;

// Princípio SOLID: Single Responsibility - Representa apenas os dados necessários para uma solicitação de crédito
public record CreditRequestDto
{
    public string ApplicantName { get; init; } = string.Empty;
    public string ApplicantEmail { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
}