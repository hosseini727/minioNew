using Cleint;
using Cleint.Services;
using Microsoft.AspNetCore.Mvc;
using Aborz.Minio.Controllers;

namespace Aborz.Minio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MinioController : ControllerBase
    {
        private readonly ILogger<MinioController> _logger;
        private readonly IMakeBucket _IMakeBucket;


        public MinioController(ILogger<MinioController> logger, IMakeBucket IMakeBucket)
        {
            _logger = logger;
            _IMakeBucket = IMakeBucket;
        }



        [HttpGet]
        public async Task<ActionResult> Test()
        {
            var endpoint = "127.0.0.1:9000";
            var accessKey = "wO8pTUIpzqfcYbqZTLqm";
            var secretKey = "CDKiLYVaSIGCcowbXGHQXE2FP7aatRm9eDVfTvND";
            var secure = false;
            var minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(secure)
                .Build();
            await _IMakeBucket.CreatBucket(minio, "test").ConfigureAwait(false);
            return Ok("test");

        }

    }
}