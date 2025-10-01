using Microsoft.EntityFrameworkCore;
using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Infrastructure.Context;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }      //Construtor para receber o banco de dados como parametro que será repassado pra o DbContext

    public DbSet<User> Users { get; set; }
    public DbSet<Provider> Providers { get; set; }
    public DbSet<ServiceOffering> ServicesOfferings { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Provider>()
                    .HasMany(p => p.Services) // "Um provedor (p) tem muitos serviços"
                    .WithMany(s => s.Providers); // "E um serviço (s) tem muitos provedores"
        modelBuilder.Entity<Booking>()  // No caso de um para muitos começamos com a entidade "muitos"
                    .HasOne(b => b.User)    // Um agendamento tem um usuário
                    .WithMany()             // E esse usuário tem muitos agendamentos
                    .HasForeignKey(b => b.UserId);  // Chave estrangeira para conectar as duas entidades em Booking
        modelBuilder.Entity<Booking>()
                    .HasOne(b => b.Provider)
                    .WithMany()
                    .HasForeignKey(b => b.ProviderId);
        modelBuilder.Entity<Booking>()
                    .HasOne(b => b.ServiceOffering)
                    .WithMany()
                    .HasForeignKey(b => b.ServiceOfferingId);
    }
}
