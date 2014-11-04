using System.Linq;
using System.Text;

namespace SageNetTuner.Model
{
    using SageNetTuner.Configuration;

    public class RequestContext
    {

        public RequestContext(string request)
        {
            Request = request;
            Settings = new RequestSettings();
        }


        public RequestContext(RequestCommand requestCommand, string[] args)
        {
            RequestCommand = requestCommand;
            CommandArgs = args;
            Settings = new RequestSettings();
        }


        public RequestCommand RequestCommand { get; set; }

        public string RequestCommandName { get; set; }

        public string[] CommandArgs { get; set; }

        public string Request { get; set; }
        
        public string Response { get; set; }

        public RequestSettings Settings { get; private set; }


    }

    public class RequestSettings
    {

        public TunerElement Tuner { get; set; }

        public DeviceElement Device { get; set; }

        public EncoderElement Encoder { get; set; }

        public Lineup Lineup { get; set; }
    }

}
