using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gr.Im.Aws.Fanout.Subscriber
{
    public class Subscriber
    {
        readonly IAmazonSQS _sqs;
        readonly SubscriberConfiguration _configuration;
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<Subscriber> _logger;

        public Subscriber(IAmazonSQS sqs, SubscriberConfiguration configuration, IServiceProvider serviceProvider, ILogger<Subscriber> logger)
        {
            _sqs = sqs;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ReceiveAsync(CancellationToken cancellationToken = default)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _configuration.QueueUrl,
                MaxNumberOfMessages = 10,
                VisibilityTimeout = 10,
                WaitTimeSeconds = 10
            };

            var response = await _sqs.ReceiveMessageAsync(_configuration.QueueUrl, cancellationToken);

            if (!response.Messages.Any()) return;

            var messagesCount = response.Messages.Count;

            _logger.LogInformation($"Mensagens recebidas: {messagesCount}");

            var i = 1;
            foreach (var message in response.Messages)
            {
                var fanoutMessage = JsonSerializer.Deserialize<FanoutMessage>(message.Body);

                if (fanoutMessage.HasMessageName)
                {
                    if (_configuration.Receivers.TryGetValue(fanoutMessage.MessageName, out ReceiverConfiguration receiverConfiguration))
                    {
                        _logger.LogInformation($"Mensagem {fanoutMessage.MessageName} sendo processada");
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var receiver = (IReceiver)scope.ServiceProvider.GetRequiredService(receiverConfiguration.ReceiverType);
                            var obj = JsonSerializer.Deserialize(fanoutMessage.Message, receiver.MessageType, receiverConfiguration.JsonSerializerOptions ?? _configuration.JsonSerializerOptions);
                            await receiver.Received(obj, cancellationToken);
                        }
                        _logger.LogInformation($"Mensagem ({i}/{messagesCount}) processada");
                    }
                    else
                    {
                        _logger.LogError("Mensagem sem 'message-name' com receiver configurado");
                    }
                }
                else
                {
                    _logger.LogError("Mensagem sem 'message-name'");
                }
                await _sqs.DeleteMessageAsync(_configuration.QueueUrl, message.ReceiptHandle, cancellationToken);
                _logger.LogInformation($"Mensagem ({i}/{messagesCount}) removida");
                i += 1;
            }
        }

    }


    public class FanoutMessage
    {
        public string Message { get; set; }
        public Dictionary<string, FanoutMessageAttribute> MessageAttributes { get; set; }

        public bool HasMessageName => MessageAttributes.ContainsKey("message-name");
        public string MessageName => MessageAttributes["message-name"].Value;
        
        
    }

    public class FanoutMessageAttribute
    {
        public string Value { get; set; }
    }
}
