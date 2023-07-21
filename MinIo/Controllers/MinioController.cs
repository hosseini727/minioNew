using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MinIo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MinioController : ControllerBase
    {        
   
        private readonly ILogger<MinioController> _logger;
        //private readonly IMakeBucket _IMakeBucket;


        //public MinioController(ILogger<MinioController> logger, IMakeBucket _IMakeBucket)
        //{
        //    _logger = logger;
        //    _IMakeBucket = _IMakeBucket;
        //}



        [HttpGet]
        public async Task<ActionResult> Test()
        {
            var endpoint = "127.0.0.1:9000";
            var accessKey = "wO8pTUIpzqfcYbqZTLqm";
            var secretKey = "CDKiLYVaSIGCcowbXGHQXE2FP7aatRm9eDVfTvND";
            var secure = false;
            //await MakeBucket.Run(minioClient, destBucketName).ConfigureAwait(false);
            return Ok("test");

        }



    }
}