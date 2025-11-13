namespace SalahlyProject.Response
{
    /// <summary>
    /// Generic API response wrapper for uniform API responses
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ApiResponse(int statusCode, string? message, T? data = default)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessage(statusCode);
            Data = data;
        }

        private static string? GetDefaultMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                201 => "Created",
                204 => "No Content",
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Not Found",
                409 => "Conflict",
                500 => "Internal Server Error",
                _ => null
            };
        }
    }

    /// <summary>
    /// Non-generic API response for simple responses without data
    /// </summary>
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
