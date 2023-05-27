namespace MAUIBroadcastServer;
public class BroadcastHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.Others.SendAsync("ReceiveMessage", message);
    }
}