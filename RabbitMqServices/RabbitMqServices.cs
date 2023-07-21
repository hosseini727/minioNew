using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace Moon
{
    public class RabbitMqServices //: IDisposable //: IRabbitMqServices
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public const string queueName = "ef";

        public RabbitMqServices()
        {

            var factory = new ConnectionFactory() { HostName = "127.0.0.1" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "ef",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
            ConsumeMessages();
        }

        //private readonly MinioClient _MinioClient;


        //public RabbitMqServices(MinioClient minioClient)
        //{
        //    _MinioClient = minioClient;
        //}

        public void ConsumeMessages()
        {
            var consumer = new EventingBasicConsumer(_channel);
            //consumer.Received += (model, ea) =>
            //{
            //    var body = ea.Body.ToArray();
            //    var message = Encoding.UTF8.GetString(body);
            //    Console.WriteLine("Received message: {0}", message);
            //};

            consumer.Received += async (model, eventArgs) =>
            {
                var properties = eventArgs.BasicProperties;
                var replyProperties =
                    _channel.CreateBasicProperties();

                replyProperties.CorrelationId = properties.CorrelationId;
                string response = "";
                string message = "";
                var body = eventArgs.Body.ToArray();
                try
                {
                    message = System.Text.Encoding.UTF8.GetString(body);
                    response = message;
                }
                catch (System.Exception ex)
                {
                    response = 0.ToString();
                }
                finally
                {
                    var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
                    _channel.BasicPublish
                        (exchange: string.Empty,
                         routingKey: _channel.QueueDeclare(queue: string.Empty).QueueName,
                        //routingKey: properties.ReplyTo,
                        basicProperties: replyProperties,
                        body: responseBytes);
                    _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);

                    _channel.QueueDeclare(queue: "ef", durable: true, exclusive: false, autoDelete: false, arguments: null);

                }
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: false,
                                  arguments: null,
                                  consumer: consumer);
        }

        
    }
}

