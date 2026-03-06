using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Models;

namespace SalihYildizFinans.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bank> Banks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<DebtTracker> DebtTrackers { get; set; }
        public DbSet<AccountPara> AccountParas { get; set; }
        public DbSet<AccountParaDetail> AccountParaDetails { get; set; }
        public DbSet<FieldAccount> FieldAccounts { get; set; }
        public DbSet<CurrencyAsset> CurrencyAssets { get; set; }
        public DbSet<VehicleTrade> VehicleTrades { get; set; }
    }
}
