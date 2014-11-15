using System;
using System.Collections.Generic;

namespace TestClient
{

    using TestClient.Commandline;

    public class MessageBuilder
    {

        private readonly List<IMessageBuilder> _builders;

        public MessageBuilder()
        {

            _builders = new List<IMessageBuilder>
                            {
                                new StartMessageBuilder(),
                                new StopMessageBuilder(),
                                new NoopMessageBuilder(),
                                new GetFileSizeMessageBuilder(),
                                new AutoInfoScanMessageBuilder(),
                                new VersionMessageBuilder()
                            };
        }

        public string GetMessage(string verb, object options)
        {

            foreach (var handler in _builders)
            {
                if (handler.CanHandle(verb))
                {
                    return handler.Handle(options);
                }
            }
            throw new Exception("Unknown VERB");
        }
    }


    public class StartMessageBuilder : IMessageBuilder
    {
        public bool CanHandle(string verb)
        {
            return (String.Compare(verb, "start", StringComparison.InvariantCultureIgnoreCase)==0);
        }

        public string Handle(object options)
        {
            var startOptions =(StartSubOptions)options;

            var message = string.Format(
                "START {0}|{1}|{2}|{3}|{4}",
                startOptions.Tuner,
                startOptions.Channel,
                "0000",
                startOptions.Filename,
                startOptions.Quality);


            return message;

        }
    }

    public class StopMessageBuilder : IMessageBuilder
    {
        public bool CanHandle(string verb)
        {
            return (String.Compare(verb, "stop", StringComparison.InvariantCultureIgnoreCase) == 0);
            
        }

        public string Handle(object options)
        {
            return "STOP";
        }
    }

    public class NoopMessageBuilder : IMessageBuilder
    {
        public bool CanHandle(string verb)
        {
            return (String.Compare(verb, "noop", StringComparison.InvariantCultureIgnoreCase) == 0);

        }

        public string Handle(object options)
        {
            return "NOOP";
        }
    }


    public class VersionMessageBuilder : IMessageBuilder
    {
        public bool CanHandle(string verb)
        {
            return (String.Compare(verb, "version", StringComparison.InvariantCultureIgnoreCase) == 0);

        }

        public string Handle(object options)
        {
            return "VERSION";
        }
    }

    public class AutoInfoScanMessageBuilder : IMessageBuilder
    {
        public bool CanHandle(string verb)
        {
            return (String.Compare(verb, "autoinfoscan", StringComparison.InvariantCultureIgnoreCase) == 0);

        }

        public string Handle(object options)
        {
            return "AUTOINFOSCAN 0";
        }
    }

    public class GetFileSizeMessageBuilder : IMessageBuilder
    {
        public bool CanHandle(string verb)
        {
            return (String.Compare(verb, "filesize", StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public string Handle(object options)
        {
            var fileSizeOptions = (FileSizeSubOptions)options;

            if (string.IsNullOrEmpty(fileSizeOptions.Filename))
            {
                return "GET_SIZE";
            }

            return string.Format("GET_FILE_SIZE {0}", fileSizeOptions.Filename);
        }
    }

}
