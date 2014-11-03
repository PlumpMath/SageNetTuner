namespace SageNetTuner.Filters
{
    using System.Globalization;

    using NLog;

    using SageNetTuner.Contracts;
    using SageNetTuner.Model;

    public class GetSizeFilter : BaseFilter
    {
        private readonly ICaptureManager _executableProcessCapture;

        public GetSizeFilter(ICaptureManager executableProcessCapture, Logger logger)
            : base(logger)
        {
            logger.Trace("GetSizeFilter.ctor()");
            _executableProcessCapture = executableProcessCapture;
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command == CommandName.GetSize);
        }

        protected override string OnExecute(RequestContext context)
        {
            return _executableProcessCapture.GetFileSize().ToString(CultureInfo.InvariantCulture);
        }
    }
}