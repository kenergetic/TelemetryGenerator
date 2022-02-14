using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

// Handles writing output to a log or error file

public class LoggedFields 
{
    // Shared
    public string? TimeStamp { get; set; }
    public string? UserName { get; set; }
    public string? ProcessName { get; set; }
    public string? ProcessCommandLine { get; set; }
    public string? ProcessID { get; set; }

    // Command specific
    public string? FullPath { get; set; }
    public string? ActivityDescriptor { get; set; }
    public string? SourceAddress { get; set; }
    public string? SourcePort { get; set; }
    public string? DestinationAddress { get; set; }
    public string? DestinationPort { get; set; }
    public string? Protocol { get; set; }
    public string? DataSize { get; set; }
}

public static class Logger
{
    private static string LOG_PATH = "log.csv";
    private static string ERROR_LOG_PATH = "error.txt";

    // Set filepaths for log files 
    public static void SetLogPaths(string logPath, string errorLogPath) 
    {
        LOG_PATH = logPath;
        ERROR_LOG_PATH = errorLogPath;
    }

    // Log successful telemetry
    public static void LogTelemetry(LoggedFields csvRow)
    {
        // Whether to write a header
        bool logExists = File.Exists(LOG_PATH);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = !logExists,
        };

        using (var stream = File.Open(LOG_PATH, FileMode.Append))
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(new List<LoggedFields> { csvRow });
        }

    }

    // Append/Create an error log with a failed command
    public static void LogError(string[] args, string error) 
    {
        DateTimeOffset dt = DateTimeOffset.UtcNow;
        string errorMessage = dt + ": TelemetryGenerator " + string.Join(' ', args) + ": " + error;
        
        Console.WriteLine(errorMessage);
        using (StreamWriter sw = File.AppendText(ERROR_LOG_PATH))
        {
            sw.WriteLine(errorMessage);
        }
    }

}