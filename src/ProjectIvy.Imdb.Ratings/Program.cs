using NLog;
using ProjectIvy.Imdb.Ratings.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser;

namespace ProjectIvy.Imdb.Ratings
{
    class Program
    {
        private const string ImdbRatingsUrl = "https://www.imdb.com/user/{userId}/ratings/export";

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            LogInfo("Application started");

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

                var users = (await DbHandler.GetImdbUsers(connectionString)).ToList();
                LogInfo($"Found {users.Count} users with an Imdb account");

                foreach (var user in users)
                {
                    LogInfo($"Processing movies for userId: {user.userId}");

                    var existingIds = await DbHandler.GetMovieIds(connectionString, user.userId);
                    var imdbCookies = await DbHandler.GetImdbUserSecrets(connectionString, user.userId);

                    if (imdbCookies.Count() == 0)
                    {
                        LogInfo($"User {user.userId} does not have imdb cookies");
                        continue;
                    }

                    var imdbRatings = await ImdbHandler.GetRatings(ImdbRatingsUrl, imdbCookies, user.imdbUsername);

                    var csvOptions = new CsvParserOptions(true, ',');
                    var movieMapping = new MovieMapping();
                    var csvParser = new CsvParser<Movie>(csvOptions, movieMapping);

                    var csvReaderOptions = new CsvReaderOptions(new[] { "\n" });
                    var movies = csvParser.ReadFromString(csvReaderOptions, imdbRatings).Where(x => x.IsValid).Select(x => x.Result).ToList();

                    var newMovies = movies.Where(x => !existingIds.Contains(x.ImdbId));

                    foreach (var newMovie in newMovies)
                    {
                        newMovie.UserId = user.userId;
                        await DbHandler.InsertMovie(connectionString, newMovie);
                        LogInfo($"Movie: {newMovie.Title}, User: {newMovie.UserId}");
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }

            LogInfo("Application ended");
        }

        public static void LogInfo(string message)
        {
            _logger.Info(message);
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            _logger.Error(message);
            Console.WriteLine(message);
        }
    }
}
