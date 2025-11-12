namespace SalahlyProject.Response
{
    public class ApiResponse
    {
        public int Statuscode { get; set; }
        public string Message { get; set; }

        public ApiResponse(int stutseCode, string? message)
        {
            Statuscode = stutseCode;
            Message = message ?? GetDefaultMessage(stutseCode);
        }

        private string? GetDefaultMessage(int stutseCode)
        {
            return stutseCode switch
            {
                400 => "A Bad Request",
                401 => "UnAuthorized",
                404 => "Resource Was Not Found",
                500 => "Internal Server Error",
                _ => null
            };
        }
    }
}
