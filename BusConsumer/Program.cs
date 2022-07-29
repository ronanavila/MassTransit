using BusConsumer.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//HealthChecks
builder.Services.AddHealthChecks();

builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = (check) => check.Tags.Contains("ready");
});

builder.Services.AddLogging();
//LOGGER
ServiceProvider? serviceProvider = builder.Services.BuildServiceProvider();
var logger = serviceProvider.GetService<ILogger<TicketConsumer>>();
builder.Services.AddSingleton(typeof(ILogger), logger);

//MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TicketConsumer>();

    x.UsingRabbitMq((hostContext, config) =>
    {        
        config.Host(new Uri("rabbitmq://localhost"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        config.ConfigureEndpoints(hostContext);

        config.ReceiveEndpoint("orderTicketQueue", ep =>
        {
            ep.PrefetchCount = 10;
            ep.UseMessageRetry(r => r.Interval(2, 100));
            ep.ConfigureConsumer<TicketConsumer>(hostContext);
        });
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
    {
        Predicate = (check) => check.Tags.Contains("ready"),
    });

    endpoints.MapHealthChecks("/health/live", new HealthCheckOptions());
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
