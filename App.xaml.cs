using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AhorcadoClient
{
    public partial class App : Application
    {
        public static string CurrentCultureCode { get; set; } = "es-MX";

        public App()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            ChangeCulture(CurrentCultureCode);
        }

        public void ChangeCulture(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode)) return;

            CurrentCultureCode = cultureCode;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);

            var dictionaries = Current.Resources.MergedDictionaries;

            var languageDictionaries = dictionaries
                .Where(d => d.Source != null && d.Source.OriginalString.Contains("Strings."))
                .ToList();

            foreach (var dict in languageDictionaries)
                dictionaries.Remove(dict);

            var newDict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Properties/Strings.{cultureCode}.xaml", UriKind.Absolute)
            };
            dictionaries.Add(newDict);
        }

        public static void ApplyCurrentCulture()
        {
            var app = (App)Current;
            app.ChangeCulture(CurrentCultureCode);
        }
    }
}
