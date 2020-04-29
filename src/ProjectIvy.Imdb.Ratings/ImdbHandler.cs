using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectIvy.Imdb.Ratings
{
    static class ImdbHandler
    {
        public static async Task<string> GetRatings(string imdbUserRatingsUrl, IEnumerable<(string Name, string Value)> cookies, string userId)
        {
            using (var handler = new HttpClientHandler())
            {
                var cookieContainer = new CookieContainer();

                foreach (var cookie in cookies)
                {
                    var cookieToAdd = new Cookie(cookie.Name, cookie.Value, "/", ".imdb.com");
                    cookieContainer.Add(cookieToAdd);
                }

                handler.CookieContainer = cookieContainer;

                using (var http = new HttpClient(handler))
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, imdbUserRatingsUrl.Replace("{userId}", userId));

                    var response = await http.SendAsync(req);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
