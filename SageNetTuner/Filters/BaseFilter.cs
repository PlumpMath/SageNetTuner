namespace SageNetTuner.Filters
{
    using System;

    using NLog;

    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public abstract class BaseFilter : IFilter<RequestContext, string>
    {
        protected Logger Logger { get; private set; }

        protected BaseFilter(Logger logger)
        {
            Logger = logger;
        }

        protected RequestContext RequestContext;

        protected abstract bool CanExecute(RequestContext context);

        public string Execute(RequestContext context, Func<RequestContext, string> executeNext)
        {

            RequestContext = context;

            if (CanExecute(context))
            {
                return OnExecute(context);
            }

            return executeNext(context);
        }

        protected abstract string OnExecute(RequestContext context);
    }
}