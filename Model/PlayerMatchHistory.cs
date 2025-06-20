using System;
using System.Windows;

namespace AhorcadoClient.Model
{
    public class PlayerMatchHistory
    {
        public int MatchID { get; set; }
        public string OpponentName { get; set; }
        public string PlayedWord { get; set; }
        public int ResultID { get; set; }
        public int Points { get; set; }
        public DateTime? EndDate { get; set; }

        public string ResultName => MatchResult.GetLocalizedResultName(ResultID);

        public string FormattedDate
        {
            get
            {
                if (EndDate == null) return "N/A";

                var now = DateTime.Now;
                var diff = now - EndDate.Value;

                var resources = Application.Current.Resources;

                if (diff.TotalSeconds < 30)
                    return resources["Time_JustNow"]?.ToString() ?? "Just now";

                if (diff.TotalSeconds < 60)
                    return string.Format(resources["Time_SecondsAgo"]?.ToString() ?? "{0} s ago", (int)diff.TotalSeconds);

                if (diff.TotalMinutes < 60)
                    return string.Format(resources["Time_MinutesAgo"]?.ToString() ?? "{0} min ago", (int)diff.TotalMinutes);

                if (diff.TotalHours < 24)
                    return string.Format(resources["Time_HoursAgo"]?.ToString() ?? "{0} h ago", (int)diff.TotalHours);

                if (diff.TotalDays < 2)
                    return resources["Time_Yesterday"]?.ToString() ?? "Yesterday";

                return string.Format(resources["Time_DaysAgo"]?.ToString() ?? "{0} days ago", (int)diff.TotalDays);
            }
        }
    }
}
