using System.Net;
using System.Text.Json;
using Banco_VivesBank.User.Exceptions;


namespace Banco_VivesBank.Utils.ExceptionMiddleware;

/// <summary>
/// Inicializa una nueva instancia de la clase <see cref="GlobalExceptionMiddleware"/>.
/// </summary>
/// <param name="next">El siguiente delegado del middleware que se ejecutará en la cadena de procesamiento.</param>
/// <param name="logger">El servicio de registro utilizado para registrar las excepciones.</param>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    
    /// <summary>
    /// Intercepta las solicitudes y maneja cualquier excepción que ocurra durante la ejecución de la aplicación.
    /// </summary>
    /// <param name="context">El contexto HTTP que representa la solicitud y la respuesta.</param>
    /// <remarks>
    /// Este método se asegura de que cualquier excepción que ocurra en el procesamiento de la solicitud se maneje correctamente
    /// y se devuelva una respuesta adecuada al cliente.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maneja la excepción y devuelve una respuesta adecuada al cliente.
        /// </summary>
        /// <param name="context">El contexto HTTP de la solicitud.</param>
        /// <param name="exception">La excepción que ocurrió durante el procesamiento de la solicitud.</param>
        /// <returns>Una tarea asincrónica que representa la operación de escritura de la respuesta.</returns>
    
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Definir el código de estado HTTP por defecto
            var statusCode = HttpStatusCode.BadRequest;
            var errorResponse = new { message = "An unexpected error occurred." };

            // Manejar tipos de excepciones personalizadas
            switch (exception)
            {
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, "Invalid operation.");
                    break;
                
                /**************** NOTFOUND EXCEPTIONS *****************************************/
                case UserNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                case InvalidPasswordException:
                case UserExistException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                
                default:
                    logger.LogError(exception, "An unhandled exception occurred.");
                    break;

            }

            // Configurar la respuesta HTTP
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }