using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace Cloud5mins.Function
{
    public static class UrlRedirect
    {
        [FunctionName("UrlRedirect")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlRedirect/{shortUrl}")] HttpRequestMessage req,
            string shortUrl,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

            string redirectUrl = "https://costv.guide";

            if (!string.IsNullOrWhiteSpace(shortUrl))
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                redirectUrl = config["defaultRedirectUrl"];

                StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]);

                var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);

                var newUrl = await stgHelper.GetShortUrlEntity(tempUrl);

                if (newUrl != null)
                {
                    log.LogInformation($"Found it: {newUrl.Url}");
                    newUrl.Clicks++;
                    stgHelper.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey));
                    await stgHelper.SaveShortUrlEntity(newUrl);
                    redirectUrl = WebUtility.UrlDecode(newUrl.Url);
                }
            }
            else
            {
                log.LogInformation("Bad Link, resorting to fallback.");
            }

            var currentQuery = HttpUtility.ParseQueryString(req.RequestUri.Query);

            var redirectUriBuilder = new UriBuilder(redirectUrl);
            var redirectQuery = HttpUtility.ParseQueryString(redirectUriBuilder.Query);

            foreach (var key in currentQuery.AllKeys)
            {
                redirectQuery[key] = currentQuery[key];

                redirectUriBuilder.Query = redirectQuery.ToString();
            }

            var response = req.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Add("Location", redirectUriBuilder.ToString());

            return response;
        }
  }
}
