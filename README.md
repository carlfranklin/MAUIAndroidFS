Create either a new **.NET MAUI App** or a **.NET MAUI Blazor App** called **MAUIAndroidFS**

![image-20230526185550578](images/image-20230526185550578.png)

![image-20230526185607142](images/image-20230526185607142.png)

![image-20230526185617302](images/image-20230526185617302.png)

Add *Platforms/Android/MyBackgroundService.cs*:

```c#
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace MAUIAndroidFS.Platforms.Android;

[Service]
internal class MyBackgroundService : Service
{
    Timer timer = null;
    int myId = (new object()).GetHashCode();
    int BadgeNumber = 0;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, 
        StartCommandFlags flags, int startId)
    {
        var input = intent.GetStringExtra("inputExtra");

        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 
            PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(this, 
                MainApplication.ChannelId)
            .SetContentText(input)
            .SetSmallIcon(Resource.Drawable.AppIcon)
            .SetContentIntent(pendingIntent);

        timer = new Timer(Timer_Elapsed, notification, 0, 10000);

        // You can stop the service from inside the service by calling StopSelf();

        return StartCommandResult.Sticky;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    void Timer_Elapsed(object state)
    {
        AndroidServiceManager.IsRunning = true;

        BadgeNumber++;
        string timeString = $"Time: {DateTime.Now.ToLongTimeString()}";
        var notification = (NotificationCompat.Builder)state;
        notification.SetNumber(BadgeNumber);
        notification.SetContentTitle(timeString);
        StartForeground(myId, notification.Build());

    }
}
```

*/Platforms/Android/MainApplication.cs*:

```c#
using Android.App;
using Android.OS;
using Android.Runtime;

namespace MAUIAndroidFS;

[Application]
public class MainApplication : MauiApplication
{
    public static readonly string ChannelId 
        = "backgroundServiceChannel";

    public MainApplication(IntPtr handle, 
        JniHandleOwnership ownership) : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => 
        MauiProgram.CreateMauiApp();

    public override void OnCreate()
    {
        base.OnCreate();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
#pragma warning disable CA1416
            var serviceChannel =
                new NotificationChannel(ChannelId, 
                    "Background Service Channel", 
                NotificationImportance.High);

            if (GetSystemService(NotificationService) 
                is NotificationManager manager)
            {
                manager.CreateNotificationChannel(serviceChannel);
            }
#pragma warning restore CA1416
        }
    }
}
```



*/Platforms/Android/MainActivity.cs*:

```c#
using Android.App;
using Android.Content.PM;
using Android.Content;
using MAUIAndroidFS.Platforms.Android;

namespace MAUIAndroidFS;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation 
    | ConfigChanges.UiMode | ConfigChanges.ScreenLayout 
    | ConfigChanges.SmallestScreenSize  | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public MainActivity()
    {
        AndroidServiceManager.MainActivity = this;
    }

    public void StartService()
    {
        var serviceIntent = new Intent(this, typeof(MyBackgroundService));
        serviceIntent.PutExtra("inputExtra", "Background Service");
        StartService(serviceIntent);
    }

    public void StopService()
    {
        var serviceIntent = new Intent(this, typeof(MyBackgroundService));
        StopService(serviceIntent);
    }
}
```



*/Platforms/Android/AndroidServiceManager.cs*:

```c#
using Android.Content;

namespace MAUIAndroidFS.Platforms.Android;

public static class AndroidServiceManager
{
    public static MainActivity MainActivity { get; set; }

    public static bool IsRunning { get; set; }

    public static void StartMyService()
    {
        if (MainActivity == null) return;
        MainActivity.StartService();
    }

    public static void StopMyService()
    {
        if (MainActivity == null) return;
        MainActivity.StopService();
        IsRunning = false;
    }
}
```



*/Platforms/Android/BootReceiver.cs*:

