using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Gr.Im.Aws.Fanout.Publisher
{
    public class Publisher
    {
        readonly IAmazonSimpleNotificationService _sns;
        readonly TopicConfiguration _configuration;

        public Publisher(IAmazonSimpleNotificationService sns, TopicConfiguration configuration)
        {
            _sns = sns;
            _configuration = configuration;
        }

        public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        {
            var messageTypeName = typeof(TMessage).Name;
            if (!_configuration.Messages.ContainsKey(messageTypeName))
                throw new ArgumentException($"Mensagem do tipo {messageTypeName} não configurada");

            var messageConfiguration = _configuration.Messages[messageTypeName];

            var messageJson = JsonSerializer.Serialize(message, _configuration.JsonSerializerOptions);

            var request = new PublishRequest
            {
                TopicArn = _configuration.Arn,
                Message = messageJson
            };

            var messageName = messageConfiguration.MessageName?.Invoke(message) ?? messageTypeName;

            request.MessageAttributes.Add("message-name", new MessageAttributeValue
            {
                StringValue = messageName
            });

            if (messageConfiguration.GroupId != null) request.MessageGroupId = $"{messageName}:{messageConfiguration.GroupId(message)}";

            foreach(var messageAttribute in messageConfiguration.Attributes)
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
