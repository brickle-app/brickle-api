using BricklePlatform.Domain.Entities;
using BricklePlatform.Infrastructure.Entities;
using BricklePlatform.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BricklePlatform.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Leasing> Leasings { get; set; }
    public DbSet<UserLeasingAgreement> UserLeasingAgreements { get; set; }
    public DbSet<UserContact> UserContacts { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Investment> Investments { get; set; }
    public DbSet<UserBankAccount> UserBankAccounts { get; set; }
    public DbSet<UserDocument> UserDocuments { get; set; }
    public DbSet<WalletBackup> WalletBackups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new LeasingConfiguration());
        modelBuilder.ApplyConfiguration(new UserLeasingAgreementConfiguration());
        modelBuilder.ApplyConfiguration(new UserContactConfiguration());
        modelBuilder.ApplyConfiguration(new ApiKeyConfiguration());
        modelBuilder.ApplyConfiguration(new CampaignConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentConfiguration());
        modelBuilder.ApplyConfiguration(new UserBankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new UserDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new WalletBackupConfiguration());

        modelBuilder.Entity<User>().ToTable("User", schema: "dbo");
        modelBuilder.Entity<Company>().ToTable("Company", schema: "dbo");
        modelBuilder.Entity<Leasing>().ToTable("Leasing", schema: "dbo");
        modelBuilder.Entity<UserLeasingAgreement>().ToTable("UserLeasingAgreement", schema: "dbo");
        modelBuilder.Entity<UserContact>().ToTable("UserContact", schema: "dbo");
        modelBuilder.Entity<ApiKey>().ToTable("Keys", schema: "dbo");
        modelBuilder.Entity<Campaign>().ToTable("Campaign", schema: "dbo");
        modelBuilder.Entity<Investment>().ToTable("Investment", schema: "dbo");
        modelBuilder.Entity<UserBankAccount>().ToTable("UserBankAccount", schema: "dbo");
        modelBuilder.Entity<UserDocument>().ToTable("UserDocument", schema: "dbo");
        modelBuilder.Entity<WalletBackup>().ToTable("WalletBackup", schema: "dbo");

        base.OnModelCreating(modelBuilder);
    }
}
