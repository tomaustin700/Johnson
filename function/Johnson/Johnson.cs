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
using LinqToTwitter.Common;

namespace Johnson
{
    public class Johnson
    {
        [FunctionName(nameof(Tweet))]
        public async Task Tweet([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer, ILogger log)
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

            string searchTerm = "\"peep show\" OR Alan Johnson";



            // default is id and text and this also brings in created_at and geo
            string tweetFields =
                string.Join(",",
                    new string[]
                    {
            TweetField.CreatedAt,
            TweetField.ID,
            TweetField.Text,
            TweetField.Geo,
            TweetField.AuthorID
                    });

            var searchResponse =
                await
                (from search in tContext.TwitterSearch
                 where search.Type == SearchType.RecentSearch &&
                       search.Query == searchTerm &&
                       search.TweetFields == TweetField.AllFieldsExceptPermissioned &&
                       search.Expansions == ExpansionField.AllTweetFields &&
                       search.MediaFields == MediaField.AllFieldsExceptPermissioned &&
                       search.PlaceFields == PlaceField.AllFields &&
                       search.PollFields == PollField.AllFields &&
                       search.UserFields == UserField.AllFields
                 select search)
                .SingleOrDefaultAsync();

            if (searchResponse?.Tweets != null)
            {
                foreach (var tweet in searchResponse.Tweets)
                {
                    await tContext.FollowAsync("1564256640479764481", tweet.AuthorID);
                }
            }
        }
    }
}
