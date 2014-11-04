namespace SageNetTuner.Filters
{
    using System;

    using NLog;

    using SageNetTuner.Contracts;
    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public class StopFilter : BaseFilter
    {

        private readonly ICaptureManager _executableProcessCapture;

        public StopFilter(ICaptureManager executableProcessCaptureManager, Logger logger)
            : base(logger)
        {
            logger.Trace("StopFilter.ctor()");

            _executableProcessCapture = executableProcessCaptureManager;
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.RequestCommand == RequestCommand.Stop);
        }

        protected override string OnExecute(RequestContext context)
        {

            Logger.Debug("StopFilter.OnExecute()");
            _executableProcessCapture.Stop();

            return "OK";
        }
    }
}