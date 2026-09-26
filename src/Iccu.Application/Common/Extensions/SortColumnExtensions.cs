namespace Iccu.Application.Common.Extensions;

using System.Reflection;
using System.Text.RegularExpressions;

public static class SortColumnExtensions
{
    public static HashSet<string> GetSortableColumns<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => !IsCollection(p.PropertyType))
            .Select(p => ToSnakeCase(p.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCollection(Type type)
    {
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type)
               && type != typeof(string);
    }

    private static string ToSnakeCase(string name)
    {
        return Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2", RegexOptions.None, TimeSpan.FromMilliseconds(100)).ToLowerInvariant();
    }
}
