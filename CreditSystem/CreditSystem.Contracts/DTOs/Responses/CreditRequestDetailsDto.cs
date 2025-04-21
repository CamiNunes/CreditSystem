namespace CreditSystem.Contracts.DTOs.Responses;

// DTO detalhado para consulta de solicitação
public record CreditRequestDetailsDto
{
    public int Id { get; init; }
    public string ApplicantName { get; init; } = string.Empty;
    public string ApplicantEmail { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public DateTime RequestDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
}