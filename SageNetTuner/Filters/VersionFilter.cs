namespace SageNetTuner.Filters
{
    using System;

    using NLog;

    using SageNetTuner.Model;

    public class VersionFilter : BaseFilter
    {
        public VersionFilter(Logger logger)
            : base(logger)
        {
            logger.Trace("VersionFilter.ctor()");

        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.RequestCommand == RequestCommand.Version);
        }

        protected override string OnExecute(RequestContext context)
        {
            Logger.Trace("VersionFilter.OnExecute()");

            return "2.1";
        }
    }
}