using Cleint;
using Cleint.DataModel;
using Cleint.DataModel.ILM;
using Cleint.DataModel.Tags;
using Microsoft.AspNetCore.Mvc;
using Services.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Xml;

namespace Alborz.MinIo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MinioController : ControllerBase
    {

        private readonly ILogger<MinioController> _logger;
        private readonly MinioClient _MinioClient;
        //private readonly IRabbitMqServices _RabbitMqServices;
        //public readonly string AccessKey = "Q1UIngYLuWLe6tvszHw5";
        //private readonly string SecretKey = "dZv2AN9bLCNdTYiVDqZGNNj5O3Jf8uRIOvUYZcM7";

        public readonly string AccessKey = "EYDDQIQwwgSKCQVZqD8V";
        private readonly string SecretKey = "rfI3CkD49bFlfduzLlrujULg2eAFtfwUg2Kr5P1i";

        public MinioController(ILogger<MinioController> logger, MinioClient minioClient)
        {
            _logger = logger;
            _MinioClient = minioClient;            
        }



        //[HttpPost("[action]")]
        //public async Task<IActionResult> Rabbit()
        //{
        //    try
        //    {
        //       var res =  _RabbitMqServices.();               
        //       return Ok("");                
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exceptions
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        return StatusCode(500, "An error occurred");
        //    }
        //}


        //[HttpPost("[action]")]
        //public async Task<IActionResult> ReceiveData()
        //{
        //    try
        //    {
        //        using (StreamReader reader = new StreamReader(Request.Body))
        //        {
        //            string requestBody = await reader.ReadToEndAsync();

        //            // Process the received data in requestBody

        //            var response = new { Message = "Data received successfully" };
        //            return Ok(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exceptions
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        return StatusCode(500, "An error occurred");
        //    }
        //}

        [HttpGet("[action]")]
        public async Task<ActionResult> MakeBucket(string bucketName)
        {
            //_logger.LogError("for test");
            var location = "us-east-1";
            //var endpoint = "127.0.0.1:9000";
            var endpoint = "host.docker.internal:9000";
            //var port = "9000";
            var accessKey = AccessKey;
            var secretKey = SecretKey;
            var secure = false;

            _MinioClient
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(secure)
            .Build();

            var mkBktArgs = new MakeBucketArgs()
                   .WithBucket(bucketName)
                   .WithLocation(location);

            var bktExistArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
            var found = await _MinioClient.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);
            if (!found)
            {
                await _MinioClient.MakeBucketAsync(mkBktArgs).ConfigureAwait(false);
                return Ok("باکت جدید ایجاد شد");
            }
            return Ok("از قبل وجود دارد");
        }


        //HttpRequestMessage data
        //public async Task<HttpResponseMessage> PutObjectBucketReturnLink1()

        //[HttpGet("[action]")]
        //public async Task<ActionResult> PutObjectBucketReturnLink1()
        //{
        //    // Retrieve the JSON data from the request body
        //    //string jsonData = await data.Content.ReadAsStringAsync();
        //    //var ss = data.Content.Headers;
        //    // Process the JSON data as needed
        //    //return new HttpResponseMessage
        //    //{
        //    //    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
        //    //};
        //    return Ok();
        //}


        /// <summary>
        /// Put Object bucket and return link for download object
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult> PutObjectBucketReturnLink(string bucketName, string objectName)
        {
            try
            {
                #region Add tag 
                IDictionary<string, string> dict = new Dictionary<string, string>();
                var collection = new Collection<Cleint.DataModel.Tags.Tag>();
                var tag = new Tag();
                tag.Key = "1";
                tag.Value = "tmp";
                collection.Add(tag);
                var tagging = new Tagging();
                var tagset = new TagSet();
                tagset.Tag = collection;
                tagging.TaggingSet = tagset;
                string environment = Environment.GetEnvironmentVariable("PATH");
                //var filePath = "D:\\down\\data\\my.png";
                var filePath = "111.png";
                var filePath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
                var imagepath = Path.Combine(filePath1, "111.png");
                var contentType = "application/octet-stream";
                var endpoint = "172.17.0.2:9000";

               // var endpoint = "127.0.0.1:9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();
                #endregion

                #region Put Object
                var putObjectArgs = new PutObjectArgs()
                     .WithBucket(bucketName)
                     .WithObject(objectName)
                     .WithFileName(filePath)
                     .WithTagging(tagging)
                     .WithContentType(contentType);
                await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                #endregion

                #region create link for download
                PresignedGetObjectArgs presignedGetObjectArgs = new PresignedGetObjectArgs()
                              .WithBucket(bucketName)
                              .WithObject(objectName)
                              .WithExpiry(60 * 60 * 24);
                string url = await minio.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
                #endregion

                return Ok(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Remove Tags for the object
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult> RemoveTagObject(string bucketName)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            var collection = new Collection<Cleint.DataModel.Tags.Tag>();
            var tag = new Tag();
            tag.Key = "1";
            tag.Value = "tmp";
            collection.Add(tag);
            var tagging = new Tagging();
            var tagset = new TagSet();
            tagset.Tag = collection;
            tagging.TaggingSet = tagset;
            var objectName = "111.png";
            //var filePath = "D:\\down\\data\\my.png";
            var filePath = "C:/111.png";
            var contentType = "application/octet-stream";
            var endpoint = "127.0.0.1:9000";
            var accessKey = AccessKey;
            var secretKey = SecretKey;
            var secure = false;
            var minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(secure)
                .Build();

            try
            {
                var args = new RemoveObjectTagsArgs()
                               .WithBucket("test")
                               .WithObject("111.png");
                await minio.RemoveObjectTagsAsync(args);
                return Ok(args);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> PutObjectBucket(string bucketName)
        {
            try
            {
                ////add tag to put object
                IDictionary<string, string> dict = new Dictionary<string, string>();
                var collection = new Collection<Cleint.DataModel.Tags.Tag>();
                var tag = new Tag();
                tag.Key = "1";
                tag.Value = "tmp";
                collection.Add(tag);
                var tagging = new Tagging();
                var tagset = new TagSet();
                tagset.Tag = collection;
                tagging.TaggingSet = tagset;
                var objectName = "my.mp3";
                var filePath = "D:\\down\\data\\my.mp3";
                //var filePath = "C:/111.png";
                var contentType = "application/octet-stream";
                var endpoint = "127.0.0.1:9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();

                // Remove Tags for the object
                //var args = new RemoveObjectTagsArgs()
                //               .WithBucket("test")
                //               .WithObject("111.png");
                //await minio.RemoveObjectTagsAsync(args);

                var putObjectArgs = new PutObjectArgs()
                     .WithBucket(bucketName)
                     .WithObject(objectName)
                     .WithFileName(filePath)
                     //.WithTagging(tagging)
                     .WithContentType(contentType);
                await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                // end tag to put object


                //create link 
                //PresignedGetObjectArgs presignedGetObjectArgs = new PresignedGetObjectArgs()
                //              .WithBucket(bucketName)
                //              .WithObject(objectName)
                //              .WithExpiry(60 * 60 * 24);
                //string url = await minio.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);

                return Ok("");



                //// Set Lifecycle configuration for the bucket
                //var lfc = new LifecycleConfiguration();
                //var lifecycleRule = new LifecycleRule();
                //lifecycleRule.Status = 1;
                //var ruleFilter = new RuleFilter();

                //ruleFilter.Prefix = "*";

                //lifecycleRule.TransitionObject.StorageClass = null;
                //lifecycleRule.TransitionObject.Days = 1;

                //lfc.Rules.Add(lifecycleRule);

                //// Create an XML element to represent the status
                //XmlDocument statusXmlElement = new XmlDocument();
                //statusXmlElement.InnerText = lifecycleRule.Status = "Status" ;



                //get status bucket name
                //GetBucketLifecycleArgs args1 = new GetBucketLifecycleArgs()
                //   .WithBucket(bucketName);
                //var findGetBucket = await minio.GetBucketLifecycleAsync(args1);
                //var status = findGetBucket.Rules[0].Status;


                //for test
                // add lifecycleConfiguration
                //IDictionary<string, string> dict = new Dictionary<string, string>();
                //var collection = new Collection<Cleint.DataModel.Tags.Tag>();
                //var tag = new Tag();
                //tag.Key = "10";
                //tag.Value = "10";
                //collection.Add(tag);
                //var tagging = new Tagging();
                //var tagset = new TagSet();
                //tagset.Tag = collection;
                //tagging.TaggingSet = tagset;

                //var lifecycleConfiguration = new LifecycleConfiguration()
                //{
                //    Rules = new Collection<LifecycleRule>()
                //    {
                //      new LifecycleRule()
                //      {
                //          Expiration = new Expiration()
                //          {
                //              Days = 1,
                //              ExpiredObjectDeleteMarker = true
                //          },
                //          Status = "Enabled",
                //          ID = "cikt4adovnbu8aoutkmk",
                //           Filter = new RuleFilter()
                //           {
                //               Tag = new Tagging()
                //               {
                //                   TaggingSet = new TagSet()
                //                   {
                //                       Tag = new Collection<Tag>()
                //                       {
                //                           new Tag (){Key = "34" , Value = "esbb" }
                //                       }
                //                   },                                  
                //               },
                //           },
                //      }
                //    }
                //};
                ////Create Bucket Lifecycle Configuration for the bucket
                //SetBucketLifecycleArgs args = new SetBucketLifecycleArgs()
                //                .WithBucket("test333")
                //                .WithLifecycleConfiguration(lifecycleConfiguration);
                //await minio.SetBucketLifecycleAsync(args);
                ////end lifecycleConfiguration




                //add lifecycleConfiguration
                //var lifecycleConfiguration = new LifecycleConfiguration()
                //{
                //    Rules = new Collection<LifecycleRule>()
                //    {
                //      new LifecycleRule()
                //      {
                //          Expiration = new Expiration()
                //          {
                //              Days = 1,
                //              ExpiredObjectDeleteMarker = true
                //          },

                //          Status = "Enabled",
                //          ID = "cikt4adovnbu8aoutkmg",
                //           Filter = new RuleFilter()
                //           {
                //               Prefix = "tmp"
                //           },
                //          //TransitionObject = new Transition()
                //          //{
                //          //  Days = 1,
                //          //  StorageClass = "DELETE"
                //          //},
                //      }
                //    }
                //};

                ////Create Bucket Lifecycle Configuration for the bucket
                //SetBucketLifecycleArgs args = new SetBucketLifecycleArgs()
                //                .WithBucket("test")
                //                .WithLifecycleConfiguration(lifecycleConfiguration);
                //await minio.SetBucketLifecycleAsync(args);
                //end lifecycleConfiguration

                //// Remove Bucket Lifecycle Configuration for the bucket
                //var args = new RemoveBucketLifecycleArgs()
                //               .WithBucket(bucketName);
                //await minio.RemoveBucketLifecycleAsync(args);
                //Console.WriteLine($"Set Lifecycle for bucket {bucketName}.");                


                //List bucker
                //var test = await minio.ListBucketsAsync().ConfigureAwait(false);


                //list object                         
                // var args = new ListObjectsArgs()
                //.WithBucket("test")
                //.WithPrefix("*")
                //.WithRecursive(true);
                //IObservable<Item> observable = minio.ListObjectsAsync(args);
                //var subscription = observable.Subscribe(
                //item => item.Key.ToString());           
                //var subscription = observable.Subscribe(
                //item => item.Key.ToString());                


                //return Ok("ok");
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> RemoveBucket(string bucketName)
        {
            try
            {
                var objectName = "my.mp3";
                var filePath = "D:\\down\\data\\my.mp3";
                var contentType = "application/octet-stream";
                var endpoint = "127.0.0.1:9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();

                var removeBucketArgs = new RemoveBucketArgs();
                removeBucketArgs.WithBucket(bucketName);
                await minio.RemoveBucketAsync(removeBucketArgs).ConfigureAwait(false);

                return Ok("عملیات با موفقیت انجام شد");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> RemoveObject(string bucketName, string objectName)
        {
            try
            {
                var endpoint = "127.0.0.1:9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();

                var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
                removeObjectArgs.WithBucket(bucketName);
                await minio.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);

                return Ok("عملیات با موفقیت انجام شد");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet("[action]")]
        public async Task<ActionResult> CreateLinkForDownload(string bucketName, string objectName)
        {
            try
            {
                //var endpoint = "127.0.0.1:9000";
                var endpoint = "localhost";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();



                PresignedGetObjectArgs presignedGetObjectArgs = new PresignedGetObjectArgs()
                                       .WithBucket(bucketName)
                                       .WithObject(objectName)
                                       .WithExpiry(60 * 60 * 24);
                string url = await minio.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);

                return Ok(url);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> ListBucket(string bucketName)
        {
            try
            {
                var endpoint = "127.0.0.1:9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();

                await minio.ListBucketsAsync().ConfigureAwait(false);

                return Ok("عملیات با موفقیت انجام شد");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> LifeCyle(string bucketName)
        {
            try
            {
                var endpoint = "127.0.0.1:9000";
                var accessKey =AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();

                await minio.ListBucketsAsync().ConfigureAwait(false);

                return Ok("عملیات با موفقیت انجام شد");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpGet("[action]")]
        public async Task<ActionResult> ListObject(string bucketName)
        {
            try
            {
                var endpoint = "127.0.0.1:9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                //var prefix = "optional-prefix";
                bool recursive = true;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();

                //list object                         
                var args = new ListObjectsArgs()
               .WithBucket("test")
               .WithBucket(bucketName)
               .WithPrefix("optional-prefix")
               .WithRecursive(true);


                #region create link for download
                PresignedGetObjectArgs presignedGetObjectArgs = new PresignedGetObjectArgs()
                              .WithBucket("test")
                              .WithObject("my9.mp3")
                              .WithExpiry(60 * 60 * 24);
                string url = await minio.PresignedGetObjectAsync(presignedGetObjectArgs).ConfigureAwait(false);
                #endregion



                ////checked object
                //var args2 = new GetObjectArgs()
                //    .WithBucket("test")
                //    .WithObject("my.mp3");

                //var test = await minio.ListBucketsAsync().ConfigureAwait(false);

                //StatObjectArgs statObjectArgs = new StatObjectArgs()
                //                       .WithBucket("test")                                       
                //                       .WithObject("my.mp3");
                //await _MinioClient.StatObjectAsync(statObjectArgs);


                //// Get input stream to have content of 'my-objectname' from 'my-bucketname'
                //GetObjectArgs getObjectArgs = new GetObjectArgs()
                //                                  .WithBucket("essi")
                //                                  .WithObject("my.mp3");

                //await _MinioClient.GetObjectAsync(getObjectArgs);


                //var bktExistArgs = new BucketExistsArgs().WithBucket(bucketName);
                //var found = await _MinioClient.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);
                //var checkedObject = _MinioClient.GetObjectAsync(args2).ConfigureAwait(false);

                //if (!found)
                //{
                //    var ggg = await minio.ListObjectsAsync(args);

                //    return Ok("عملیات با موفقیت انجام شد");
                //}
                //var ff = await  minio.ListObjectsAsync(args);

                return Ok(url);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}