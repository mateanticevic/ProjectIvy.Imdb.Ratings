using System;

namespace ProjectIvy.Sync.Imdb.Model
{
    class Movie
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ImdbId { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Directors { get; set; }
        public int MyRating { get; set; }
        public decimal? Rating { get; set; }
        public int? Runtime { get; set; }
        public short Year { get; set; }
        public string Genres { get; set; }
        public int? Votes { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
