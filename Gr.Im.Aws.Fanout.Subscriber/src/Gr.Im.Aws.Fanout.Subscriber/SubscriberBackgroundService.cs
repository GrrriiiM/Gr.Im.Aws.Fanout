
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gr.Im.Aws.Fanout.Subscriber
{
    public class SubscriberBackgroundService : BackgroundService
    {
        readonly Subscriber _subscriber;
        readonly ILogger<SubscriberBackgroundService> _logger;

        public SubscriberBackgroundService(Subscriber subscriber, ILogger<SubscriberBackgroundService> logger)
        {
            _subscriber = subscriber;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _subscriber.ReceiveAsync(stoppingToken);    
                }
                catch (System.Exception ex)
                {
                     _logger.LogCritical(ex, "Falha o receber mensagem");
                }
                
            }
        }
    }
}
