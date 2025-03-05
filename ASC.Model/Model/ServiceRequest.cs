using ASC.Model.BaseTypes;

public class ServiceRequest : BaseEntity, IAuditTracker
{
    public ServiceRequest() { }

    public ServiceRequest(string email, string vehicleName, string vehicleType, string status, string requestedServices, string serviceEngineer)
    {
        this.RowKey = Guid.NewGuid().ToString();
        this.PartitionKey = email;
        this.VehicleName = vehicleName;
        this.VehicleType = vehicleType;
        this.Status = status;
        this.RequestedServices = requestedServices;
        this.ServiceEngineer = serviceEngineer;
    }

    public string VehicleName { get; set; }
    public string VehicleType { get; set; }
    public string Status { get; set; }
    public string RequestedServices { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string ServiceEngineer { get; set; }
}
