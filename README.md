# ServiceBooking API

![.NET](https://img.shields.io/badge/.NET-9.0-purple?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-blue?logo=csharp)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue?logo=postgresql)
![Status](https://img.shields.io/badge/status-conclu%C3%ADdo-brightgreen)

Um sistema de API RESTful robusto para uma plataforma de agendamento de serviços, construído com .NET 9 e princípios de Arquitetura Limpa (Clean Architecture).

## 🚀 Conceito do Projeto

O **ServiceBooking** é um back-end desenhado para gerir a complexa lógica de agendamentos entre múltiplos prestadores de serviços e clientes. A plataforma permite que clientes (`Users`) agendem serviços (`ServiceOfferings`) oferecidos por diferentes prestadores (`Providers`), respeitando regras de negócio como horários, capacidade concorrente e gestão de estado (pendente, confirmado, cancelado).

## ✨ Funcionalidades Principais

* **Gestão de Autenticação e Utilizadores:**
    * Registo de utilizadores (Clientes) e Provedores (que também são Utilizadores com um perfil `Provider` associado).
    * Autenticação baseada em **JWT (JSON Web Tokens)**.
    * Sistema de `Roles` (Cliente, Provedor, Admin) para autorização de endpoints.
    * Hashing de senhas com `BCrypt`.

* **Gestão de Agendamentos (Bookings):**
    * Clientes podem criar, reagendar e cancelar os seus próprios agendamentos.
    * Sistema de **verificação de conflitos** que respeita a `ConcurrentCapacity` (capacidade de atendimento simultâneo) de cada provedor.
    * Ciclo de vida completo do agendamento com `Status` (Pending, Confirmed, Cancelled, Completed).

* **Gestão de Provedores e Serviços:**
    * Provedores podem gerir o seu perfil e associar/desassociar os serviços que oferecem.
    * Provedores podem consultar a sua própria agenda (os seus agendamentos).
    * Provedores podem `Confirmar` agendamentos pendentes.
    * Admins podem gerir serviços (CRUD de `ServiceOfferings`).

* **Boas Práticas de API:**
    * **Paginação:** Todas as listagens (`GET` de coleções) são paginadas, com metadados enviados no header `X-Pagination`.
    * **Gestão de Erros Global:** Um *middleware* (`GlobalExceptionHandler`) trata de todas as exceções não apanhadas e retorna respostas JSON padronizadas (ex: 409 Conflict para regras de negócio, 401 Unauthorized, 500 Internal Error).
    * **Rate Limiting:** Proteção básica contra abuso de API.
    * **Swagger/OpenAPI:** Documentação de API automática com suporte para autenticação Bearer.

## 🏛️ Arquitetura

O projeto está dividido numa solução (`.sln`) que segue os princípios da Arquitetura Limpa, separando responsabilidades:

* **`ServiceBooking.Domain`**: Contém as entidades de negócio (ex: `Booking`, `User`, `Provider`) e as interfaces dos repositórios (`IRepository`, `IUnitOfWork`).
* **`ServiceBooking.Application`**: Contém a lógica de negócio (Services), DTOs, *mappings* de AutoMapper e interfaces de serviço.
* **`ServiceBooking.Infrastructure`**: Implementa a persistência de dados (Entity Framework Core, Repositórios, Migrations) e serviços externos (ex: `TokenService`).
* **`ServiceBooking.API`**: O ponto de entrada da aplicação. Contém os Controllers, configuração de *middleware* (`Program.cs`) e gestão de erros.
* **`ServiceBooking.CrossCutting`**: Configura a Injeção de Dependência (`DependencyInjectionConfig.cs`).
* **`ServiceBooking.Shared`**: Classes comuns usadas por todos os projetos (ex: `PagedResult`).
* **`ServiceBooking.Application.Tests`**: Testes unitários para a camada de aplicação, usando xUnit e Moq.

## 🛠️ Tecnologias Utilizadas

* **Framework:** .NET 9.0
* **Linguagem:** C# 12
* **API:** ASP.NET Core Web API
* **Base de Dados:** PostgreSQL
* **ORM:** Entity Framework Core
* **Autenticação:** JWT (JSON Web Tokens)
* **Mapeamento:** AutoMapper
* **Testes:** xUnit & Moq
* **Padrões:** Repository Pattern, Unit of Work, Clean Architecture (DDD-lite)

## 🚀 Instalação e Execução

### Pré-requisitos

* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (ou superior)
* Um servidor [PostgreSQL](https://www.postgresql.org/download/) a correr (localmente ou na nuvem).

### Passos

1.  **Clonar o repositório:**
    ```bash
    git clone [https://github.com/SEU-USUARIO/ServiceBooking.git](https://github.com/SEU-USUARIO/ServiceBooking.git)
    cd ServiceBooking
    ```

2.  **Configurar a Base de Dados:**
    Abre o ficheiro `ServiceBooking.API/appsettings.json` (ou `appsettings.Development.json`). Precisas de configurar duas secções:

    * A `DefaultConnection` para o teu servidor PostgreSQL.
    * As definições `Jwt` (SecretKey, Issuer, Audience).

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=ServiceBookingDB;Username=postgres;Password=tua-senha-segura"
      },
      "Jwt": {
        "SecretKey": "tua-chave-secreta-super-longa-e-segura-aqui",
        "Issuer": "ServiceBooking.API",
        "Audience": "ServiceBooking.Users"
      },
      "Logging": { ... }
    }
    ```

3.  **Correr as Migrations:**
    Para criar a estrutura da base de dados, corre o comando de update do EF Core a partir da pasta raiz da solução (onde está o `.sln`) ou da pasta `ServiceBooking.API`:

    ```bash
    # Se estiveres na pasta ServiceBooking.API
    dotnet ef database update
    ```

4.  **Executar o projeto:**
    Navega para a pasta da API e corre a aplicação:

    ```bash
    cd ServiceBooking.API
    dotnet run
    ```

5.  **Aceder à API:**
    A API estará a correr (por defeito) em `http://localhost:5075`.
    Podes aceder à documentação interativa do Swagger em: `http://localhost:5075/swagger`.

## 📚 Documentação da API (Endpoints Principais)

Todos os endpoints que requerem autenticação esperam um `Bearer Token` no *header* `Authorization`.

### Autenticação (`/User`)

* `POST /User/register`: Regista um novo utilizador (Cliente).
* `POST /User/login`: Autentica um utilizador e retorna um JWT.

### Provedores (`/Provider`)

* `POST /Provider`: Regista um novo Provedor (requer dados de Utilizador e de Provedor).
* `GET /Provider`: Lista todos os provedores (paginado).
* `PUT /Provider/{id}/services`: (Auth: Provider) Atualiza a lista de serviços que o provedor oferece.

### Agendamentos (`/Booking`)

* `POST /Booking`: (Auth: User) Cria um novo agendamento.
* `GET /Booking`: (Auth: User) Retorna os agendamentos *do utilizador logado* (paginado).
* `GET /Booking/provider-schedule`: (Auth: Provider) Retorna a agenda *do provedor logado* (paginado).
* `PUT /Booking/{id}`: (Auth: User) Reagenda um agendamento (muda data e/ou provedor).
* `PUT /Booking/{id}/confirm`: (Auth: Provider) Confirma um agendamento que estava pendente.
* `DELETE /Booking/{id}`: (Auth: User/Provider) Cancela um agendamento.

---

## 👨‍💻 Autor

* **[Samuel Gomes]**
* **LinkedIn:** `https://www.linkedin.com/in/samuel-gomes-dev/`
* **GitHub:** `https://github.com/Samuelz47`