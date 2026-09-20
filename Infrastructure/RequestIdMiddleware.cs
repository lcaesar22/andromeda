using Serilog.Context;

namespace Andromeda.Infrastructure;

// RequestIdMiddleware attaches a request ID to a request for logging
public class RequestIdMiddleware(RequestDelegate next)
{
    private const string HeaderKey = "x-request-id";

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers.TryGetValue(HeaderKey, out var existing) 
            ? existing.ToString()
            : Guid.NewGuid().ToString();
        
        context.Response.Headers[HeaderKey] = requestId;

        using (LogContext.PushProperty("RequestId", requestId))
        {
            await next(context);
        }
    }
}