using System;

namespace dotnet_nats
{
    public interface ITransport : dotnet_sockets.ISocketClient
    {
    }
}
