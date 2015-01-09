namespace SageNetTuner.Model
{
    using SageNetTuner.Configuration;

    public class RequestSettings
    {

        public TunerElement Tuner { get; set; }

        public DeviceElement Device { get; set; }

        public CaptureProfileElement CaptureProfile { get; set; }

        public Lineup Lineup { get; set; }
    }
}