```c#
using Android.App;
using Android.Content;
using Android.Widget;
using AndroidX.Core.Content;

namespace MAUIAndroidFS.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted })]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionBootCompleted)
        {
            Toast.MakeText(context, "Boot completed event received", 
                ToastLength.Short).Show();

            var serviceIntent = new Intent(context, 
                typeof(MyBackgroundService));

            ContextCompat.StartForegroundService(context, 
                serviceIntent);
        }
    }
}
```



*/Platforms/Android/AndroidManifext.xml*:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
	<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED" />
	<application android:name="MauiAndroidFS.MainApplication"
                 android:debuggable="true"
                 android:enabled="true"
                 android:allowBackup="true"
				 android:permission="android.permission.RECEIVE_BOOT_COMPLETED"
                 android:icon="@mipmap/appicon"
                 android:roundIcon="@mipmap/appicon_round"
                 android:supportsRtl="true">
		<receiver android:name=".BootReceiver"
                  android:directBootAware="true"
				  android:permission="android.permission.RECEIVE_BOOT_COMPLETED"
                  android:enabled="true"
                  android:exported="true">
			<intent-filter>
				<action android:name="android.intent.action.BOOT_COMPLETED" />
				<category android:name="android.intent.category.DEFAULT" />
			</intent-filter>
		</receiver>
	</application>
	<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
	<uses-permission android:name="android.permission.INTERNET" />
	<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
</manifest>
```

### For MAUI XAML:

Replace *MainPage.xaml* with the following:

```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MAUIAndroidFS.MainPage">

    <VerticalStackLayout Margin="20">
        <Button x:Name="StopButton" Clicked="StopButton_Clicked" Text="Stop Service"></Button>
        <Label x:Name="MessageLabel" FontSize="20" />
    </VerticalStackLayout>

</ContentPage>
```

Replace *MainPage.xaml.cs* with the following:

```c#
namespace MAUIAndroidFS;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        this.Loaded += MainPage_Loaded;
	}

    private void MainPage_Loaded(object sender, EventArgs e)
    {
#if ANDROID
        if (!MAUIAndroidFS.Platforms.Android.AndroidServiceManager.IsRunning)
        {
            MAUIAndroidFS.Platforms.Android.AndroidServiceManager.StartMyService();
            MessageLabel.Text = "Service has started";
        }
        else{
            MessageLabel.Text = "Service is running";
        }
#endif
    }

    private void StopButton_Clicked(object sender, EventArgs e)
    {
#if ANDROID
        MAUIAndroidFS.Platforms.Android.AndroidServiceManager.StopMyService();
        MessageLabel.Text = "Service is stopped";
#endif
    }
}
```

### For MAUI  Blazor:

*/Pages/Index.razor*:

```c#
namespace MAUIAndroidFS;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        this.Loaded += MainPage_Loaded;
	}

    private void MainPage_Loaded(object sender, EventArgs e)
    {
#if ANDROID
        if (!MAUIAndroidFS.Platforms.Android.AndroidServiceManager.IsRunning)
        {
            MAUIAndroidFS.Platforms.Android.AndroidServiceManager.StartMyService();
            MessageLabel.Text = "Service has started";
        }
        else{
            MessageLabel.Text = "Service is running";
        }
#endif
    }

    private void StopButton_Clicked(object sender, EventArgs e)
    {
#if ANDROID
        MAUIAndroidFS.Platforms.Android.AndroidServiceManager.StopMyService();
        MessageLabel.Text = "Service is stopped";
#endif
    }
}
```

**Add */Platforms/Android/Resources/Drawable/AppIcon.png***

Run it on your local android device.

Let it run until you get at least one message, then restart the phone. The messages should start shortly after booting up. 

> :point_up: If after several minutes you do not start seeing notifications, you may be hitting a snag that I hit. Change the name of the `BootReceiver` class in *BootReceiver.cs* and also the reference to it in */Platforms/Android/AndroidManifest.xml*

## Using SignalR to Push Notifications

To the solution add a new **ASP.NET Core Empty** project named **MAUIBroadcastServer**

![image-20230526195719417](images/image-20230526195719417.png)

![image-20230526195732485](images/image-20230526195732485.png)

![image-20230526195811469](images/image-20230526195811469.png)

Replace *Program.cs* with the following:

```c#
global using Microsoft.AspNetCore.SignalR;
using MAUIBroadcastServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapHub<BroadcastHub>("/BroadcastHub");

