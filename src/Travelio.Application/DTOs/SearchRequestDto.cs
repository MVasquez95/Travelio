using System;

namespace Travelio.Application.DTOs
{
    public class SearchRequestDto
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Passengers { get; set; } = 1;
    }
}
