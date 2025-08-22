using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace Worker_Service_with_RabbitMq_and_Asp.net_core_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IRabbitMqPublisher _publisher;

        public EmailController(IRabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost]
        public IActionResult SendEmail([FromBody] EmailMessage email)
        {
            _publisher.Publish(email);
            return Accepted(new { message = "Email queued." });
        }
    }

}
