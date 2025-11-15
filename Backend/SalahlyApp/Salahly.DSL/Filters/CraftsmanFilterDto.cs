namespace Salahly.DSL.Filters
{
    /// <summary>
    /// DTO for filtering craftsmen with pagination
    /// </summary>
    public class CraftsmanFilterDto
    {
        /// <summary>
        /// Search by craftsman full name (partial match)
        /// </summary>
        public string? SearchName { get; set; }

        /// <summary>
        /// Filter by craft ID
        /// </summary>
        public int? CraftId { get; set; }

        /// <summary>
        /// Filter by service area ID
        /// </summary>
        //public int? AreaId { get; set; }

        /// <summary>
        /// Filter by Region
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Filter by City
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Filter by availability status
        /// </summary>
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// Minimum rating average (0-5)
        /// </summary>
        public decimal? MinRating { get; set; }

        /// <summary>
        /// Maximum hourly rate
        /// </summary>
        public decimal? MaxHourlyRate { get; set; }

        /// <summary>
        /// Current page number (default: 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of records per page (default: 10, max: 100)
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Validate and normalize filter parameters
        /// </summary>
        public void Validate()
        {
            // Ensure page number is at least 1
            if (PageNumber < 1)
                PageNumber = 1;

            // Ensure page size is between 1 and 100
            if (PageSize < 1)
                PageSize = 10;
            else if (PageSize > 100)
                PageSize = 100;

            // Validate rating range
            if (MinRating.HasValue)
            {
                if (MinRating < 0)
                    MinRating = 0;
                else if (MinRating > 5)
                    MinRating = 5;
            }
        }
    }
}
