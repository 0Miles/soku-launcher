using System;
using System.Windows;

namespace SokuLauncher
{
    public delegate void CnahgeLanguageDelegate(string languageCode);
    public class LanguageService
    {
        public event CnahgeLanguageDelegate OnChangeLanguage;

        public void ChangeLanguagePublish(string languageCode)
        {
            OnChangeLanguage?.Invoke(languageCode);
        }

        public string GetString(string key)
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
