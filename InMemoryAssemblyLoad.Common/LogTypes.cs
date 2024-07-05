namespace InMemoryAssemblyLoad.Common
{
    public enum LogTypes : byte
    {
        [EnumDescription("Error")]
        [EnumShortDescription("ERR")]
        Error = 0,

        [EnumDescription("Warning")]
        [EnumShortDescription("WRN")]
        Warning = 1,

        [EnumDescription("Information")]
        [EnumShortDescription("INF")]
        Information = 2,

        [EnumDescription("Unformatted")]
        [EnumShortDescription("UFT")]
        Unformatted = 3,

        [EnumDescription("Debug")]
        [EnumShortDescription("DBG")]
        Debug = 4,

        [EnumDescription("Trace")]
        [EnumShortDescription("TRC")]
        Trace = 5
    }
}