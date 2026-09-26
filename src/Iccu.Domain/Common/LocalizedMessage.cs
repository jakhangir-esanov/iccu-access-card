namespace Iccu.Domain.Common;

public sealed record LocalizedMessage(string En, string Uz, string Ru)
{
    public static readonly LocalizedMessage Empty = new(string.Empty, string.Empty, string.Empty);
}
