


using System.Net.Mime;
using System.Text.Json;
using DynamicFormBuilder.API.Exceptions;

namespace DynamicFormBuilder.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

	public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (ValidationAppException ex)
		{
			_logger.LogWarning(ex, "Validation exception handled by global middleware.");
			await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ex.Message, ex.Errors);
		}
		catch (AppException ex)
		{
			_logger.LogWarning(ex, "Application exception handled by global middleware.");
			await WriteResponseAsync(context, ex.StatusCode, ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception handled by global middleware.");
			await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, "Beklenmeyen bir hata oluştu.");
		}
	}

	private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message, IEnumerable<string>? errors = null)
	{
		if (context.Response.HasStarted)
		{
			throw new InvalidOperationException("The response has already started.");
		}

		context.Response.Clear();
		context.Response.StatusCode = statusCode;
		context.Response.ContentType = MediaTypeNames.Application.Json;

		if (errors is null)
		{
			await context.Response.WriteAsync(JsonSerializer.Serialize(new
			{
				statusCode,
				message,
				traceId = context.TraceIdentifier
			}));
			return;
		}

		await context.Response.WriteAsync(JsonSerializer.Serialize(new
		{
			statusCode,
			message,
			errors,
			traceId = context.TraceIdentifier
		}));
	}
}