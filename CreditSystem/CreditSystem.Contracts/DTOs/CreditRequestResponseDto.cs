namespace CreditSystem.Contracts.DTOs;

// DTO para resposta da solicitação de crédito
public record CreditRequestResponseDto
{
    public int RequestId { get; init; }
    public string Status { get; init; } = string.Empty;
}