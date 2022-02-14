
    public enum TelemetryCommand 
    {
        START_PROCESS,
        CREATE_FILE,
        MODIFY_FILE,
        DELETE_FILE,
        TRANSMIT_DATA,
        CREATE_REGISTRY_KEY,
        CREATE_REGISTRY_VALUE,
        MODIFY_REGISTRY_KEY,
        MODIFY_REGISTRY_VALUE,
        DELETE_REGISTRY_KEY,
        DELETE_REGISTRY_VALUE,
        
        // Doesn't create a command, just relays arguments
        HELP
    } 
    public enum ActivityDescriptor 
    {
        CREATE,
        MODIFY,
        DELETE,
        CREATE_REGISTRY_KEY,
        CREATE_REGISTRY_VALUE,
        MODIFY_REGISTRY_KEY,
        MODIFY_REGISTRY_VALUE,
        DELETE_REGISTRY_KEY,
        DELETE_REGISTRY_VALUE
    }
