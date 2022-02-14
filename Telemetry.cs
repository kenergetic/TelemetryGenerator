using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// What each command does
public static class Telemetry
{
    // For IO Exceptions, retry a couple times
    private const int NumberOfRetries = 2;
    private const int BaseDelayOnRetry = 1000;

    // Start a process 
    public static void StartProcess(string[] args) 
    {
        if (!HasEnoughArgs(args)) return;

        string exePath = args[1];
        string[] additionalArgs = args.Skip(2).ToArray();

        try 
        {
            // Run this command
            var processInfo = new ProcessStartInfo {
                FileName = exePath,
                Arguments = string.Join(' ', additionalArgs).Trim(),
                UseShellExecute = false,
            };
            var process = Process.Start(processInfo);
            
            // Log Action
            if (process == null) 
            {
                Logger.LogError(args, "Process does not exist");
                return;
            }
            
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = process.ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = process.Id.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }
    
    // File CRUD
    public static void CreateFile(string[] args) 
    {
        if (!HasEnoughArgs(args)) return;

        string filePath = args[1];

        // Retry on file IO exceptions
        for(int i=1; i<=NumberOfRetries; i++) 
        {
            try 
            {
                // Run this command
                File.Create(filePath).Close();
                
                // Log Action
                LoggedFields log = new LoggedFields 
                {
                    TimeStamp = DateTimeOffset.Now.ToString(),
                    UserName = Environment.UserName,
                    ProcessName = Process.GetCurrentProcess().ProcessName,
                    ProcessCommandLine = string.Join(' ', args),
                    ProcessID = Process.GetCurrentProcess().Id.ToString(),
                    ActivityDescriptor = ActivityDescriptor.CREATE.ToString()
                };
                Logger.LogTelemetry(log);
            }
            catch(IOException) when (i <= NumberOfRetries)
            {
                Thread.Sleep(1000 * i);
            }
            catch(Exception ex) 
            {
                Logger.LogError(args, ex.Message);
                return;
            }
        }
    }
    
    public static void ModifyFile(string[] args) 
    {
        if (!HasEnoughArgs(args, 3)) return;
        if (!FileExists(args, args[1])) return;

        string filePath = args[1];
        string data = args[2];
        
        // Retry on file IO exceptions
        for(int i=1; i<=NumberOfRetries; i++) 
        {
            try 
            {
                // Run this command
                File.WriteAllText(filePath, data);
                
                // Log Action
                LoggedFields log = new LoggedFields 
                {
                    TimeStamp = DateTimeOffset.Now.ToString(),
                    UserName = Environment.UserName,
                    ProcessName = Process.GetCurrentProcess().ProcessName,
                    ProcessCommandLine = string.Join(' ', args),
                    ProcessID = Process.GetCurrentProcess().Id.ToString(),
                    ActivityDescriptor = ActivityDescriptor.MODIFY.ToString()
                };
                Logger.LogTelemetry(log);
            }
            catch(IOException) when (i <= NumberOfRetries)
            {
                Thread.Sleep(1000 * i);
            }
            catch(Exception ex) 
            {
                Logger.LogError(args, ex.Message);
                return;
            }
        }
    }

    public static void DeleteFile(string[] args) 
    {
        if (!HasEnoughArgs(args)) return;
        if (!FileExists(args, args[1])) return;

        string filePath = args[1];

        // Retry on file IO exceptions
        for(int i=1; i<=NumberOfRetries; i++) 
        {
            try 
            {
                // Run this command
                File.Delete(filePath);
                
                // Log Action
                LoggedFields log = new LoggedFields 
                {
                    TimeStamp = DateTimeOffset.Now.ToString(),
                    UserName = Environment.UserName,
                    ProcessName = Process.GetCurrentProcess().ProcessName,
                    ProcessCommandLine = string.Join(' ', args),
                    ProcessID = Process.GetCurrentProcess().Id.ToString(),
                    ActivityDescriptor = ActivityDescriptor.DELETE.ToString()
                };
                Logger.LogTelemetry(log);
            }
            catch(IOException) when (i <= NumberOfRetries)
            {
                Thread.Sleep(1000 * i);
            }
            catch(Exception ex) 
            {
                Logger.LogError(args, ex.Message);
                return;
            }
        }
    }

    // Sending Http / other data
    public static void TransmitData(string[] args)
    {
        if (!HasEnoughArgs(args, 3)) return;

        // <destination_address> <destination_port> <data>
        string destinationAddress = args[1];
        string data = args[2];
        int port = 80;

        try 
        {
            string json = JsonSerializer.Serialize(data);
            var jsonData = new StringContent(json, Encoding.UTF8, "application/json");
            var url = destinationAddress;

            // Run this command
            using (var client = new HttpClient()) 
            {
                client.BaseAddress = new Uri(destinationAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                port = client.BaseAddress.Port;
                
                // Synchronous / blocking 
                // - if we just want to send data and log we did this, we could make this async
                var response = client.PostAsync(client.BaseAddress.AbsoluteUri, jsonData).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                
                DestinationAddress = destinationAddress,
                DestinationPort = port.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }

    // Registry CRUD (windows)
    public static void CreateRegistryKey(string[] args) 
    {
        if (!HasEnoughArgs(args)) return;
        if (!IsWindows(args)) return;

        string key = args[1];

        try 
        {
            // Run this command
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                Microsoft.Win32.RegistryKey baseRegistryKey;  
                using (baseRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TelemetryRegistry"))
                {
                    baseRegistryKey.CreateSubKey(key);
                    baseRegistryKey.Close();
                }
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                ActivityDescriptor = ActivityDescriptor.CREATE_REGISTRY_KEY.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }
    
    public static void ModifyRegistryKey(string[] args) 
    {
        if (!HasEnoughArgs(args, 3)) return;
        if (!IsWindows(args)) return;

        string oldKey = args[1];
        string newKey = args[2];

        try 
        {
            // Run this command
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                Microsoft.Win32.RegistryKey? baseRegistryKey;
                Microsoft.Win32.RegistryKey? oldRegistryKey;
                Microsoft.Win32.RegistryKey? newRegistryKey;
                using (baseRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TelemetryRegistry"))
                {
                    if (baseRegistryKey != null) 
                    {
                        using (oldRegistryKey = baseRegistryKey.OpenSubKey(oldKey)) 
                        {
                            // If the old key exists, copy its values
                            // - does not do recursive copying
                            if (oldRegistryKey != null) 
                            {
                                using(newRegistryKey = baseRegistryKey.CreateSubKey(newKey)) 
                                {
                                    foreach (var valueName in oldRegistryKey.GetValueNames())
                                    {
                                        newRegistryKey.SetValue(valueName, 
                                            oldRegistryKey.GetValue(valueName), 
                                            oldRegistryKey.GetValueKind(valueName));
                                    }
                                }
                            }
                        }
                        baseRegistryKey.DeleteSubKey(oldKey);
                    }
                }
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                ActivityDescriptor = ActivityDescriptor.MODIFY_REGISTRY_KEY.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }
    public static void DeleteRegistryKey(string[] args) 
    {
        if (!HasEnoughArgs(args)) return;
        if (!IsWindows(args)) return;

        string key = args[1];

        try 
        {
            // Run this command
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                Microsoft.Win32.RegistryKey baseRegistryKey;  
                using (baseRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TelemetryRegistry"))
                {
                    baseRegistryKey.DeleteSubKey(key);
                    baseRegistryKey.Close();
                }
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                ActivityDescriptor = ActivityDescriptor.DELETE_REGISTRY_KEY.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }

    public static void CreateRegistryValue(string[] args) 
    {
        if (!HasEnoughArgs(args, 4)) return;
        if (!IsWindows(args)) return;

        string key = args[1];
        string valueName = args[2];
        string valueValue = args[3];

        try 
        {
            // Run this command
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                Microsoft.Win32.RegistryKey? baseRegistryKey;
                Microsoft.Win32.RegistryKey? registryKey;
                using (baseRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TelemetryRegistry"))
                {
                    if (baseRegistryKey != null) 
                    {
                        using (registryKey = baseRegistryKey.OpenSubKey(key)) 
                        {
                            if (registryKey != null)
                            {
                                registryKey.SetValue(valueName, valueValue, Microsoft.Win32.RegistryValueKind.String);
                            }
                        }
                    }
                }
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                ActivityDescriptor = ActivityDescriptor.CREATE_REGISTRY_VALUE.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }
    
    public static void ModifyRegistryValue(string[] args)
    {
        if (!HasEnoughArgs(args, 4)) return;
        if (!IsWindows(args)) return;

        string key = args[1];
        string valueName = args[2];
        string valueValue = args[3];

        try 
        {
            // Run this command
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                Microsoft.Win32.RegistryKey? baseRegistryKey;
                Microsoft.Win32.RegistryKey? registryKey;
                using (baseRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TelemetryRegistry"))
                {
                    if (baseRegistryKey != null) 
                    {
                        using (registryKey = baseRegistryKey.OpenSubKey(key)) 
                        {
                            if (registryKey != null)
                            {
                                registryKey.SetValue(valueName, valueValue, Microsoft.Win32.RegistryValueKind.String);
                            }
                        }
                    }
                }
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                ActivityDescriptor = ActivityDescriptor.MODIFY_REGISTRY_VALUE.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }
    
    public static void DeleteRegistryValue(string[] args)
    {
        if (!HasEnoughArgs(args, 3)) return;
        if (!IsWindows(args)) return;

        string key = args[1];
        string valueName = args[2];

        try 
        {
            // Run this command
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                Microsoft.Win32.RegistryKey? baseRegistryKey;
                Microsoft.Win32.RegistryKey? registryKey;
                using (baseRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("TelemetryRegistry"))
                {
                    if (baseRegistryKey != null) 
                    {
                        using (registryKey = baseRegistryKey.OpenSubKey(key)) 
                        {
                            if (registryKey != null)
                            {
                                registryKey.DeleteValue(valueName);
                            }
                        }
                    }
                }
            }
            
            // Log Action
            LoggedFields log = new LoggedFields 
            {
                TimeStamp = DateTimeOffset.Now.ToString(),
                UserName = Environment.UserName,
                ProcessName = Process.GetCurrentProcess().ProcessName,
                ProcessCommandLine = string.Join(' ', args),
                ProcessID = Process.GetCurrentProcess().Id.ToString(),
                ActivityDescriptor = ActivityDescriptor.DELETE_REGISTRY_VALUE.ToString()
            };
            Logger.LogTelemetry(log);
        }
        catch(Exception ex) 
        {
            Logger.LogError(args, ex.Message);
            return;
        }
    }

    #region Helpers

    // Basic check for valid number of arguments
    private static bool HasEnoughArgs(string[] args, int minimum=2) 
    {
        if (args.Length >= minimum) return true;

        Logger.LogError(args, "Not enough arguments provided for this command");
        return false;
    }

    // Basic check if a file exists
    private static bool FileExists(string[] args, string fullPath) 
    {
        if (File.Exists(fullPath)) return true;

        Logger.LogError(args, "File does not exist");
        return false;
    }

    private static bool IsWindows(string[] args)
    {
        bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows) return true;
        
        Logger.LogError(args, "This command can only be run on a Windows OS");
        return false;
    }

    #endregion

}