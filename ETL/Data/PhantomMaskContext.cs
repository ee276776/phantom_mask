using Microsoft.EntityFrameworkCore;
using PhantomMaskETL.Data.Entities;

namespace PhantomMaskETL.Data
{
    public class PhantomMaskContext : DbContext
    {
        public PhantomMaskContext(DbContextOptions<PhantomMaskContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<PharmacyEntity> Pharmacies { get; set; }
        public DbSet<MaskEntity> Masks { get; set; }
        public DbSet<PurchaseEntity> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("users");  // 使用實際的小寫表格名稱
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.CashBalance).HasColumnName("cashbalance").HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.HasIndex(e => e.Name);
            });

            modelBuilder.Entity<PharmacyEntity>(entity =>
            {
                entity.ToTable("pharmacies");  // 使用實際的小寫表格名稱
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.CashBalance).HasColumnName("cashbalance").HasPrecision(10, 2);
                entity.Property(e => e.OpeningHours).HasColumnName("openinghours").HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.HasIndex(e => e.Name);
            });

            modelBuilder.Entity<MaskEntity>(entity =>
            {
                entity.ToTable("masks");  // 使用實際的小寫表格名稱
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
                entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
                entity.Property(e => e.StockQuantity).HasColumnName("stockquantity");
                entity.Property(e => e.PharmacyId).HasColumnName("pharmacyid");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.HasOne(e => e.Pharmacy)
                      .WithMany(p => p.Masks)
                      .HasForeignKey(e => e.PharmacyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PurchaseEntity>(entity =>
            {
                entity.ToTable("purchases");  // 使用實際的小寫表格名稱
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserName).HasColumnName("username").HasMaxLength(255);
                entity.Property(e => e.PharmacyName).HasColumnName("pharmacyname").HasMaxLength(255);
                entity.Property(e => e.MaskName).HasColumnName("maskname").HasMaxLength(255);
                entity.Property(e => e.TransactionAmount).HasColumnName("transactionamount").HasPrecision(10, 2);
                entity.Property(e => e.TransactionQuantity).HasColumnName("transactionquantity");
                entity.Property(e => e.TransactionDatetime).HasColumnName("transactiondatetime");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.HasIndex(e => e.TransactionDatetime);
                entity.HasIndex(e => e.UserName);
                entity.HasIndex(e => e.PharmacyName);
            });
        }
    }
}
