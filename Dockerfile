# Estágio 1: Build (Compilação)
# Usamos a imagem oficial do SDK do .NET 9 para compilar o projeto
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os ficheiros de projeto (.sln e .csproj) primeiro para otimizar o cache
COPY *.sln .
COPY ServiceBooking.API/*.csproj ServiceBooking.API/
COPY ServiceBooking.Application/*.csproj ServiceBooking.Application/
COPY ServiceBooking.Domain/*.csproj ServiceBooking.Domain/
COPY ServiceBooking.Infrastructure/*.csproj ServiceBooking.Infrastructure/
COPY ServiceBooking.CrossCutting/*.csproj ServiceBooking.CrossCutting/
COPY ServiceBooking.Shared/*.csproj ServiceBooking.Shared/
COPY ServiceBooking.Application.Tests/*.csproj ServiceBooking.Application.Tests/
COPY ServiceBooking.API.Tests/*.csproj ServiceBooking.API.Tests/
# (Nota: Os projetos de teste não são necessários para o deploy)

# Restaura as dependências
RUN dotnet restore "ServiceBooking.sln"

# Copia todo o resto do código-fonte
COPY . .

# Publica a aplicação API em modo Release
WORKDIR "/src/ServiceBooking.API"
RUN dotnet publish "ServiceBooking.API.csproj" -c Release -o /app/publish

# ---

# Estágio 2: Final (Execução)
# Usamos a imagem "aspnet" que é mais leve e segura (só tem o runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# O Render espera que a aplicação oiça na porta 8080
# A imagem base do ASP.NET já está configurada para ouvir na porta 8080 ou 80
EXPOSE 8080

# Comando para iniciar a API
ENTRYPOINT ["dotnet", "ServiceBooking.API.dll"]