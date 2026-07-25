using System;

namespace Travelio.Domain.Models
{
    public class Provider
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string Metadata { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
