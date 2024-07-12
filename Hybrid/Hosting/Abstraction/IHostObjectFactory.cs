namespace Hybrid.Hosting.Abstraction;

public interface IHostObjectFactory
{
    string Name { get; }
    object Create();
}