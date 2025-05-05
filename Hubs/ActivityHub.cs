using Microsoft.AspNetCore.SignalR;

namespace BestReads.Hubs;

public class ActivityHub : Hub {
    public async Task SendActivity(Activity activity) {
        // Sends message to all connected clients
        await Clients.All.SendAsync("ReceiveActivity", activity);
    }
}