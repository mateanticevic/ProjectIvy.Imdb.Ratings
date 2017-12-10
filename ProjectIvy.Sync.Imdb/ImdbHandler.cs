using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectIvy.Sync.Imdb
{
    static class ImdbHandler
    {
        public static async Task<string> GetRatings(string imdbUserRatingsUrl, IEnumerable<(string Name, string Value)> cookies, string userId)
        {
            var handler = new HttpClientHandler();
            try
            {
                var cc = new CookieContainer();

                foreach (var cookie in cookies)
                {
                    var cookieToAdd = new Cookie(cookie.Name, cookie.Value, "/", ".imdb.com");
                    cc.Add(cookieToAdd);
                }

                handler.CookieContainer = cc;

                var req = new HttpRequestMessage(HttpMethod.Get, imdbUserRatingsUrl.Replace("{userId}", userId));
                var http = new HttpClient(handler);

                return await http.SendAsync(req).Result.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
