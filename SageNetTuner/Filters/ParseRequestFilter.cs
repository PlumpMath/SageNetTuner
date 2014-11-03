using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SageNetTuner.Filters
{
    using NLog;

    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public class ParseRequestFilter : IFilter<RequestContext, string>
    {
        private readonly Logger _logger;

        public ParseRequestFilter(Logger logger)
        {
            _logger = logger;
        }

        public string Execute(RequestContext context, Func<RequestContext, string> executeNext)
        {
            if (string.IsNullOrEmpty(context.Request) ) 
                throw new Exception("RequestContext.Request cannot be blank");

            var parser = new RequestParser();

            return executeNext(parser.Parse(context.Request));

        }
    }
}
