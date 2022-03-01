using System.Text.Json;

namespace Gr.Im.Aws.Sns.Fanout.Publisher
{
    public class TopicConfiguration
    {
        public TopicConfiguration(string arn, JsonSerializerOptions jsonSerializerOptions)
        {
            Arn = arn;
            JsonSerializerOptions = jsonSerializerOptions;
        }

        public string Arn { get; private set; }
        public JsonSerializerOptions JsonSerializerOptions { get; private set; }
        
    }
}