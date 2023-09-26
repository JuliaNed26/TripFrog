using Newtonsoft.Json;

namespace TripFrogMVC;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> DeserializeResponseAsync<T>(this HttpResponseMessage response)
    {
        var jsonResponseString = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonConvert.DeserializeObject<T>(jsonResponseString);

        if (deserializedResponse == null)
        {
            throw new InvalidOperationException($"Can not deserialize response in a type {typeof(T)}");
        }
        return deserializedResponse;
    }
}
