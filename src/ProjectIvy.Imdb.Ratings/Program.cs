using ProjectIvy.Imdb.Ratings.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using System;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser;

namespace ProjectIvy.Imdb.Ratings
{
    class Program
    {
        private const string ImdbRatingsUrl = "https://www.imdb.com/user/{userId}/ratings/export";

        private static ILogger _logger;

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                                  .MinimumLevel.Override(nameof(Microsoft), LogEventLevel.Information)
                                                  .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                                                  .Enrich.FromLogContext()
                                                  .WriteTo.Console()
                                                  .WriteTo.Graylog(new GraylogSinkOptions()
                                                  {
                                                      Facility = "project-ivy-imdb-ratings",
                                                      HostnameOrAddress = "10.0.1.24",
                                                      Port = 12201,
                                                      TransportType = TransportType.Tcp
                                                  })
                                                  .CreateLogger();
            _logger = Log.Logger;

            _logger.Information("Application started");

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

                var users = (await DbHandler.GetImdbUsers(connectionString)).ToList();
                _logger.Information("Found {UserCount} users with an Imdb account", users.Count);

                foreach (var user in users)
                {
                    _logger.Information("Processing movies for userId: {UserId}", user.userId);

                    var existingIds = await DbHandler.GetMovieIds(connectionString, user.userId);
                    var imdbCookies = await DbHandler.GetImdbUserSecrets(connectionString, user.userId);

                    if (imdbCookies.Count() == 0)
                    {
                        _logger.Information("User {UserId} does not have imdb cookies", user.userId);
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
                        _logger.Information("Movie {Title} added for userId: {UserId}", newMovie.Title, newMovie.UserId);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }

            _logger.Information("Application ended");
        }
    }
}
