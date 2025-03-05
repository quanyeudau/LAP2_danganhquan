using ASC.Model.BaseTypes;

public class MasterDataValue : BaseEntity, IAuditTracker
{
    public MasterDataValue() { } // ✅ EF Core cần constructor này để tạo object

    public MasterDataValue(string masterDataPartitionKey, string value, string name)
    {
        this.PartitionKey = masterDataPartitionKey;
        this.RowKey = Guid.NewGuid().ToString();
        this.Name = name;
    }

    public bool IsActive { get; set; }
    public string Name { get; set; }
}
