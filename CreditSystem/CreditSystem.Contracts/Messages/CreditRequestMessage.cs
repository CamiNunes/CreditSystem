namespace CreditSystem.Contracts.Messages;

public record CreditRequestMessage(
    int RequestId,
    string ApplicantEmail,
    decimal Amount
);