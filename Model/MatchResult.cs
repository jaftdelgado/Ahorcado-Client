using System.Collections.Generic;
using System.Windows;

namespace AhorcadoClient.Model
{
    public class MatchResult
    {
        public int ResultID { get; }
        public string ResourceKey { get; }

        public MatchResult(int resultId, string resourceKey)
        {
            ResultID = resultId;
            ResourceKey = resourceKey;
        }

        public static List<MatchResult> GetDefaultResults()
        {
            return new List<MatchResult>
            {
                new MatchResult(1, "Result_Win"),
                new MatchResult(2, "Result_Lose"),
                new MatchResult(3, "Result_Forfeit"),
                new MatchResult(4, "Result_Cancelled")
            };
        }

        public static string GetLocalizedResultName(int resultId)
        {
            var result = GetDefaultResults().Find(r => r.ResultID == resultId);
            if (result == null)
                return "Unknown";

            var value = Application.Current.Resources[result.ResourceKey];
            return value?.ToString() ?? "Unknown";
        }
    }
}
