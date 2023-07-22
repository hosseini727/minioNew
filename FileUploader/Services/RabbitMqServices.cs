using Cleint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Collections.ObjectModel;
using Cleint.DataModel.Tags;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

namespace Services.Services
{
   
    public class RabbitMqServices 
    {
        private readonly ILogger<RabbitMqServices> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public const string CurrentQueue = "CurrentQueue";
        public const string logQueue = "logQueue";


        public RabbitMqServices()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "127.0.0.1",
                //HostName = "host.docker.internal",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "CurrentQueue",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }
       
        public void ConsumeMessages()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, eventArgs) =>
                        {
                            var properties = eventArgs.BasicProperties;
                            var replyProperties =
                                _channel.CreateBasicProperties();

                            Exception dontDoThis;
                            replyProperties.CorrelationId = properties.CorrelationId;
                            string response = "";
                            string message = "";
                            var body = eventArgs.Body.ToArray();
                            try
                            {
                                message = System.Text.Encoding.UTF8.GetString(body);
                                bool removeTagObject = RemoveTagObject(message);
                                var responseBytes = System.Text.Encoding.UTF8.GetBytes(message);                              
                                _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                                _channel.QueueDeclare(queue: CurrentQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);                               
                            }
                            catch (Exception ex)
                            {
                                _channel.BasicPublish(exchange: "", routingKey: logQueue, basicProperties: properties, body: body);
                                _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                            }                           
            };

            _channel.BasicConsume(queue: CurrentQueue,
                                  autoAck: false,
                                  arguments: null,
                                  consumer: consumer);
        }

        public bool RemoveTagObject(string bucketName)
        {
            MinioDtoRemoveTag minioDtoRemoveTag = JsonConvert.DeserializeObject<MinioDtoRemoveTag>(bucketName);            
            var endpoint = "127.0.0.1:9000";
            var accessKey = "EYDDQIQwwgSKCQVZqD8V";
            var secretKey = "rfI3CkD49bFlfduzLlrujULg2eAFtfwUg2Kr5P1i";
            var secure = false;
            var minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(secure)
                .Build();

            try
            {
                var args = new RemoveObjectTagsArgs()
                               .WithBucket(minioDtoRemoveTag.BucketName)
                               .WithObject(minioDtoRemoveTag.ObjectName);
                var removeObjectTags = minio.RemoveObjectTagsAsync(args);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return false;
        }       
    }
    public class MinioDtoRemoveTag
    {
        public string BucketName { get; set; }
        public string ObjectName { get; set; }
    }
}
