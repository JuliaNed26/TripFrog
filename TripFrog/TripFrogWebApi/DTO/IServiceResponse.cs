namespace TripFrogWebApi.DTO;

public interface IServiceResponse<T>
{
    public T? Data { get; }
    public bool Successful { get; }
    public string Message { get; }
}
