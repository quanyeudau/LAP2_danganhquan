using ASC.Model.BaseTypes;

public class MasterDataKey : BaseEntity
{
    public MasterDataKey() { }

    public MasterDataKey(string key, string name)
    {
        this.RowKey = Guid.NewGuid().ToString();
        this.PartitionKey = key;
        this.Name = name;
    }

    public bool IsActive { get; set; }
    public required string Name { get; set; }
}
