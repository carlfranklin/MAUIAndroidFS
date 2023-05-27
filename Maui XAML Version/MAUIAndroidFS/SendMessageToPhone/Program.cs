using Microsoft.AspNetCore.SignalR.Client;

string Message = "";
HubConnection hubConnection;

hubConnection = new HubConnectionBuilder()
.WithUrl("https://[YOUR-AZURE-SERVER-NAME].azurewebsites.net/BroadcastHub")
.Build();

try
{
    await hubConnection.StartAsync();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    return;
}

while (true)
{
    Console.WriteLine("Enter a message to send to the phone, or press ENTER to exit");
    Message = Console.ReadLine();
    if (Message == "")
        break;

    await hubConnection.InvokeAsync("SendMessage", Message);
}