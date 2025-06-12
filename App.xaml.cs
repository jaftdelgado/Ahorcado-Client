using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AhorcadoClient
{
    public partial class App : Application
    {
        public App()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            ChangeCulture("es-MX");
        }

        public void ChangeCulture(string cultureCode)
        {
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
    }
}
