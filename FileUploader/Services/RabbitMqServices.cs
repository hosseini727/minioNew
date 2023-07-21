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


    public class MinioDtoRemoveTag
    {
        public string BucketName { get; set; }
        public string ObjectName { get; set; }
    }
    public class RabbitMqServices //: IDisposable //: IRabbitMqServices
    {
        private readonly ILogger<RabbitMqServices> _logger;

        private readonly IConnection _connection;
        private readonly IModel _channel;
        public const string queueName = "ef";
        
        public RabbitMqServices()
        {
            //var factory = new ConnectionFactory() { HostName = "127.0.0.1" };
            string environment = Environment.GetEnvironmentVariable("PATH");

            var factory = new ConnectionFactory()
            {
                HostName = "host.docker.internal",
                //HostName = environment,
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "ef",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        //private readonly MinioClient _MinioClient;


        //public RabbitMqServices(MinioClient minioClient)
        //{
        //    _MinioClient = minioClient;
        //}


       
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
                                //response = message;

                                bool removeTagObject = RemoveTagObject(message);

                                var responseBytes = System.Text.Encoding.UTF8.GetBytes(message);
                                _channel.BasicPublish
                                    (exchange: string.Empty,
                                     routingKey: _channel.QueueDeclare(queue: string.Empty).QueueName,
                                    //routingKey: properties.ReplyTo,
                                    basicProperties: replyProperties,
                                    body: responseBytes);
                                _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);

                                _channel.QueueDeclare(queue: "ef", durable: true, exclusive: false, autoDelete: false, arguments: null);
                               
                            }
                            catch (System.Exception ex)
                            {
                                _channel.BasicPublish(exchange: "", routingKey: "q1", basicProperties: properties, body: body);
                                _channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                                _logger.LogWarning(ex.Message == null ? "problem" : ex.Message);
                                //SendEmail(ex.Message.ToString());

                            }                           
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: false,
                                  arguments: null,
                                  consumer: consumer);
        }

        //public void SendEmail(string exceptionMessage)
        //{
        //    // Send email to the admin
        //    var smtpClient = new SmtpClient("smtp.gmail.com", 587)
        //    {
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential("your_email@gmail.com", "your_password"),
        //        EnableSsl = true
        //    };

        //    var mailMessage = new MailMessage
        //    {
        //        From = new MailAddress("esmaeil.hosseini727@gmail.com"),
        //        Subject = "RabbitMQ Exception",
        //        Body = exceptionMessage
        //    };

        //    mailMessage.To.Add("esmaeil.hosseini727@gmail.com");

        //    // Send the email
        //    smtpClient.Send(mailMessage);
        //}

        //public void Consume()
        //{
        //    int i = 0;
        //    long messageIndex = 0;
        //    var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "127.0.0.1" };

        //    using (var connection = factory.CreateConnection())
        //    {
        //        using (var channel = connection.CreateModel())
        //        {

        //            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        //            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        //            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(model: channel);

        //            // using RabbitMQ.Client;
        //            channel.BasicConsume
        //                (queue: queueName,
        //                autoAck: false,
        //                arguments: null,
        //                consumer: consumer);

        //            consumer.Received += async (model, eventArgs) =>
        //            {
        //                var properties = eventArgs.BasicProperties;
        //                var replyProperties =
        //                    channel.CreateBasicProperties();

        //                replyProperties.CorrelationId = properties.CorrelationId;
        //                string response = "";
        //                string message = "";
        //                var body = eventArgs.Body.ToArray();
        //                try
        //                {
        //                    message = System.Text.Encoding.UTF8.GetString(body);
        //                    response = message;
        //                }
        //                catch (System.Exception ex)
        //                {
        //                    response = 0.ToString();
        //                }
        //                finally
        //                {
        //                    var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
        //                    channel.BasicPublish
        //                        (exchange: string.Empty,
        //                         routingKey: channel.QueueDeclare(queue: string.Empty).QueueName,
        //                        //routingKey: properties.ReplyTo,
        //                        basicProperties: replyProperties,
        //                        body: responseBytes);
        //                    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);

        //                    channel.QueueDeclare(queue: "ef", durable: true, exclusive: false, autoDelete: false, arguments: null);

        //                    var removeTagObject = RemoveTagObject(response);

        //                }
        //            };


        //            System.Console.ReadKey();
        //        }
        //    }
        //    System.Console.WriteLine("Consumer Stoped!");

       
        //}


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
                var ff = minio.RemoveObjectTagsAsync(args);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {

            }
        }

        //public void Dispose()
        //{
        //    _channel.Dispose();
        //    _channel.Dispose();
        //}
    }
}
