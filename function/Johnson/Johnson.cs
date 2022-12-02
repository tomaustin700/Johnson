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
        //public async Task Tweet([TimerTrigger("* * * * *")] TimerInfo myTimer, ILogger log)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api.peepquote.com/v2/search?person=Johnson");
            var body = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<APIReponse>(body);

            var quoteData = data.results.Where(a => a.quote.Length >= 30);

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

            TweetQuery? tweetResponse =
               await
               (from tweet in tContext.Tweets
                where tweet.Type == TweetType.TweetsTimeline &&
                      tweet.ID == "1564256640479764481" && tweet.MaxResults == 50
                select tweet)
               .SingleOrDefaultAsync();

            var thisWeeks = tweetResponse.Tweets;

            while (thisWeeks.Any(a => a.Text == quote.quote))
            {
                var wRnd = new Random();
                int wRInt = wRnd.Next(0, quoteData.Count());

                quote = quoteData.ElementAt(wRInt);
            }

            await tContext.TweetAsync(quote.quote);

            var friendship =
                            await
                            (from friend in tContext.Friendship
                             where friend.Type == FriendshipType.FollowersList &&
                                   friend.ScreenName == "peepscript" &&
                                   friend.Cursor == -1 &&
                                   friend.Count == 20
                             select friend)
                            .SingleOrDefaultAsync();




            foreach (var id in friendship.Users)
            {
                await tContext.FollowAsync("1564256640479764481", id.UserIDResponse.ToString());
            }

            TwitterUserQuery? userResponse =
               await
           (from user in tContext.TwitterUser
            where user.Type == UserType.Following &&
                  user.ID == "1564256640479764481" && user.PaginationToken == "VRVVVVVVVVVVUZZZ" && user.MaxResults == 20
            select user)
           .SingleOrDefaultAsync();



            foreach (var id in userResponse.Users)
            {
                await tContext.UnFollowAsync("1564256640479764481", id.ID.ToString());
            }

        }

    }
}
