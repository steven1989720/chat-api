using Serilog;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChattingAPI
{
    public class ChatServer
    {
        private HttpListener? _listener;
        private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new ConcurrentDictionary<Guid, WebSocket>();

        public async Task Start(string url)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _listener.Start();

            Log.Information("WebSocket server started at {url}", url);

            while (true)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    await ProcessWebSocketRequest(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            Guid clientId = Guid.NewGuid();
            _clients.TryAdd(clientId, webSocket);

            string clientIpAddress = context.Request.RemoteEndPoint.Address.ToString();
            Log.Information("Connected a WebClient from {ip} with id:{guid}", clientIpAddress, clientId);

            while (webSocket.State == WebSocketState.Open)
            {
                // Receive message from client
                var buffer = new byte[1024];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else
                {
                    // Echo message back to client
                    //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }

            Log.Information("Disconnected a WebClient with id:{guid}", clientId);

            webSocket.Dispose();
            _clients.TryRemove(clientId, out _);
        }

        public async Task BroadcastMessageAsync(string message, WebSocket? sender)
        {
            int count = 0;
            foreach (var client in _clients)
            {
                if (client.Value != sender && client.Value.State == WebSocketState.Open)
                {
                    await client.Value.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                    count++;
                }
            }
            Log.Information("Broadcasted to {count} clients", count);
        }

        public void Stop()
        {
            _listener?.Stop();
            _listener?.Close();

            Log.Information("WebSocket server stopped");
        }
    }
}
