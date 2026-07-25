using System;

namespace Travelio.Domain.Models;

public class Search
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = "anonymous";
    public string Criteria { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
