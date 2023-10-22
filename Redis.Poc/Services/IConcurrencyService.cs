namespace Redis.Poc.Services
{
    public interface IConcurrencyService
    {
        Task Run();
    }
}