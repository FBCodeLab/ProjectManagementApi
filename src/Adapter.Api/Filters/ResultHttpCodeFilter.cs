namespace Adapter.Api.Filters;
public class ResultHttpCodeFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is Core.Contracts.Results.IResult resultWithStatus)
        {
            context.HttpContext.Response.StatusCode = (int)resultWithStatus.HttpStatusCode;
            return resultWithStatus;
        }
        return result;
    }
}