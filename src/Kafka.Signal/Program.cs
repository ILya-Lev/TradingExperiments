using Confluent.Kafka;

//see https://github.com/confluentinc/confluent-kafka-dotnet/
Console.WriteLine("Hello, from kafka signal app - press enter to quit!");

const string topic = "kinaction_helloworld";
var config = new ProducerConfig() { BootstrapServers = "localhost:9094", };
using var producer = new ProducerBuilder<Null, string>(config).Build();
try
{
    for (var i = 0; i < 10; i++)
    {
        var message = new Message<Null, string>() { Value = $"{DateTimeOffset.Now:R} {i}" };
        Console.WriteLine($"Sending message: {message.Value}");

        var deliveryResult = await producer.ProduceAsync(topic, message);
        Console.WriteLine($"Delivered {deliveryResult.Value} to {deliveryResult.TopicPartitionOffset}");

        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}
catch (Exception exc)
{
    Console.WriteLine(exc);
}
Console.ReadLine();