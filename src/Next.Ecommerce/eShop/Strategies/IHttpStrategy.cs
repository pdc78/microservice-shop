namespace eShop.Strategies;
public interface IHttpStrategy
{
    Task<T?> GetAsync<T>(string url);
    Task PostAsync<T>(string url, T data);
    Task<TResult?> PostAsync<TRequest, TResult>(string url, TRequest data);
    Task DeleteAsync(string url);
}
