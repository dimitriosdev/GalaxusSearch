using HotChocolate;

namespace Backend.Api.GraphQL
{
  public class GraphQLErrorFilter : IErrorFilter
  {
    public IError OnError(IError error)
    {
      return error.Exception switch
      {
        ArgumentException => error.WithMessage("Invalid argument provided")
            .WithCode("INVALID_ARGUMENT"),
        InvalidOperationException => error.WithMessage("Invalid operation")
            .WithCode("INVALID_OPERATION"),
        TimeoutException => error.WithMessage("Request timeout")
            .WithCode("TIMEOUT"),
        _ => error.WithMessage("An unexpected error occurred")
            .WithCode("INTERNAL_ERROR")
      };
    }
  }
}
