using TinyCsvParser.Mapping;

namespace ProjectIvy.Imdb.Ratings.Models
{
    class MovieMapping : CsvMapping<Movie>
    {
        public MovieMapping() : base()
        {
            MapProperty(0, x => x.ImdbId);
            MapProperty(1, x => x.MyRating);
            MapProperty(2, x => x.Timestamp);
            MapProperty(3, x => x.Title);
            MapProperty(5, x => x.Type);
            MapProperty(6, x => x.Rating);
            MapProperty(7, x => x.Runtime);
            MapProperty(8, x => x.Year);
            MapProperty(9, x => x.Genres);
            MapProperty(10, x => x.Votes);
            MapProperty(11, x => x.ReleaseDate);
            MapProperty(12, x => x.Directors);
        }
    }
}
