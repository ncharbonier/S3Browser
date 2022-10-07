using System;
namespace S3Browser.Models
{
    public class AWSAccess
    {
        public AWSAccess()
        {
        }

        public string AccessId { get; set; }

        public string AccessSecret { get; set; }

        public string Bucket { get; set; }

        public string Prefix { get; set; }

    }
}
