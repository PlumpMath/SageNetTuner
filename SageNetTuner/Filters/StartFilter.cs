namespace SageNetTuner.Filters
{
    using System;
    using System.Linq;

    using NLog;

    using SageNetTuner.Contracts;
    using SageNetTuner.Model;

    public class StartFilter :  BaseFilter
    {
        private readonly ICaptureManager _executableProcessCapture;


        public StartFilter(ICaptureManager executableProcessCaptureManager, Logger logger)
            : base(logger)
        {
            logger.Trace("StartFilter.ctor()");

            _executableProcessCapture = executableProcessCaptureManager;
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.RequestCommand == RequestCommand.Start);
        }

        protected override string OnExecute(RequestContext context)
        {
            Logger.Trace("StartFilter.OnExecute()");

            //var command = new StartCommand(context.CommandArgs[1], context.CommandArgs[3]);

            var channel = context.CommandArgs[1];
            var filename = context.CommandArgs[3];

            Logger.Debug("StartRecording(): Channel={0}, Filename={1}", channel, filename);

            try
            {
                // Find the requested channel to get the URL
                var ch = (from x in context.Settings.Lineup.Channels where x.GuideNumber == channel select x).FirstOrDefault();
                if (ch != null)
                {
                    Logger.Debug("StartRecording(): Found Requested Channel: GuideName={0}, GuideNumber={1}, URL={2}", ch.GuideName, ch.GuideNumber, ch.URL);

                    _executableProcessCapture.Start(ch, filename);

                    Logger.Trace("StartRecording(): Recording Started");

                    context.TunerState.RecordingStarted(filename,ch);

                    return "OK";
                }
                else
                {
                    context.TunerState.RecordingStopped();

                    _executableProcessCapture.Stop();

                    Logger.Warn("StartRecording(): Channel not found");
                    return string.Format("ERROR Channel not found in device lineup. {0}", channel);
                }

            }
            catch (Exception ex)
            {
                context.TunerState.RecordingStopped();
                Logger.Error("StartRecording(): Exception trying start recording", ex);
                return string.Format("ERROR {0}", ex.Message);
            }
        }

    }
}