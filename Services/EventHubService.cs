using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EventHubService
{
    private readonly string _connectionString;
    private readonly string _eventHubName;

    public EventHubService(IConfiguration config)
    {
        _connectionString = config["EventHub:ConnectionString"];
        _eventHubName = config["EventHub:HubName"];
    }

    public async Task SendQuizDataAsync(object quizData)
    {
        await using var producerClient = new EventHubProducerClient(_connectionString, _eventHubName);

        using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
        var json = JsonSerializer.Serialize(quizData);
        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(json)));

        await producerClient.SendAsync(eventBatch);
    }
}