app.Run();
```

Add the following class:

*BroadcastHub.cs*:

```c#
namespace MAUIBroadcastServer;
public class BroadcastHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.Others.SendAsync("ReceiveMessage", message);
    }
}
```

#### Publish MAUIBroadcastServer to Azure

Make a note of the name of your web app. You'll need it for the next step.

### Add a Console App

Let's add a **Console App** project to the solution to send messages to the SignalR hub. Name it **SendMessageToPhone**:

![image-20230526195956997](images/image-20230526195956997.png)

![image-20230526200005988](images/image-20230526200005988.png)

![image-20230526200012782](images/image-20230526200012782.png)

Add the SignalR client package to the project:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="7.0.5" />
</ItemGroup>
```

Replace *Program.cs* with the following:

```c#
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
```

Make sure to replace `[YOUR-AZURE-SERVER-NAME]` with your actual published web app name.

### Modify the MAUI App to use SignalR

Add the SignalR Client package to the MAUI App:

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="7.0.5" />
```

Replace */Platforms/Android/MyBackgroundService.cs* with the following:

```c#
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.AspNetCore.SignalR.Client;

namespace MAUIAndroidFS.Platforms.Android;

[Service]
internal class MyBackgroundService : Service
{
    Timer timer = null;
    int myId = (new object()).GetHashCode();
    int BadgeNumber = 0;
    NotificationCompat.Builder notification;
    HubConnection hubConnection;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent,
        StartCommandFlags flags, int startId)
    {
        var input = intent.GetStringExtra("inputExtra");

        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent,
            PendingIntentFlags.Immutable);

        notification = new NotificationCompat.Builder(this,
                MainApplication.ChannelId)
            .SetContentText(input)
            .SetSmallIcon(Resource.Drawable.AppIcon)
            .SetContentIntent(pendingIntent);

        BadgeNumber++;
        notification.SetNumber(1);
        notification.SetContentTitle("Service Running");
        StartForeground(myId, notification.Build());

        // timer to ensure hub connection
        timer = new Timer(Timer_Elapsed, notification, 0, 10000);

        // You can stop the service from inside the service by calling StopSelf();

        return StartCommandResult.Sticky;
    }

    async Task EnsureHubConnection()
    {
        if (hubConnection == null)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://[YOUR-AZURE-SERVER-NAME].azurewebsites.net/BroadcastHub")
                .Build();

            hubConnection.On<string>("ReceiveMessage", (message) =>
            {
                BadgeNumber++;
                notification.SetNumber(BadgeNumber);
                notification.SetContentTitle(message);
                StartForeground(myId, notification.Build());
            });
            try
            {
                await hubConnection.StartAsync();
            }
            catch (Exception e)
            {
                // Put a breakpoint on the next line to debug
            }

        }
        else if (hubConnection.State != HubConnectionState.Connected)
        {
            try
            {
                await hubConnection.StartAsync();
            }
            catch (Exception e)
            {
                // Put a breakpoint on the next line to debug
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    async void Timer_Elapsed(object state)
    {
        AndroidServiceManager.IsRunning = true;

        await EnsureHubConnection();
    }
}
```

Again, replace `[YOUR-AZURE-SERVER-NAME]` with your Azure web app name.

Uninstall the app from the Android Phone, and run it again.

Wait until you get the "Service Running" notification, then close the app.

Restart the phone and wait until you get the "Service Running" notification.

> :point_up: If after several minutes you do not start seeing notifications, you may be hitting a snag that I hit. Change the name of the `BootReceiver` class in *BootReceiver.cs* and also the reference to it in */Platforms/Android/AndroidManifest.xml* Delete the app on the phone, and redeploy it

Set the **SendMessageToPhone** project as the startup project and run it.

Enter a message and see if you don't receive it on your phone.

