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

        public string Token { get; set; } = string.Empty;

        public string BucketName { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

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
                !string.IsNullOrWhiteSpace(HttpContext.Request.Query["bucket"]))
            {
                // List bucket items
                BucketName = HttpContext.Request.Query["bucket"];
                Prefix = HttpContext.Request.Query["prefix"];
                Token = HttpContext.Request.Query["token"];
                ListObjectsV2Response response;
                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = BucketName;
                request.Prefix = Prefix;
                request.ContinuationToken = Token;
                response = await s3Client.ListObjectsV2Async(request);
                foreach (var item in response.S3Objects)
                {
                    Items.Add(item);
                }

                if (response.IsTruncated)
                {
                    Token = response.NextContinuationToken;
                }
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
