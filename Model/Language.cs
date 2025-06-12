using System.Collections.Generic;
using System.Windows;

namespace AhorcadoClient.Model
{
    public class Language
    {
        public int LanguageID { get; set; }
        public string LanguageName { get; set; }
        public string ResourceKey { get; set; }

        public Language(int languageId, string nameKeyBase)
        {
            LanguageID = languageId;
            ResourceKey = $"Language_{nameKeyBase}";
            LanguageName = Application.Current.Resources[ResourceKey]?.ToString() ?? nameKeyBase;
        }

        public override string ToString()
        {
            return LanguageName;
        }

        public static List<Language> GetDefaultLanguages()
        {
            return new List<Language>
            {
                new Language(1, "Spanish"),
                new Language(2, "English")
            };
        }
    }
}
