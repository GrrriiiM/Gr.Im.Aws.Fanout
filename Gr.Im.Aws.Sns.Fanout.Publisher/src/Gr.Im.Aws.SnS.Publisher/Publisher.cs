using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Gr.Im.Aws.Sns.Fanout.Publisher
{
    public class Publisher<TMessage> where TMessage : class
    {
        readonly IAmazonSimpleNotificationService _sns;
        readonly MessageConfiguration<TMessage> _configuration;

        public Publisher(IAmazonSimpleNotificationService sns, MessageConfiguration<TMessage> configuration)
        {
            _sns = sns;
            _configuration = configuration;
        }

        public async Task SendAsync(TMessage message, CancellationToken cancellationToken = default)
        {

            var messageJson = JsonSerializer.Serialize(message, _configuration.JsonSerializerOptions);

            var request = new PublishRequest
            {
                TopicArn = _configuration.Topic.Arn,
                Message = messageJson
            };

            var messageName = _configuration.MessageName?.Invoke(message) ?? message.GetType().Name;

            request.MessageAttributes.Add("message-name", new MessageAttributeValue
            {
                StringValue = messageName
            });

            if (_configuration.GroupId != null) request.MessageGroupId = $"{messageName}:{_configuration.GroupId(message)}";

            foreach(var messageAttribute in _configuration.Attributes)
            {
                request.MessageAttributes.Add(messageAttribute.Key, new MessageAttributeValue 
                {
                    StringValue = messageAttribute.Value(message)
                });
            }

            var response = await _sns.PublishAsync(request, cancellationToken);
        }
    }
}
