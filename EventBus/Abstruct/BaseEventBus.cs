using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace RabbitMQ.Bus.EventBus
{
    public enum ExchangeType
    {
        [Description("direct")]
        Direct,
        [Description("fanout")]
        Fanout,
        [Description("headers")]
        Headers,
        [Description("topic")]
        Topic
    }

    public abstract class BaseEventBus
    {
        private readonly IConnection connection;
        private BaseQueue queue;

        private IModel _channel;
        private IModel channel => _channel ?? (_channel = GetModel());

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public BaseEventBus(BaseQueue queue)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.queue = queue;

            if (connection == null || !connection.IsOpen)
                connection = GetConnection();

            channel.ExchangeDeclare(queue.Exchange, ExchangeType.Direct.Description());
            channel.QueueDeclare(queue.AppQueueName, false, false, false);
            channel.QueueBind(queue.AppQueueName, queue.Exchange, queue.AppQueueName);
            channel.QueueDeclare(queue.ListenQueueName, false, false, false);
            channel.QueueBind(queue.ListenQueueName, queue.Exchange, queue.ListenQueueName);
        }

        private IModel GetModel()
        {
            return connection.CreateModel();
        }

        private IConnection GetConnection()
        {
            return new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            }.CreateConnection();
        }

        public virtual bool Publish<T>(T model)
        {
            var message = model?.SetMessageArray();
            if (message != null)
            {
                channel.BasicPublish(queue.Exchange, queue.AppQueueName, null, message);
                return true;
            }
            else
            {
                return false;
            }
        }

        public EventingBasicConsumer GetQueueEventListen()
        {
            var queueEvent = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue.ListenQueueName, true, queueEvent);
            return queueEvent;
        }
    }

    public static class BaseEventBusHelper
    {
        public static string Description(this ExchangeType source)
        {
            Type type = source.GetType();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            FieldInfo fi = type.GetField(source.ToString());
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static byte[] SetMessageArray(this object model)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
        }

        public static T GetMessageObject<T>(this byte[] model)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(model));
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
