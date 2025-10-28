using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceBooking.API.Middleware;
using ServiceBooking.CrossCutting.DependencyInjection;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});
builder.Services.AddProblemDetails();       //Linha parar criar respostas de exceções no padrão JSON
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Define o esquema de segurança: Explica para o Swagger o que é o Bearer Token
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    });

    // 2. Adiciona o requisito de segurança: Aplica o esquema a todos os endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddInfrastructure(builder.Configuration);      // Adicionando injeção de dependencia da connectionString

builder.Services.AddRateLimiter(options =>
{
    // Define o código de status quando o limite é atingido
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Adiciona uma política de "Janela Fixa" chamada "fixed"
    options.AddFixedWindowLimiter(policyName: "fixed", fixedWindow =>
    {
        fixedWindow.PermitLimit = 100; // Máximo de 100 requisições...
        fixedWindow.Window = TimeSpan.FromMinutes(1); // ...a cada 1 minuto.
        // A fila de espera, se o limite for atingido
        fixedWindow.QueueLimit = 25; // Rejeita imediatamente se o limite for atingido
        fixedWindow.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Ordem da fila
    });
});

var app = builder.Build();

// 1. Obter o DbContext a partir dos serviços configurados
using (var scope = app.Services.CreateScope())              //Migrações automáticas
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceBooking.Infrastructure.Context.AppDbContext>();

    // 2. Tentar aplicar as migrações pendentes
    try
    {
        // Verifica se a base de dados existe e aplica migrações
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrações aplicadas com sucesso."); // Log para vermos no Render
    }
    catch (Exception ex)
    {
        // Logar o erro se a migração falhar (importante para debugging no Render)
        Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
    }
}

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();                    
    app.UseSwaggerUI();
}


app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
