namespace SageNetTuner.Model
{
    using SageNetTuner.Configuration;

    public class RequestSettings
    {

        public TunerElement Tuner { get; set; }

        public DeviceElement Device { get; set; }

        public EncoderElement Encoder { get; set; }

        public Lineup Lineup { get; set; }
    }
}