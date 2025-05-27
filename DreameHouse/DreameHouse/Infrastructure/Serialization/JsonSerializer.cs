using System.Reflection;
using System.Globalization;

namespace DreameHouse.Infrastructure.Serialization
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(List<T> objects)
        {
            var lines = new List<string> { "[" };

            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                var type = typeof(T);
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                lines.Add("  {");
                for (int j = 0; j < props.Length; j++)
                {
                    var prop = props[j];
                    var value = prop.GetValue(obj);
                    string formattedValue = value switch
                    {
                        null => "null",
                        string str => $"\"{str}\"",
                        Enum => $"\"{value}\"", 
                        _ => Convert.ToString(value, CultureInfo.InvariantCulture)
                    };

                    bool isLast = j == props.Length - 1;
                    lines.Add($"    \"{prop.Name}\": {formattedValue}" + (isLast ? "" : ","));
                }
                lines.Add(i == objects.Count - 1 ? "  }" : "  },");
            }

            lines.Add("]");
            return string.Join(Environment.NewLine, lines);
        }
    }
}
