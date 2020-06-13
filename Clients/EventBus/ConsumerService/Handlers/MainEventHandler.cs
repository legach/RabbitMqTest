using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Logging;

namespace ConsumerService.Handlers
{
    public class MainEventHandler : IEventHandler<MainEvent>
    {
        private readonly ILogger<MainEventHandler> _logger;

        public MainEventHandler(ILogger<MainEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(MainEvent @event)
        {
            _logger.LogInformation($"Receive event: {@event.Value}");
            return Task.CompletedTask;
        }
    }
}
