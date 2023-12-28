using Android.App;
using Android.Content.PM;
using Android.Content;
using MAUIAndroidFS.Platforms.Android;

namespace MAUIAndroidFS;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
    | ConfigChanges.UiMode | ConfigChanges.ScreenLayout
    | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public MainActivity()
    {
        AndroidServiceManager.MainActivity = this;
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);
        // Handle the intent that you received
        ProcessIntent(intent);
    }

    private void ProcessIntent(Intent intent)
    {
        // Extract data from the intent and use it
        // For example, you can check for a specific action or extract extras
        if (intent != null)
        {
            // Example: checking for a specific action
            var action = intent.Action;
            if (action == "USER_TAPPED_NOTIFIACTION")
            {
                // Handle the specific action
            }
        }
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