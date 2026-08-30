using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace FilePilot.Services;

public interface IStartupService
{
    bool IsAutoStartEnabled();
    void EnableAutoStart();
    void DisableAutoStart();
}

public class StartupService : IStartupService
{
    private const string AppName = "FilePilot";
    private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public bool IsAutoStartEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
        if (key == null) return false;
        
        var value = key.GetValue(AppName) as string;
        return !string.IsNullOrEmpty(value);
    }

    public void EnableAutoStart()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
        if (key != null)
        {
            // Get executing assembly location returns the .dll in .NET 5+, we need the .exe
            string exePath = Assembly.GetExecutingAssembly().Location;
            exePath = Path.ChangeExtension(exePath, ".exe");
            
            // Add --minimized flag to indicate it should start hidden
            key.SetValue(AppName, $"\"{exePath}\" --minimized");
        }
    }

    public void DisableAutoStart()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
        key?.DeleteValue(AppName, false);
    }
}
