namespace SageNetTuner.Contracts
{
    using SageNetTuner.Configuration;
    using SageNetTuner.Model;

    public interface IChannelProvider
    {
        Lineup GetLineup(DeviceElement deviceSettings);
    }
}
