using Microsoft.AspNetCore.SignalR;

namespace vATMS.Hubs;

public class TargetGeneratorHub : Hub
{
    public async Task NewMessage(string username, string message)
    {
        await Clients.All.SendAsync("messageReceived", username, message);
    }

    public async Task IncrementTestValue(string callsign, int value)
    {
        await Clients.All.SendAsync("testValueIncremented", callsign, value + 1);
    }
}