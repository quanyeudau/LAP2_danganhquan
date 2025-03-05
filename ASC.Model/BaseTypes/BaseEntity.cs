namespace ASC.Model.BaseTypes
{
    public class BaseEntity
    {
        public required string PartitionKey { get; set; }
        public required string RowKey { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public required string CreatedBy { get; set; }
        public required string UpdatedBy { get; set; }

        public BaseEntity() { }
    }
}
