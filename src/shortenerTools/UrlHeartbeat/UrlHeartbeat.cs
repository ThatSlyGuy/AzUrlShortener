using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Cloud5mins.Function
{
    public static class UrlHeartbeat
    {
        [FunctionName("UrlHeartbeat")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var heartbeatUrl = Environment.GetEnvironmentVariable("HEARTBEAT_URL");

            log.LogInformation($"Run(): About to hit URL: '{heartbeatUrl}'");

            await HitUrl(heartbeatUrl, log).ConfigureAwait(false);

            log.LogInformation($"Run(): Completed..");
        }

        private static async Task<HttpResponseMessage> HitUrl(string url, ILogger log)
        {
            var stopwatch = Stopwatch.StartNew();

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation($"hitUrl(): Successfully hit URL: '{url}' in {stopwatch.ElapsedMilliseconds}ms");
            }
            else
            {
                log.LogInformation($"hitUrl(): Failed to hit URL: '{url}'. Response: {(int)response.StatusCode + " : " + response.ReasonPhrase}");
            }

            return response;
        }
    }
}
