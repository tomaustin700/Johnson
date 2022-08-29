using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using LinqToTwitter.OAuth;
using LinqToTwitter;

namespace Johnson
{
    public class Johnson
    {
        [FunctionName(nameof(Tweet))]
        public async Task Tweet([TimerTrigger("0 */6 * * *")] TimerInfo myTimer, ILogger log)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api.peepquote.com/v2/search?person=Johnson");
            var body = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<APIReponse>(body);

            var quoteData = data.results.Where(a => a.quote.Length >= 40);

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = Environment.GetEnvironmentVariable("APIKey"),
                    ConsumerSecret = Environment.GetEnvironmentVariable("APIKeySecret"),
                    AccessToken = Environment.GetEnvironmentVariable("AccessToken"),
                    AccessTokenSecret = Environment.GetEnvironmentVariable("AccessTokenSecret")
                }
            };

            var tContext = new TwitterContext(auth);

            var rnd = new Random();
            int rInt = rnd.Next(0, quoteData.Count());

            var quote = quoteData.ElementAt(rInt);

            await tContext.TweetAsync(quote.quote);
        }
    }
}