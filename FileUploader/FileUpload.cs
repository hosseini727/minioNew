/*
* MinIO .NET Library for Amazon S3 Compatible Cloud Storage, (C) 2017-2021 MinIO, Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Net;
using System.Runtime.InteropServices;
using Cleint;
using Cleint.DataModel;

namespace FileUploader;

/// <summary>
///     This example creates a new bucket if it does not already exist, and
///     uploads a file to the bucket. The file name is chosen to be
///     "C:\\Users\\vagrant\\Downloads\\golden_oldies.mp3"
///     Either create a file with this name or change it with your own file,
///     where it is defined down below.
/// </summary>
/// 

public static class FileUpload
{
    private static bool IsWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    private static async Task Main(string[] args)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                                               | SecurityProtocolType.Tls11
                                               | SecurityProtocolType.Tls12;
        var endpoint = "127.0.0.1:9000";
        var accessKey = "5lIGl4a3XxLAhRGObvoY";
        var secretKey = "O6B28HlqIiHGn8FqfLFFmRIVGdUDkfkg2pc1QLn4";
        var secure = false;
        try
        {


            using var minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(secure)
                .Build();
            await Run(minio).ConfigureAwait(false);

            // Create an async task for listing buckets.
            var getListBucketsTask = await minio.ListBucketsAsync().ConfigureAwait(false);



            foreach (var bucket in getListBucketsTask.Buckets)
            {
                Console.WriteLine(bucket.Name + " " + bucket.CreationDateDateTime);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        if (IsWindows()) Console.ReadLine();
    }

    /// <summary>
    ///     Task that uploads a file to a bucket
    /// </summary>
    /// <param name="minio"></param>
    /// <returns></returns>
    private static async Task Run(IMinioClient minio)
    {
        // Make a new bucket called mymusic.
        var bucketName = "test"; //<==== change this
        var location = "us-east-1";
        // Upload the zip file
        var objectName = "my.mp3";
        // The following is a source file that needs to be created in
        // your local filesystem.
        //var filePath = "C:\\Users\\vagrant\\Downloads\\golden_oldies.mp3";
        //var filePath = "D:\\dawn\\data\\my.mp3";
        var filePath = "/my2.mp4";
        //var contentType = "application/zip";
        var contentType = "application/octet-stream";

        

        try
        {

            var bktExistArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            var found = await minio.BucketExistsAsync(bktExistArgs).ConfigureAwait(false);
            if (!found)
            {
                var mkBktArgs = new MakeBucketArgs()
                    .WithBucket(bucketName)
                    .WithLocation(location);
                await minio.MakeBucketAsync(mkBktArgs).ConfigureAwait(false);

                //   //sample remove
                //   var bktExistArgs2 = new RemoveBucketArgs();
                //   bktExistArgs2.WithBucket("test");
                //await minio.RemoveBucketAsync(bktExistArgs2);

                ////sample list bucket
                //var list = await minio.ListBucketsAsync();


                ////sample get  object(create link)
                //PresignedGetObjectArgs args = new PresignedGetObjectArgs()
                //                       .WithBucket("test")
                //                       .WithObject("my.mp3")
                //                       .WithExpiry(60 * 60 * 24);
                //String url = await minio.PresignedGetObjectAsync(args);

            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
                .WithContentType(contentType);
            await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
            Console.WriteLine($"\nSuccessfully uploaded {objectName}\n");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // Added for Windows folks. Without it, the window, tests
        // run in, dissappears as soon as the test code completes.
        if (IsWindows()) Console.ReadLine();
    }


}