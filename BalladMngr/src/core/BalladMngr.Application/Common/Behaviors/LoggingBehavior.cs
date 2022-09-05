using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BalladMngr.Application.Common.Behaviors
{
    /*
     * MediatR işleyişinde mesajlar arasına girmek mümkün.
     * Burada mesaj işlenmeden önce araya girip basitçe log atmaktayız.
     */
    public class LoggingBehavior<TRequest>
        : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger<TRequest> _logger;
        public LoggingBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
        }
        public async Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation($"Talep geldi {request}", requestName);
        }
    }
}