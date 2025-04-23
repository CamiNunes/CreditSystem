using CreditSystem.Domain;
using Microsoft.EntityFrameworkCore;

namespace CreditSystem.Infrastructure.Data;

// Princípio SOLID: Single Responsibility - Responsável apenas pelo acesso ao banco de dados
public class ApplicationDbContext : DbContext
{
    // Construtor para runtime (DI)
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }

    // Construtor sem parâmetros para migrations (OBRIGATÓRIO)
    public ApplicationDbContext()
        : base(new DbContextOptionsBuilder<ApplicationDbContext>().Options)
    {
    }

    public DbSet<CreditRequest> CreditRequests { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações do modelo usando Fluent API
        modelBuilder.Entity<CreditRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicantName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ApplicantEmail)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.RequestedAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.RequestDate)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500);
        });

        // Princípio SOLID: Open/Closed - Podemos adicionar novas configurações sem modificar o código existente
        // através do método OnModelCreating
    }
}