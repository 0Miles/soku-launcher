using System;
using System.Windows;

namespace SokuLauncher.Shared
{
    public delegate void ChangeLanguageDelegate(string languageCode);
    public class LanguageService
    {
        public static event ChangeLanguageDelegate OnChangeLanguage;

        public static void ChangeLanguagePublish(string languageCode)
        {
            OnChangeLanguage?.Invoke(languageCode);
        }

        public static string GetString(string key)
        {
            string result = (string)Application.Current.Resources[key];
            if (result != null)
            {

                if (result.Contains("\\r\\n"))
                    result = result.Replace("\\r\\n", Environment.NewLine);

                if (result.Contains("\\n"))
                    result = result.Replace("\\n", Environment.NewLine);
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
