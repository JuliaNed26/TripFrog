namespace TripFrogWebApi.DTO;

public sealed class ServiceResponse<T> :IServiceResponse<T>
{
    public T? Data { get; set; }
    public bool Successful { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}
