using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace S3Browser.Pages
{
    public class DownloadModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            var awsAccessId = HttpContext.Session.GetString("awsAccessKeyId");
            var awsAccessSecret = HttpContext.Session.GetString("awsSecretAccessKey");
            var s3Client = new AmazonS3Client(awsAccessId, awsAccessSecret);
            var bucketName = HttpContext.Request.Query["bucket"];
            var itemKey = HttpContext.Request.Query["item"];
            GetObjectRequest requestDownload = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = itemKey
            };
            using (GetObjectResponse responseDownload = await s3Client.GetObjectAsync(requestDownload))
            using (Stream responseStream = responseDownload.ResponseStream)
            {
                Stream fileStream = new MemoryStream();
                string title = responseDownload.Metadata["x-amz-meta-title"];
                string contentType = responseDownload.Headers["Content-Type"];

                await responseStream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                fileStream.Seek(0, SeekOrigin.Begin);
                return File(fileStream, contentType, !string.IsNullOrWhiteSpace(title) ? title : "downloaded.file");

            }
        }
    }
}
