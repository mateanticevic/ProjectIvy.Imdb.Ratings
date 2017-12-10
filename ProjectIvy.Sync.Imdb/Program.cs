using Microsoft.Extensions.Configuration;
using ProjectIvy.Sync.Imdb.Model;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

namespace ProjectIvy.Sync.Imdb
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            string connectionString = Configuration["ConnectionStrings:MainDb"];
            string imdbUserRatingsUrl = Configuration["ImdbUserRatingsUrl"];
            var imdbCookies = Configuration.GetSection("ImdbCookies").GetChildren().Select(x => (x.GetValue<string>("Name"), x.GetValue<string>("Value")));

            var users = (await DbHandler.GetImdbUsers(connectionString)).ToList();
            Console.WriteLine($"Found {users.Count} users with an Imdb account");

            foreach (var user in users)
            {
                Console.WriteLine($"Processing movies for userId: {user.userId}");
                var existingIds = await DbHandler.GetMovieIds(connectionString, user.userId);
                var imdbRatings = await ImdbHandler.GetRatings(imdbUserRatingsUrl, imdbCookies, user.imdbUsername);

                var lines = new List<string>(imdbRatings.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
                lines.RemoveAt(0);

                foreach (var line in lines)
                {
                    var reg = new Regex("\"(?<value>[^\"]*)\"");
                    var values = new List<string>();

                    foreach (Match item in reg.Matches(line))
                    {
                        values.Add(item.Groups["value"].Value);
                    }

                    var m = new Movie
                    {
                        UserId = user.userId,
                        ImdbId = values[1].Trim()
                    };

                     var contains = existingIds.Contains(m.ImdbId);
                    if (contains)
                        continue;

                    m.Title = values[5];
                    m.Type = values[6];
                    m.Directors = values[7];
                    m.MyRating = Convert.ToInt32(values[8]);
                    m.Rating = Convert.ToDecimal(values[9], new CultureInfo("en-US"));

                    m.Year = Convert.ToInt16(values[11]);
                    m.Genres = values[12];
                    m.Votes = Convert.ToInt32(values[13]);

                    int.TryParse(values[10], out var runtime);
                    m.Runtime = runtime;

                    var releaseDate = DateTime.Now;

                    if (DateTime.TryParse(values[14], out releaseDate)) m.ReleaseDate = releaseDate;

                    var timestamp = DateTime.Now;

                    if (DateTime.TryParseExact(values[2], "ddd MMM dd hh:mm:ss yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out timestamp)) m.Timestamp = timestamp;
                    else if (DateTime.TryParseExact(values[2], "ddd MMM  d hh:mm:ss yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out timestamp)) m.Timestamp = timestamp;

                    await DbHandler.InsertMovie(connectionString, m);
                    Console.WriteLine($"Movie: {m.Title}, User: {m.UserId}");
                }
            }
        }
    }
}
