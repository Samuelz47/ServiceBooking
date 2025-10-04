using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBooking.Application.Interfaces;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;
using ServiceBooking.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.CrossCutting.DependencyInjection;
public static class DependencyInjectionConfig
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");      // Busca a connection string em appsettings.json
        services.AddDbContext<AppDbContext>(dbContextOptions =>
        {
            dbContextOptions.UseNpgsql(connectionString);               // Passa a string de conexão para o banco do tipo PostgreSQL
        });
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
