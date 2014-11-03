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
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command == CommandName.Version);
        }

        protected override string OnExecute(RequestContext context)
        {
            return "2.1";
        }
    }
}