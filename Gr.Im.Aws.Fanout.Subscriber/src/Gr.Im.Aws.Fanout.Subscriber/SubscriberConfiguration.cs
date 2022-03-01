using System.Collections.Generic;
using System.Text.Json;

namespace Gr.Im.Aws.Fanout.Subscriber
{
    public class SubscriberConfiguration
    {
        public SubscriberConfiguration(string queueUrl, IReadOnlyDictionary<string, ReceiverConfiguration> receivers, JsonSerializerOptions jsonSerializerOptions)
        {
            QueueUrl = queueUrl;
            Receivers = receivers;
            JsonSerializerOptions = jsonSerializerOptions;
        }

        public string QueueUrl { get; private set; }
        public IReadOnlyDictionary<string, ReceiverConfiguration> Receivers { get; private set; }
        public JsonSerializerOptions JsonSerializerOptions { get; private set; }
    }
}