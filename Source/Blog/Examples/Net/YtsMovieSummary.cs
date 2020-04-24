namespace Examples.Net
{
    using System;

    public class YtsMovieSummary
    {
        public YtsMovieSummary() // Required by JsonSerializer.
            : this(string.Empty, string.Empty, default, string.Empty, string.Empty, Array.Empty<string>())
        {
        }

        public YtsMovieSummary(string title, string link, int year, string image, string rating, string[] tags)
        {
            this.Title = title;
            this.Link = link;
            this.Year = year;
            this.Image = image;
            this.Rating = rating;
            this.Tags = tags;
        }

        public string Title { get; set; }

        public string Link { get; set; }

        public int Year { get; set; }

        public string Image { get; set; }

        public string Rating { get; set; }

        public string[] Tags { get; set; }
    }
}