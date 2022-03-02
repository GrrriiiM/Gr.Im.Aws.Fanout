using System.Collections.Generic;
using System.Text.Json;

namespace Gr.Im.Aws.Fanout.Publisher
{
    public class TopicConfiguration
    {
        public TopicConfiguration(string arn, JsonSerializerOptions jsonSerializerOptions, Dictionary<string, MessageConfiguration> messages)
        {
            Arn = arn;
            JsonSerializerOptions = jsonSerializerOptions;
            Messages = messages;
        }

        public string Arn { get; private set; }
        public JsonSerializerOptions JsonSerializerOptions { get; private set; }
        
        public IReadOnlyDictionary<string, MessageConfiguration> Messages { get; private set; }
    }
}