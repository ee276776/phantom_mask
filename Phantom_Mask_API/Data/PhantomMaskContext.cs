using Microsoft.EntityFrameworkCore;
using PhantomMaskAPI.Models.Entities;

namespace PhantomMaskAPI.Data
{
    public class PhantomMaskContext : DbContext
    {
        public PhantomMaskContext(DbContextOptions<PhantomMaskContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Pharmacy> Pharmacies { get; set; } = null!;
        public DbSet<Mask> Masks { get; set; } = null!;
        public DbSet<Purchase> Purchases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User 配置
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.CashBalance).HasColumnName("cashbalance").HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            });

            // Pharmacy 配置
            modelBuilder.Entity<Pharmacy>(entity =>
            {
                entity.ToTable("pharmacies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.CashBalance).HasColumnName("cashbalance").HasColumnType("decimal(18,2)");
                entity.Property(e => e.OpeningHours).HasColumnName("openinghours").HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                
                // 一對多關係：一個藥局有多個口罩
                entity.HasMany(e => e.Masks)
                      .WithOne(m => m.Pharmacy)
                      .HasForeignKey(m => m.PharmacyId);
            });

            // Mask 配置
            modelBuilder.Entity<Mask>(entity =>
            {
                entity.ToTable("masks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
                entity.Property(e => e.StockQuantity).HasColumnName("stockquantity");
                entity.Property(e => e.PharmacyId).HasColumnName("pharmacyid");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            });

            // Purchase 配置
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.ToTable("purchases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserName).HasColumnName("username").HasMaxLength(100).IsRequired();
                entity.Property(e => e.PharmacyName).HasColumnName("pharmacyname").HasMaxLength(100).IsRequired();
                entity.Property(e => e.MaskName).HasColumnName("maskname").HasMaxLength(100).IsRequired();
                entity.Property(e => e.TransactionQuantity).HasColumnName("transactionquantity");
                entity.Property(e => e.TransactionAmount).HasColumnName("transactionamount").HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransactionDateTime).HasColumnName("transactiondatetime");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
