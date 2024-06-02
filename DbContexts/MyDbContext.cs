using Microsoft.EntityFrameworkCore;
using C_Sharp_lab_4.Models;
namespace C_Sharp_lab_4.DbContexts
{
    public partial class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
        {
        }

        MyDbContext()
        {
            Database.EnsureCreated();
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Message>? Message { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("userdb");
                entity.Property(e => e.Id)
                .HasColumnName("id");
                entity.Property(e => e.Login)
                .HasColumnName("login")
                .HasMaxLength(40);
                entity.Property(e => e.Password)
                .HasColumnName("password")
                .HasMaxLength(40);
                entity.Property(e => e.FIO)
                .HasColumnName("fio")
                .HasMaxLength(120);
            });
            modelBuilder.Entity<Message>(entity => {
                entity.ToTable("messagedb");
                entity.Property(e => e.Id)
                .HasColumnName("id");
                entity.Property(e => e.Id_Sender)
                .HasColumnName("id_sender");
                entity.Property(e => e.Id_Recipient)
                .HasColumnName("id_recipient");
                entity.Property(e => e.Hedder)
                .HasColumnName("hedder");
                entity.Property(e => e.TextMessage)
                .HasColumnName("textmessage");
                entity.Property(e => e.DateDispatch)
                .HasColumnName("datedispatch");
                entity.Property(e => e.Status)
                .HasColumnName("status");
            });

        }
    }
}