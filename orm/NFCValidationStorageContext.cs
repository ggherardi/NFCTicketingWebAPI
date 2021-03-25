using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class NFCValidationStorageContext : DbContext
    {
        public NFCValidationStorageContext()
        {
        }

        public NFCValidationStorageContext(DbContextOptions<NFCValidationStorageContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CreditTransaction> CreditTransactions { get; set; }
        public virtual DbSet<SmartTicket> SmartTickets { get; set; }
        public virtual DbSet<SmartTicketUser> SmartTicketUsers { get; set; }
        public virtual DbSet<Validation> Validations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<CreditTransaction>(entity =>
            {
                entity.ToTable("CreditTransaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasColumnType("smallmoney")
                    .HasColumnName("amount");

                entity.Property(e => e.CardId)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("card_id")
                    .IsFixedLength(true);

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Location).HasColumnName("location");

                entity.HasOne(d => d.Card)
                    .WithMany(p => p.CreditTransactions)
                    .HasForeignKey(d => d.CardId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CreditTransaction_SmartTicket");
            });

            modelBuilder.Entity<SmartTicket>(entity =>
            {
                entity.HasKey(e => e.CardId);

                entity.ToTable("SmartTicket");

                entity.Property(e => e.CardId)
                    .HasMaxLength(150)
                    .HasColumnName("card_id")
                    .IsFixedLength(true);

                entity.Property(e => e.Credit)
                    .HasColumnType("smallmoney")
                    .HasColumnName("credit");

                entity.Property(e => e.CurrentValidation)
                    .HasColumnType("datetime")
                    .HasColumnName("current_validation");

                entity.Property(e => e.SessionExpense)
                    .HasColumnType("smallmoney")
                    .HasColumnName("session_expense");

                entity.Property(e => e.SessionValidation)
                    .HasColumnType("datetime")
                    .HasColumnName("session_validation");

                entity.Property(e => e.TicketType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ticket_type");

                entity.Property(e => e.UsageTimestamp)
                    .HasColumnType("datetime")
                    .HasColumnName("usage_timestamp");

                entity.Property(e => e.Username)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("username");

                entity.Property(e => e.Virtual).HasColumnName("virtual");

                entity.HasOne(d => d.UsernameNavigation)
                    .WithMany(p => p.SmartTickets)
                    .HasForeignKey(d => d.Username)
                    .HasConstraintName("FK_SmartTicket_SmartTicketUser");
            });

            modelBuilder.Entity<SmartTicketUser>(entity =>
            {
                entity.HasKey(e => e.Username)
                    .HasName("PK_SmartTicketUser_1");

                entity.ToTable("SmartTicketUser");

                entity.Property(e => e.Username)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("username");

                entity.Property(e => e.CreationTime)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("creation_time");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Surname)
                    .IsUnicode(false)
                    .HasColumnName("surname");
            });

            modelBuilder.Entity<Validation>(entity =>
            {
                entity.HasKey(e => new { e.CardId, e.EncryptedTicket });

                entity.ToTable("Validation");

                entity.Property(e => e.CardId)
                    .HasMaxLength(150)
                    .HasColumnName("card_id")
                    .IsFixedLength(true);

                entity.Property(e => e.EncryptedTicket)
                    .HasMaxLength(150)
                    .HasColumnName("encrypted_ticket");

                entity.Property(e => e.Location).HasColumnName("location");

                entity.Property(e => e.ValidationTime)
                    .HasColumnType("datetime")
                    .HasColumnName("validation_time");

                entity.HasOne(d => d.Card)
                    .WithMany(p => p.Validations)
                    .HasForeignKey(d => d.CardId)
                    .HasConstraintName("FK_Validation_SmartTicket");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
