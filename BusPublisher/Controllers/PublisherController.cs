using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SharedModel;

namespace BusPublisher.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PublisherController : Controller
{
    private readonly IBus _bus;

    public PublisherController(IBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(Ticket ticket)
    {
        if (ticket != null)
        {
            ticket.Booked = DateTime.Now;
            Uri uri = new Uri("rabbitmq://localhost/orderTicketQueue");
            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(ticket);
            return Ok();
        }
        return BadRequest();
    }
}
