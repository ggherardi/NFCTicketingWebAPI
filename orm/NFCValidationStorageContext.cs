﻿using System;
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

        public virtual DbSet<SmartTicket> SmartTickets { get; set; }
        public virtual DbSet<SmartTicketUser> SmartTicketUsers { get; set; }
        public virtual DbSet<Validation> Validations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

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

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SmartTickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_SmartTicket_SmartTicketUser");
            });

            modelBuilder.Entity<SmartTicketUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("SmartTicketUser");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.CreationTime)
                    .IsRequired()
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .HasColumnName("creation_time");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("password")
                    .IsFixedLength(true);

                entity.Property(e => e.Surname)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("surname");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("username");
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