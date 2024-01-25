using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SokuModManager
{
    public class IniFile
    {
        string IniFilePath;

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string section, byte[] key, byte[] val, string filePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        public IniFile(string iniPath)
        {
            IniFilePath = new FileInfo(iniPath).FullName;
        }

        public string Read(string key, string section)
        {
            byte[] buffer = new byte[1024];
            int bufLen = GetPrivateProfileString(section, key, "", buffer, buffer.GetUpperBound(0), IniFilePath);
            return Encoding.UTF8.GetString(buffer, 0, bufLen);
        }

        public void Write(string key, string value, string section)
        {
            WritePrivateProfileString(section, key == null ? null : Encoding.UTF8.GetBytes(key), value == null ? null : Encoding.UTF8.GetBytes(value), IniFilePath);
        }

        public void DeleteKey(string key, string section)
        {
            Write(key, null, section);
        }

        public void EnableKey(string key, string section)
        {
            List<string> iniLineList = File.ReadAllLines(IniFilePath).ToList();
            bool inSection = false;
            for (int i = 0; i < iniLineList.Count; i++)
            {
                string line = iniLineList[i];
                if (line.Contains($"[{section}]"))
                {
                    inSection = true;
                }
                else if (new Regex(@"^\s*\[.*\]\s*$").IsMatch(line))
                {
                    inSection = false;
                }

                if (inSection && line.Contains(key))
                {
                    iniLineList[i] = new Regex($@";\s*{key}\s*=\s*(.*)\s*").Replace(line, $@"{key}=$1");
                    break;
                }
            }

            WriteLinesWithoutBom(iniLineList);
        }

        public void DisableKey(string key, string section)
        {
            List<string> iniLineList = File.ReadAllLines(IniFilePath).ToList();
            bool inSection = false;
            for (int i = 0; i < iniLineList.Count; i++)
            {
                string line = iniLineList[i];
                if (line.Contains($"[{section}]"))
                {
                    inSection = true;
                }
                else if (new Regex(@"^\s*\[.*\]\s*$").IsMatch(line))
                {
                    inSection = false;
                }

                if (inSection && line.Contains(key))
                {
                    iniLineList[i] = new Regex($@"^\s*{key}\s*=\s*(.*)\s*").Replace(line, $@";{key}=$1");
                    break;
                }
            }

            WriteLinesWithoutBom(iniLineList);
        }

        private void WriteLinesWithoutBom(IEnumerable<string> lines)
        {
            using (StreamWriter writer = new StreamWriter(IniFilePath, false, new UTF8Encoding(false)))
            {
                foreach (string line in lines)
                {
                    writer.WriteLine(line);
                }
                writer.Close();
            }
        }

        public void DeleteSection(string section)
        {
            Write(null, null, section);
        }

        public bool KeyExists(string key, string section)
        {
            return !string.IsNullOrEmpty(Read(key, section));
        }
    }
}
