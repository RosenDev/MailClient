using MediatR;
using Serilog;

namespace MailClient.App.CommandsAndQueries.Behaviors
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;
        public ExceptionHandlingBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error handling {Request}", typeof(TRequest).Name);
                throw;
            }
        }
    }
}
