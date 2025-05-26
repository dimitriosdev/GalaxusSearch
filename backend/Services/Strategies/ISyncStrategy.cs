namespace Backend.Api.Services.Strategies
{
  public interface ISyncStrategy
  {
    void SyncProducts(string connectionString);
  }
}
