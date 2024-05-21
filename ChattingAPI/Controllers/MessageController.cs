using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChattingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public MessageController(IConfiguration configuration)
        {
        }

        // GET: api/<MessageController>
        [HttpGet]
        public string Get([FromServices] ChatDbContext dbContext)
        {
            return this.GetHistory(dbContext, 10);
        }

        // GET api/<MessageController>/period/5
        [HttpGet("period/{minutes}")]
        public string Get([FromServices] ChatDbContext dbContext, int minutes)
        {
            return this.GetHistory(dbContext, minutes);
        }

        protected string GetHistory(ChatDbContext dbContext, int period)
        {
            var results = dbContext?.GetHistory(period);
            Log.Information("Fetch history from {ip} for {period} minutes", HttpContext.Connection.RemoteIpAddress, period);

            return JsonSerializer.Serialize(results);
        }

        // POST api/<MessageController>
        [HttpPost]
        public IActionResult Post([FromServices] ChatDbContext dbContext, [FromServices] ChatServer chatServer, [FromBody] string message)
        {
            dbContext?.SaveMessage(message);
            _ = (chatServer?.BroadcastMessageAsync(message, null));

            Log.Information("Message from {ip}: {message}", HttpContext.Connection.RemoteIpAddress, message);

            return Ok();
        }
        /*
                // PUT api/<MessageController>/5
                [HttpPut("{id}")]
                public void Put(int id, [FromBody] string value)
                {
                }

                // DELETE api/<MessageController>/5
                [HttpDelete("{id}")]
                public void Delete(int id)
                {
                }*/
    }
}
