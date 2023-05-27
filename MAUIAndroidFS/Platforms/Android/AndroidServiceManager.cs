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