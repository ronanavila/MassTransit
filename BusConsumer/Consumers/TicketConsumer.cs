using MassTransit;
using SharedModel;

namespace BusConsumer.Consumers;

public class TicketConsumer : IConsumer<Ticket>
{
    private readonly ILogger _logger;

    public TicketConsumer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Ticket> context)
    {
        await Console.Out.WriteLineAsync(context.Message.UserName);

        _logger.LogInformation($"New message received: {context.Message.UserName} {context.Message.Location}");
    }
}
