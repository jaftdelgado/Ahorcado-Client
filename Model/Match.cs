using System;
using System.Linq;
using System.Windows;

namespace AhorcadoClient.Model
{
    public class Match
    {
        public int MatchID { get; set; }
        public int Player1 { get; set; }
        public int? Player2 { get; set; }
        public int WordID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusID { get; set; }

        public Word Word { get; set; }

        public string Player1Username { get; set; }

        public string CategoryName
        {
            get
            {
                var category = Category.GetDefaultCategories()
                    .FirstOrDefault(c => c.CategoryID == Word?.CategoryID);
                return category?.CategoryName ?? "Unknown";
            }
        }

        public string DifficultyName
        {
            get
            {
                var difficulty = Difficulty.GetDefaultDifficulties()
                    .FirstOrDefault(d => d.DifficultyID == Word?.Difficult);
                return difficulty?.DifficultyName ?? "Unknown";
            }
        }

        public string FormattedCreateDate
        {
            get
            {
                var now = DateTime.Now;
                var diff = now - CreateDate;

                var resources = Application.Current.Resources;

                if (diff.TotalSeconds < 30)
                    return resources["Time_JustNow"]?.ToString() ?? "Hace unos segundos";

                if (diff.TotalSeconds < 60)
                    return string.Format(resources["Time_SecondsAgo"]?.ToString() ?? "Hace {0} s", (int)diff.TotalSeconds);

                if (diff.TotalMinutes < 60)
                    return string.Format(resources["Time_MinutesAgo"]?.ToString() ?? "Hace {0} min", (int)diff.TotalMinutes);

                if (diff.TotalHours < 24)
                    return string.Format(resources["Time_HoursAgo"]?.ToString() ?? "Hace {0} h", (int)diff.TotalHours);

                if (diff.TotalDays < 2)
                    return resources["Time_Yesterday"]?.ToString() ?? "Ayer";

                return string.Format(resources["Time_DaysAgo"]?.ToString() ?? "Hace {0} días", (int)diff.TotalDays);
            }
        }
    }
}