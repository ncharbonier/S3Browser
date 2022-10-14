using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using S3Browser.Models;

namespace S3Browser.Pages
{
    public class AWSDataModel : PageModel
    {
        [BindProperty]
        public AWSAccess AWSAccess { get; set; } = default!;

        public IActionResult OnGet()
        {
            return Page();
        }


        public IActionResult OnPost()
        {
            if (!string.IsNullOrWhiteSpace(AWSAccess?.AccessId) &&
                !string.IsNullOrWhiteSpace(AWSAccess?.AccessSecret))
            {
                HttpContext.Session.SetString("awsAccessKeyId", AWSAccess.AccessId);
                HttpContext.Session.SetString("awsSecretAccessKey", AWSAccess.AccessSecret);
                return Redirect($"Index?bucket={AWSAccess.Bucket}&prefix={AWSAccess.Prefix}");
            }
            return Page();
        }
    }
}
