namespace RabbitMQ.Bus.EventBus
{
    public class BaseQueue
    {
        public string Exchange { get; set; }
        public string AppQueueName { get; set; }
        public string ListenQueueName { get; set; }
    }
}
