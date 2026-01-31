using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace FileUploader.Api.Hubs
{
    public class UploadHub : Hub
    {
        public static readonly ConcurrentDictionary<Guid, string> UploadConnections = new();
        
        public Task RegisterUpload(Guid fileId)
        {
            UploadConnections[fileId] = Context.ConnectionId;
            return Task.CompletedTask;
        }
        
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var deadConnections = UploadConnections.Where(x => x.Value == Context.ConnectionId)
                                    .Select(x => x.Key).ToList();
            foreach (var deadConnection in deadConnections)
            {
                UploadConnections.TryRemove(deadConnection, out _);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public static bool TryGetConnection(Guid fileId, out string connectionId)
            => UploadConnections.TryGetValue(fileId, out connectionId);
    }
}
