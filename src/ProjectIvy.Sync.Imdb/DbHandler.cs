using Dapper;
using ProjectIvy.Sync.Imdb.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ProjectIvy.Sync.Imdb
{
    static class DbHandler
    {
        public static async Task<IEnumerable<(int userId, string imdbUsername)>> GetImdbUsers(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                const string sql = @"SELECT Id, ImdbId FROM [User].[User] WHERE ImdbId IS NOT NULL";

                return await connection.QueryAsync<(int, string)>(sql);
            }
        }

        public static async Task<IEnumerable<string>> GetMovieIds(string connectionString, int userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                const string sql = @"SELECT ImdbId FROM [User].Movie WHERE UserId = @UserId";

                var query = new
                {
                    UserId = userId
                };

                return await connection.QueryAsync<string>(sql, query);
            }
        }

        public static async Task InsertMovie(string connectionString, Movie movie)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                const string sql = @"INSERT INTO [User].[Movie]
                                       ([UserId]
                                       ,[ImdbId]
                                       ,[Timestamp]
                                       ,[Title]
                                       ,[Type]
                                       ,[Directors]
                                       ,[MyRating]
                                       ,[Rating]
                                       ,[Runtime]
                                       ,[Year]
                                       ,[Genres]
                                       ,[Votes]
                                       ,[ReleaseDate])
                                 VALUES
                                       (@UserId
                                       ,@ImdbId
                                       ,@Timestamp
                                       ,@Title
                                       ,@Type
                                       ,@Directors
                                       ,@MyRating
                                       ,@Rating
                                       ,@Runtime
                                       ,@Year
                                       ,@Genres
                                       ,@Votes
                                       ,@ReleaseDate)";

                await connection.ExecuteAsync(sql, movie);
            }
        }
    }
}
