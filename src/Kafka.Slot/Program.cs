using Confluent.Kafka;
//see https://github.com/confluentinc/confluent-kafka-dotnet/
Console.WriteLine("Hello from kafka slot - press enter to quit!");

const string topic = "kinaction_helloworld";
var config = new ConsumerConfig()
{
    GroupId = "aaa",
    BootstrapServers = "localhost:9094",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe(topic);

while (true)
{
    try
    {
        var consumeResult = consumer.Consume();
        Console.WriteLine($"consumed message {consumeResult.Message.Value} from {consumeResult.TopicPartitionOffset}");
    }
    catch (Exception exc)
    {
        Console.WriteLine($"error consuming message: {exc.Message}");
    }
}

Console.ReadLine();