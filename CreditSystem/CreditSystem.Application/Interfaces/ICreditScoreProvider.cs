using CreditSystem.Domain.ValueObjects;
using System.Threading.Tasks;

namespace CreditSystem.Application.Interfaces;

// Princípio SOLID: Interface Segregation - Interface específica para obtenção de score de crédito
public interface ICreditScoreProvider
{
    Task<CreditScore> GetCreditScoreAsync(string applicantEmail);
}