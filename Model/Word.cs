namespace AhorcadoClient.Model
{
    public class Word
    {
        public int WordID { get; set; }

        public int CategoryID { get; set; }

        public int LanguageID { get; set; }

        public string WordText { get; set; }

        public string Description { get; set; }

        public int Difficult { get; set; }

        public override string ToString()
        {
            return WordText;
        }
    }
}