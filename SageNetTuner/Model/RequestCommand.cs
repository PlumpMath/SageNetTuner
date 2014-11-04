namespace SageNetTuner.Model
{
    public enum RequestCommand
    {

        Unknown=-1,

        Noop,
        Start,
        Stop,
        Buffer,
        Switch,
        BufferSwitch,
        GetFileSize,
        GetSize,
        Firmware,
        Tune,
        AutoTune,
        Version,
        Port,
        AutoInfoScan,
        Properties,

    }
}