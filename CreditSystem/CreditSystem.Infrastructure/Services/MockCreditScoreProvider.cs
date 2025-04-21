using CreditSystem.Application.Interfaces;
using CreditSystem.Domain.ValueObjects;
using System.Threading.Tasks;

namespace CreditSystem.Infrastructure.Services;

// Princípio SOLID: Dependency Inversion - Implementa ICreditScoreProvider
public class MockCreditScoreProvider : ICreditScoreProvider
{
    // Em um sistema real, isso chamaria um serviço externo de score de crédito
    public async Task<CreditScore> GetCreditScoreAsync(string applicantEmail)
    {
        // Lógica mockada - em produção, substituir por chamada real a serviço de crédito
        await Task.Delay(100); // Simula latência de rede

        // Gera um score baseado no hash do email (apenas para demonstração)
        var hash = Math.Abs(applicantEmail.GetHashCode());
        var score = 300 + (hash % 550); // Score entre 300-850

        return new CreditScore(score);
    }
}