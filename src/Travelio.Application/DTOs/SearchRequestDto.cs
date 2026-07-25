using System;
using System.ComponentModel.DataAnnotations;

namespace Travelio.Application.DTOs
{
    public class SearchRequestDto
    {
        [Required, StringLength(3, MinimumLength = 3)]
        public string Origin { get; set; } = null!;
        [Required, StringLength(3, MinimumLength = 3)]
        public string Destination { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Range(1, 9)] public int Passengers { get; set; } = 1;
    }
}
