using System.Text;

class Program
{
    // Creates Telemetry for endpoint detection 
    // - this telemetry comes in through arguments and performs a task
    // - this task is saved to a csv log
    // - failed tasks are saved to a csv error log

    // Currently accepted arguments

    // StartProcess <path_to_executable> <arguments>
    // CreateFile <path_to_folder> <filename>
    // ModifyFile <path_to_file>
    // DeleteFile <path_to_file>
    // TransmitData <destination_address> <destination_port>

    // [Windows only]
    // CreateRegistryKey <>
    // ModifyRegistryKey <>
    // DeleteRegistryKey <>
    // CreateRegistryValue <>
    // ModifyRegistryValue <>
    // DeleteRegistryValue <>
    

    // TODO: Move log settings into appsettings.json
    // https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration

    // TODO: Handle large logs

    static void Main(string[] args)
    {

        //foreach(var arg in args) Console.WriteLine(arg);
        CreateTelemetry(args);
    }

    // Performs specific actions based on TelemetryCommand
    // - for example, START_PROCESS will run an executable
    //   - successful telemetry is logged to a csv file
    //   - errors are also logged to a text file on failure or invalid commands
    static void CreateTelemetry(string[] args) 
    { 
        if (args.Length < 1) 
        {
            Logger.LogError(args, "Not enough arguments provided for this command");
            return;
        }

        // Get telemetry command
        string commandString = args[0].ToUpper();
        if (!Enum.TryParse(commandString, out TelemetryCommand command))
        {
            Logger.LogError(new string[] { args[0] }, "Invalid Telemetry command");
        }

        switch(command)
        {
            case TelemetryCommand.START_PROCESS:
                Telemetry.StartProcess(args);
                break;
            case TelemetryCommand.CREATE_FILE:
                Telemetry.CreateFile(args);
                break;
            case TelemetryCommand.MODIFY_FILE:
                Telemetry.ModifyFile(args);
                break;
            case TelemetryCommand.DELETE_FILE:
                Telemetry.DeleteFile(args);
                break;
            case TelemetryCommand.TRANSMIT_DATA:
                Telemetry.TransmitData(args);
                break;
            case TelemetryCommand.CREATE_REGISTRY_KEY:
                Telemetry.CreateRegistryKey(args);
                break;
            case TelemetryCommand.MODIFY_REGISTRY_KEY:
                Telemetry.ModifyRegistryKey(args);
                break;
            case TelemetryCommand.DELETE_REGISTRY_KEY:
                Telemetry.DeleteRegistryKey(args);
                break;
            case TelemetryCommand.CREATE_REGISTRY_VALUE:
                Telemetry.CreateRegistryValue(args);
                break;
            case TelemetryCommand.MODIFY_REGISTRY_VALUE:
                Telemetry.ModifyRegistryValue(args);
                break;
            case TelemetryCommand.DELETE_REGISTRY_VALUE:
                Telemetry.DeleteRegistryValue(args);
                break;

            // todo: help commands
            case TelemetryCommand.HELP:
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("START_PROCESS <full_path>");
                sb.AppendLine("CREATE_FILE <full_path>");
                sb.AppendLine("  * Note: This will overwrite an existing file");
                sb.AppendLine("MODIFY_FILE <full_path> <data>");
                sb.AppendLine("DELETE_FILE <full_path>");
                sb.AppendLine("TRANSMIT_DATA <destination_address> <data>");
                sb.AppendLine("  * Note: This assumes data is being posted as json");
                
                sb.AppendLine("");
                sb.AppendLine(" * Note: The following will only run on a windows machine and creates registry keys under HKEY_CURRENT_USER\\TelemetryRegistry");
                sb.AppendLine("CREATE_REGISTRY_KEY <value>");
                sb.AppendLine("MODIFY_REGISTRY_KEY <old_key> <new_key>");
                sb.AppendLine("DELETE_REGISTRY_KEY <key>");
                sb.AppendLine("CREATE_REGISTRY_VALUE <key> <value_key> <value_value>");
                sb.AppendLine("MODIFY_REGISTRY_VALUE <key> <value_key> <value_value>");
                sb.AppendLine("DELETE_REGISTRY_VALUE <key> <value_key>");

                Console.WriteLine(sb.ToString());
                Console.ReadKey();
                break;
            default:
                break;
        }
    }



}