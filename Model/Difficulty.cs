using System.Collections.Generic;
using System.Windows;

namespace AhorcadoClient.Model
{
    public class Difficulty
    {
        public int DifficultyID { get; set; }
        public string DifficultyName { get; set; }
        public string ResourceKey { get; set; }

        public Difficulty(int difficultyId, string nameKeyBase)
        {
            DifficultyID = difficultyId;
            ResourceKey = $"Difficulty_{nameKeyBase}";
            DifficultyName = Application.Current.Resources[ResourceKey]?.ToString() ?? nameKeyBase;
        }

        public override string ToString()
        {
            return DifficultyName;
        }

        public static List<Difficulty> GetDefaultDifficulties()
        {
            return new List<Difficulty>
            {
                new Difficulty(1, "Easy"),
                new Difficulty(2, "Normal"),
                new Difficulty(3, "Hard")
            };
        }
    }
}