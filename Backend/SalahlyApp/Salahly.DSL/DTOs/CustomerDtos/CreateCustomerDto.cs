using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.CustomerDtos
{
    public class CreateCustomerDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }


        public string? Area { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
