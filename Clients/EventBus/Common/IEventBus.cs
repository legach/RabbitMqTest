namespace Common
{
    public interface IEventBus
    {
        void Publish(IEvent @event, string exchange);
        void Subscribe<THandler, TEvent>(string subscriber, string exchange) where THandler : IEventHandler<TEvent> where TEvent : IEvent;
    }
}