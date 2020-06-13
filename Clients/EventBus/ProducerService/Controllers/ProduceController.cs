using Common;
using Microsoft.AspNetCore.Mvc;

namespace ProducerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProduceController : ControllerBase
    {
        private readonly IEventBus _eventBus;

        public ProduceController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpPost]
        public void SendMessageAsync([FromBody] MainEvent message)
        {
            _eventBus.Publish(message, nameof(MainEvent)+"_exchange");
        }
    }
}
