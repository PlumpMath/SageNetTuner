namespace SageNetTuner.Filters
{
    using System;

    using NLog;

    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public class StopFilter : BaseFilter
    {

        private readonly ExecutableProcessCaptureManager _executableProcessCapture;
        
        public StopFilter(ExecutableProcessCaptureManager executableProcessCaptureManager, Logger logger)
            : base(logger)
        {
            _executableProcessCapture = executableProcessCaptureManager;
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command == CommandName.Stop);
        }

        protected override string OnExecute(RequestContext context)
        {


            Logger.Debug("StopRecording:");
            _executableProcessCapture.Stop();

            return "OK";
        }
    }
}