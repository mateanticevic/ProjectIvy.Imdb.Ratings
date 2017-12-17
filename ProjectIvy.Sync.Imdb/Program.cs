using Microsoft.Extensions.Configuration;
using ProjectIvy.Sync.Imdb.Model;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using TinyCsvParser;

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

                var csvOptions = new CsvParserOptions(true, ',');
                var movieMapping = new MovieMapping();
                var csvParser = new CsvParser<Movie>(csvOptions, movieMapping);

                var csvReaderOptions = new CsvReaderOptions(new[] {"\n"});
                var movies = csvParser.ReadFromString(csvReaderOptions, imdbRatings).Where(x => x.IsValid).Select(x => x.Result).ToList();

                var newMovies = movies.Where(x => !existingIds.Contains(x.ImdbId));

                foreach (var newMovie in newMovies)
                {
                    newMovie.UserId = user.userId;
                    await DbHandler.InsertMovie(connectionString, newMovie);
                    Console.WriteLine($"Movie: {newMovie.Title}, User: {newMovie.UserId}");
                }
            }
        }
    }
}
