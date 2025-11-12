namespace SalahlyProject.Response.Error
{
    public class ValidationErrorResponse: ApiResponse
    {
        public IEnumerable<string> Errors { get; set; }
        public ValidationErrorResponse() : base(400, null)
        {
            Errors = new List<string>();
        }
    }
}
