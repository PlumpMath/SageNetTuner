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
            return (context.RequestCommand == RequestCommand.GetSize);
        }

        protected override string OnExecute(RequestContext context)
        {
            Logger.Trace("GetSizeFilter.OnExecute()");

            var length = _executableProcessCapture.GetFileSize();
            Logger.Trace("GetSize(): Length={1:n0}", length);

            return length.ToString(CultureInfo.InvariantCulture);
        }
    }
}