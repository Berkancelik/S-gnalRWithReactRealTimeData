using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalRWithReactRealTimeData.Hubs;
 
namespace SignalRWithReactRealTimeData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public MessagesController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetMessages()
        {
            return Ok(MessageStorage.Messages);
        }

        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] MessageRequest request)
        {
            var newMessage = new Message { Id = Guid.NewGuid().ToString(), Text = request.Message };
            MessageStorage.Messages.Add(newMessage);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", newMessage);
            return Ok(new { Message = "Mesaj eklendi!", Data = MessageStorage.Messages });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var message = MessageStorage.Messages.FirstOrDefault(m => m.Id == id);
            if (message == null)
            {
                return NotFound("Mesaj bulunamadı!");
            }

            MessageStorage.Messages.Remove(message);
            await _hubContext.Clients.All.SendAsync("ReceiveMessages", MessageStorage.Messages);
            return Ok(new { Message = "Mesaj silindi!", Data = MessageStorage.Messages });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(string id, [FromBody] MessageRequest request)
        {
            var message = MessageStorage.Messages.FirstOrDefault(m => m.Id == id);
            if (message == null)
            {
                return NotFound("Mesaj bulunamadı!");
            }

            message.Text = request.Message;
            await _hubContext.Clients.All.SendAsync("ReceiveUpdatedMessage", message);
            return Ok(new { Message = "Mesaj güncellendi!", Data = MessageStorage.Messages });
        }
    }

    public class MessageRequest
    {
        public string Message { get; set; }
    }

    public class Message
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

    public static class MessageStorage
    {
        public static List<Message> Messages { get; set; } = new List<Message>();
    }
}
