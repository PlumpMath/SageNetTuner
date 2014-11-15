using System.Linq;
using System.Text;

namespace SageNetTuner.Model
{
    public class RequestContext
    {

        public RequestContext(string request)
        {
            Request = request;
            Settings = new RequestSettings();
            TunerState = new TunerState();
        }


        public RequestContext(RequestCommand requestCommand, string[] args)
        {
            RequestCommand = requestCommand;
            CommandArgs = args;
            Settings = new RequestSettings();
            TunerState= new TunerState();
        }


        public RequestCommand RequestCommand { get; set; }

        public string RequestCommandName { get; set; }

        public string[] CommandArgs { get; set; }

        public string Request { get; set; }
        
        public string Response { get; set; }

        public RequestSettings Settings { get; private set; }

        public TunerState TunerState { get; set; }
    }
}
