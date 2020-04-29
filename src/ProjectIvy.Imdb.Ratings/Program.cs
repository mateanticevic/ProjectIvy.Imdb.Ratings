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
            _logger.Info("Application started");

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

                var users = (await DbHandler.GetImdbUsers(connectionString)).ToList();
                _logger.Info($"Found {users.Count} users with an Imdb account");

                foreach (var user in users)
                {
                    _logger.Info($"Processing movies for userId: {user.userId}");

                    var existingIds = await DbHandler.GetMovieIds(connectionString, user.userId);
                    var imdbCookies = await DbHandler.GetImdbUserSecrets(connectionString, user.userId);
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
                        _logger.Info($"Movie: {newMovie.Title}, User: {newMovie.UserId}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            _logger.Info("Application ended");
        }
    }
}
