using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Exceptions;

namespace MyCheckpoint.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Acceso denegado", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflicto", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Error inesperado", "Ha ocurrido un error inesperado.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Excepción no controlada");
            }

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }, cancellationToken);

            return true;
        }
    }
}
