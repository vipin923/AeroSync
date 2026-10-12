namespace AeroSync.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        // Notice how Data is now type 'T' instead of a hardcoded string
        public T? Data { get; set; }

        public string? Error { get; set; }
    }
}
