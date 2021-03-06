namespace Terminals.Connections
{
    /// <summary>
    /// Support implementation to be able resolve current connection by the plugin
    /// </summary>
    public interface ICurrenctConnectionProvider
    {
        IConnection CurrentConnection { get; }
    }
}