using CreditSystem.API.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CreditSystem.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = exception switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado", "Autenticação necessária"),
                CreditAuthorizationException => (StatusCodes.Status403Forbidden, "Acesso proibido", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno", _environment.IsDevelopment() ? exception.Message : "Ocorreu um erro")
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
