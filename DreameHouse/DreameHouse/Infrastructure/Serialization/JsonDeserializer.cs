using System.Globalization;
using System.Reflection;

namespace DreameHouse.Infrastructure.Serialization
{
    public static class JsonDeserializer
    {
        public static List<T> Deserialize<T>(string json) where T : new()
        {
            var result = new List<T>();
            var lines = json.Split('\n')
                            .Select(l => l.Trim())
                            .Where(l => l != "[" && l != "]")
                            .ToList();

            T current = default!;
            bool insideObject = false;

            foreach (var line in lines)
            {
                if (line == "{")
                {
                    current = new T();
                    insideObject = true;
                    continue;
                }

                if (line == "}," || line == "}" || line == "},")
                {
                    if (insideObject)
                    {
                        result.Add(current);
                        current = default!;
                        insideObject = false;
                    }
                    continue;
                }

                if (!insideObject || string.IsNullOrWhiteSpace(line))
                    continue;

                int colonIndex = line.IndexOf(':');
                if (colonIndex < 0) continue;

                string propName = line.Substring(0, colonIndex).Trim().Trim('"');
                string rawValue = line.Substring(colonIndex + 1).Trim().Trim(',');

                var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite)
                    continue;

                object? value = ParseValue(rawValue, prop.PropertyType);
                prop.SetValue(current, value);
            }

            return result;
        }

        private static object ParseValue(string raw, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                if (raw == "null")
                    return null;

                targetType = underlyingType;
            }

            if (targetType == typeof(string))
                return raw.Trim('"');

            if (targetType.IsEnum)
                return Enum.Parse(targetType, raw.Trim('"'));

            if (targetType == typeof(int))
                return int.Parse(raw);

            if (targetType == typeof(float))
                return float.Parse(raw);

            if (targetType == typeof(double))
                return double.Parse(raw);

            if (targetType == typeof(bool))
                return raw.ToLower() == "true";

            throw new NotSupportedException($"Cannot parse type {targetType.Name}");
        }

    }
}
