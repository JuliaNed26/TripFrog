namespace TripFrogWebApi.DTO;

public interface IResponse<T>
{
    public T Data { get; }
    public bool Successful { get; }
    public string Message { get; } 
}
