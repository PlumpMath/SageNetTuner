using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SageNetTuner.Filters
{
    using NLog;

    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public class LogRequestResponseFilter : IFilter<RequestContext,string>
    {
        private readonly Logger _logger;

        public LogRequestResponseFilter(Logger logger)
        {
            _logger = logger;
        }

        public string Execute(RequestContext context, Func<RequestContext, string> executeNext)
        {
            _logger.Debug("========= >> Request [{0}] ==============", context.Request);
            var response =  executeNext(context);
            _logger.Debug("========= << Response [{0}] {1} =========", response, context.TunerState.ToString());

            return response;
        }
    }
}
