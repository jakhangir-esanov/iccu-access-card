namespace Iccu.Infrastructure.ObjectStorage;

internal sealed class StorageOptions
{
    internal const string SectionName = "Storage";

    public string RootPath { get; set; } = string.Empty;
}
