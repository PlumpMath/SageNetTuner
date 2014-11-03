namespace SageNetTuner.Filters
{
    using System.Globalization;

    using NLog;

    using SageNetTuner.Model;

    public class GetSizeFilter : BaseFilter
    {
        private readonly ExecutableProcessCaptureManager _executableProcessCapture;

        public GetSizeFilter(ExecutableProcessCaptureManager executableProcessCapture, Logger logger)
            : base(logger)
        {
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