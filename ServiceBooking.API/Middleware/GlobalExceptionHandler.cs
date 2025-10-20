using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ServiceBooking.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1. Logamos a exceção no servidor para futura análise.
        _logger.LogError(exception, "Ocorreu uma exceção não tratada: {Message}", exception.Message);

        // 2. Criamos um objeto padronizado para a resposta de erro.
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        // 3. "Traduzimos" a exceção para uma resposta HTTP apropriada.
        switch (exception)
        {
            // Caso seja um erro de regra de negócio (ex: email duplicado, conflito de horário)
            case InvalidOperationException:
                problemDetails.Title = "Requisição Inválida ou Conflito de Negócio";
                problemDetails.Status = StatusCodes.Status409Conflict; // 409 Conflict
                problemDetails.Detail = exception.Message;
                break;

            // Caso seja um erro de autenticação (ex: senha errada)
            case UnauthorizedAccessException:
                problemDetails.Title = "Não Autorizado";
                problemDetails.Status = StatusCodes.Status401Unauthorized; // 401 Unauthorized
                problemDetails.Detail = exception.Message;
                break;

            // Para qualquer outro erro não esperado
            default:
                problemDetails.Title = "Erro Interno do Servidor";
                problemDetails.Status = StatusCodes.Status500InternalServerError; // 500 Internal Server Error
                problemDetails.Detail = "Ocorreu um erro inesperado no sistema. Por favor, tente novamente.";
                break;
        }

        // 4. Preparamos e enviamos a resposta JSON.
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // 5. Retornamos 'true' para indicar que a exceção foi tratada.
        return true;
    }
}
