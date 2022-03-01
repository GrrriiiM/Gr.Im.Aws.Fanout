using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Gr.Im.Aws.Fanout.Subscriber
{
    public class Subscriber
    {
        readonly IAmazonSQS _sqs;
        readonly SubscriberConfiguration _configuration;
        readonly IServiceProvider _serviceProvider;

        public Subscriber(IAmazonSQS sqs, SubscriberConfiguration configuration, IServiceProvider serviceProvider)
        {
            _sqs = sqs;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
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

            foreach(var message in response.Messages)
            {
                var fanoutMessage = JsonSerializer.Deserialize<FanoutMessage>(message.Body);
                if (fanoutMessage.MessageAttributes.ContainsKey("message-name"))
                {
                    var messageName = fanoutMessage.MessageAttributes["message-name"].Value;
                    if (_configuration.Receivers.ContainsKey(messageName))
                    {
                        using(var scope = _serviceProvider.CreateScope())
                        {
                            var receiverConfiguration = _configuration.Receivers[messageName];
                            var receiver = (IReceiver)scope.ServiceProvider.GetRequiredService(receiverConfiguration.ReceiverType);
                            var obj = JsonSerializer.Deserialize(fanoutMessage.Message, receiver.MessageType, receiverConfiguration.JsonSerializerOptions ?? _configuration.JsonSerializerOptions);
                            await receiver.Received(obj, cancellationToken);
                        }
                    }
                }
                await _sqs.DeleteMessageAsync(_configuration.QueueUrl, message.ReceiptHandle, cancellationToken);
            }

        }
    }

    public class FanoutMessage
    {
        public string Message { get; set; }
        public Dictionary<string, FanoutMessageAttribute> MessageAttributes { get; set; }
    }

    public class FanoutMessageAttribute
    {
        public string Value { get; set; }
    }
}
