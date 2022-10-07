using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace S3Browser.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<S3Object> Items { get; set; } = new List<S3Object>();

        public List<string> Buckets { get; set; } = new List<string>();

        public async Task<IActionResult> OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("awsAccessKeyId")) ||
                string.IsNullOrEmpty(HttpContext.Session.GetString("awsSecretAccessKey")))
            {
                return RedirectToPage("AWSData");
            }
            var awsAccessId = HttpContext.Session.GetString("awsAccessKeyId");
            var awsAccessSecret = HttpContext.Session.GetString("awsSecretAccessKey");
            var s3Client = new AmazonS3Client(awsAccessId, awsAccessSecret);

            if (HttpContext.Request.QueryString.HasValue &&
                !string.IsNullOrWhiteSpace(HttpContext.Request.Query["item"]))
            {
                var bucketName = HttpContext.Request.Query["bucket"];
                var itemKey = HttpContext.Request.Query["item"];
                GetObjectRequest requestDownload = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = itemKey
                };
                using (GetObjectResponse responseDownload = await s3Client.GetObjectAsync(requestDownload))
                using (Stream responseStream = responseDownload.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string title = responseDownload.Metadata["x-amz-meta-title"];
                    string contentType = responseDownload.Headers["Content-Type"];


                    //string responseBody = reader.ReadToEnd();
                    return File(responseStream, contentType);
                }
            }
            else if (HttpContext.Request.QueryString.HasValue &&
                !string.IsNullOrWhiteSpace(HttpContext.Request.Query["bucket"]))
            {
                // List bucket items
                var bucketName = HttpContext.Request.Query["bucket"];
                string token = null;
                ListObjectsV2Response response;
                do
                {
                    ListObjectsV2Request request = new ListObjectsV2Request();
                    request.BucketName = bucketName;
                    request.Prefix = HttpContext.Request.Query["prefix"];
                    request.ContinuationToken = token;
                    response = await s3Client.ListObjectsV2Async(request);
                    foreach (var item in response.S3Objects)
                    {
                        if(item.Size > 0)
                            Items.Add(item);
                    }
                    token = response.NextContinuationToken;
                } while (response.IsTruncated == true);
                return Page();
            }
            else
            {
                var response = await s3Client.ListBucketsAsync();
                foreach (var item in response.Buckets)
                {
                    Buckets.Add(item.BucketName);
                }
                return Page();
            }
            
        }
    }
}
