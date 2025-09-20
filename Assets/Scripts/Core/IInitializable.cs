using System.Threading.Tasks;

/// <summary>
/// Interface for components that require synchronous initialization
/// </summary>
public interface IInitializable
{
    void Initialize();
}

/// <summary>
/// Interface for components that require asynchronous initialization
/// </summary>
public interface IAsyncInitializable
{
    Task InitializeAsync();
}