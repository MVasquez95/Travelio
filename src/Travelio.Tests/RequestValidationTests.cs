using System.ComponentModel.DataAnnotations;
using Travelio.Application.DTOs;

namespace Travelio.Tests;

public class RequestValidationTests
{
    [Fact]
    public void Search_requires_three_letter_origin_and_destination()
    {
        var request = new SearchRequestDto { Origin = "L", Destination = "LIMA", Passengers = 0 };
        var results = Validate(request);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Booking_requires_client_and_pre_booking()
    {
        var results = Validate(new BookingRequestDto());
        Assert.Equal(2, results.Count);
    }

    private static List<ValidationResult> Validate(object target)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(target, new ValidationContext(target), results, validateAllProperties: true);
        return results;
    }
}
