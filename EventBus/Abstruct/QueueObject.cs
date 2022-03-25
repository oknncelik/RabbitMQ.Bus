using System.ComponentModel;

namespace RabbitMQ.Consumer.EventBus.Abstruct
{
    public enum QueueStatus
    {
        [Description("Prosesing...")]
        Prosesing,
        [Description("Done")]
        Done,
        [Description("Error")]
        Error,
    }
    public class QueueObject<T>
    {
        public string Id { get; set; }
        public QueueStatus Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public QueueObject()
        {

        }
        public QueueObject(T model)
        {
            Id = Guid.NewGuid().ToString();
            Status = QueueStatus.Prosesing;
            Message = "Raport creating...";
            Data = model;
        }
    }
}
