using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowScript
{
    public class INI
    {
        private Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

        public string this[string category, string key]
        {
            get
            {
                if (data.ContainsKey(category))
                {
                    var dataInCategory = this[category];

                    if (dataInCategory.ContainsKey(key))
                    {
                        return dataInCategory[key];
                    }
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (!data.ContainsKey(category))
                    {
                        data.Add(category, new Dictionary<string, string>());
                    }

                    if (data[category].ContainsKey(key))
                    {
                        data[category][key] = value;
                    }
                    else
                    {
                        data[category].Add(key, value);
                    }
                }
                else
                {
                    if (data.ContainsKey(category))
                    {
                        if (data[category].ContainsKey(key))
                        {
                            data[category].Remove(key);
                        }
                    }
                }
            }
        }

        public Dictionary<string, string> this[string category]
        {
            get
            {
                if (data.ContainsKey(category))
                {
                    return data[category];
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (data.ContainsKey(category))
                    {
                        data[category] = value;
                    }
                    else
                    {
                        data.Add(category, value);
                    }
                }
                else if (data.ContainsKey(category))
                {
                    data.Remove(category);
                }
            }
        }

        public static INI Parse(string text)
        {
            INI ini = new INI();
            string currentCategory = "";
            string[] lines = text.Split('\n')
                              .Select(x => x.Trim('\r'))
                              .ToArray();

            string[] array = lines;
            foreach (string line in array)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentCategory = line.Substring(1, line.Length - 2);
                    if (!ini.data.ContainsKey(currentCategory))
                    {
                        ini.data.Add(currentCategory, new Dictionary<string, string>());
                    }
                }
                else
                {
                    int equalsIndex = line.IndexOf("=");
                    if (equalsIndex >= 0)
                    {
                        ini[currentCategory, line.Substring(0, equalsIndex)] = line.Substring(equalsIndex + 1);
                    }
                }
            }
            return ini;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Dictionary<string, string>> datum in data)
            {
                if (!string.IsNullOrWhiteSpace(datum.Key))
                {
                    sb.AppendLine($"[{datum.Key}]");
                }
                foreach (KeyValuePair<string, string> item in datum.Value)
                {
                    sb.AppendLine($"{item.Key}={item.Value}");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }
}
