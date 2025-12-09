using Salahly.DSL.DTOs.ServiceRequstDtos;

namespace Salahly.DSL.DTOs.Booking
{
    public class BookingWithServiceRequestDto
    {
        public BookingDto Booking { get; set; }
        public ServiceRequestDto ServiceRequest { get; set; }

        // Customer contact (explicit)
        public string? CustomerPhone { get; set; }

        // Number of craftsmen who offered (offers count)
        public int CraftsmenCount { get; set; }
    }
}
