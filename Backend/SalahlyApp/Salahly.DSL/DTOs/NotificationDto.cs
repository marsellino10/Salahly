using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salahly.DAL.Entities;

namespace Salahly.DSL.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public NotificationType Type { get; set; }

        // Content
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }
    }
}
