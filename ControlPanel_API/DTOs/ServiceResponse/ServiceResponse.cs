namespace ControlPanel_API.DTOs.ServiceResponse
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public int? TotalCount { get; set; }

        public ServiceResponse(bool success, string message, T data, int statusCode, int? totalCount = null)
        {
            Success = success;
            Message = message;
            Data = data;
            StatusCode = statusCode;
            TotalCount = totalCount;
        }
    }
}
