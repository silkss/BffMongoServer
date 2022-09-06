namespace ContainerStore.Data.Settings;

public class ContainerStoreDatabaseSettings
{
    public string? ConnectionString { get; set; } = null!;
    public string? DatabaseName { get; set; } = null!;
    public string? ContainersCollectionName { get; set; } = null!;
}

