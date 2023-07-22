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
        public readonly string AccessKey = "EYDDQIQwwgSKCQVZqD8V";
        private readonly string SecretKey = "rfI3CkD49bFlfduzLlrujULg2eAFtfwUg2Kr5P1i";
        private readonly string EndpointDocker = "host.docker.internal:9000";
        private readonly string Endpoint = "127.0.0.1:9000";

        public MinioController(ILogger<MinioController> logger, MinioClient minioClient)
        {
            _logger = logger;
            _MinioClient = minioClient;            
        }


        /// <summary>
        /// Show AccessKey and SecretKey
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("[action]")]
        public async Task<IActionResult> DisplayKeys()
        {
            try
            {
                Dictionary<string,string> keys = new Dictionary<string,string>();
                keys.Add("AccessKey", AccessKey);
                keys.Add("SecretKey", SecretKey);

                //string keys = ($"AccessKey = {AccessKey}, SecretKey = {SecretKey}") ;
                return Ok(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);

            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> MakeBucket(string bucketName)
        {
            try
            {
                #region Initialization Minio
                var location = "us-east-1";
                //var endpoint = EndpointDocker;
                var endpoint = Endpoint;
                //var port = "9000";
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;

                _MinioClient
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(secure)
                .Build();
                #endregion

                #region Make Bucket
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
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);

            }
        }

        /// <summary>
        /// Put Object bucket and return link for download object
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult> CreateLinkDownload(string bucketName, string objectName)
        {
            try
            {
                #region Add tag                 
                var endpoint = Endpoint;
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();
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
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Remove Tags for the object
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult> RemoveTagObject(string bucketName, string objectName)
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
            try
            {
                var args = new RemoveObjectTagsArgs()
                               .WithBucket(bucketName)
                               .WithObject(objectName);
                await minio.RemoveObjectTagsAsync(args);
                return Ok(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Place the object in the queue
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("[action]")]
        public async Task<ActionResult> PutObject(string bucketName, string objectName)
        {
            try
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
                var filePath = objectName;
                //var filePath = "C:/111.png";
                var contentType = "application/octet-stream";
                var endpoint = Endpoint;
                var accessKey = AccessKey;
                var secretKey = SecretKey;
                var secure = false;
                var minio = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(secure)
                    .Build();                

                var putObjectArgs = new PutObjectArgs()
                     .WithBucket(bucketName)
                     .WithObject(objectName)
                     .WithFileName(filePath)
                     .WithTagging(tagging)
                     .WithContentType(contentType);
                await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
               
                return Ok("عملیات با موفقیت انجام شد");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Delete the bucket in Minio
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("[action]")]
        public async Task<ActionResult> RemoveBucket(string bucketName)
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

                var removeBucketArgs = new RemoveBucketArgs();
                removeBucketArgs.WithBucket(bucketName);
                await minio.RemoveBucketAsync(removeBucketArgs).ConfigureAwait(false);

                return Ok("عملیات با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Delete the object in Minio
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Create a link to download the file
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Show the list of available buckets Minio
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("[action]")]
        public async Task<ActionResult> ListBucket()
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

                var listBucket = await minio.ListBucketsAsync().ConfigureAwait(false);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        
    }
}