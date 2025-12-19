using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SeaBattleDB.DB;

public partial class User29Context : DbContext
{
    public User29Context()
    {
    }

    public User29Context(DbContextOptions<User29Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=192.168.200.35;user=user29;database=user29;password=94426;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatorUserId).HasColumnName("creator_user_id");
            entity.Property(e => e.DatetimeLastTurn)
                .HasColumnType("datetime")
                .HasColumnName("datetime_last_turn");
            entity.Property(e => e.DatetimeStartGame)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("datetime_start_game");
            entity.Property(e => e.FieldUser1)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("field_user_1");
            entity.Property(e => e.FieldUser2)
                .HasMaxLength(100)
                .IsFixedLength()
                .HasColumnName("field_user_2");
            entity.Property(e => e.IdUserNextTurn).HasColumnName("id_user_next_turn");
            entity.Property(e => e.IdUserWinner).HasColumnName("id_user_winner");
            entity.Property(e => e.Status)
                .HasComment("0 start, 1 process, 2 end")
                .HasColumnName("status");

            entity.HasMany(d => d.IdUsers).WithMany(p => p.IdGames)
                .UsingEntity<Dictionary<string, object>>(
                    "CrossGameUser",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CrossGameUsers_Users"),
                    l => l.HasOne<Game>().WithMany()
                        .HasForeignKey("IdGame")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CrossGameUsers_Games"),
                    j =>
                    {
                        j.HasKey("IdGame", "IdUser");
                        j.ToTable("CrossGameUsers");
                        j.IndexerProperty<int>("IdGame").HasColumnName("id_game");
                        j.IndexerProperty<int>("IdUser").HasColumnName("id_user");
                    });
            entity.Navigation("IdUsers").AutoInclude();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.Rating).HasColumnName("rating");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
