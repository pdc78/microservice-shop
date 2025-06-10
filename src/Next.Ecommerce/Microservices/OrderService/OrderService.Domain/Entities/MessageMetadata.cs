namespace OrderService.Domain.Entities;
public class MessageMetadata
{
    public string MessageType { get; set; } = default!;
    public Guid OrderId { get; set; }
    public string JsonBody { get; set; } = default!;
}
