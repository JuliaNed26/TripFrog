namespace TripFrogWebApi.DTO;

public sealed class Response<T> :IResponse<T>
{
    public T Data { get; set; }
    public bool Successful { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}
