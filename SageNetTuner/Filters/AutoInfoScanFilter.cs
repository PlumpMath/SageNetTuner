namespace SageNetTuner.Filters
{
    using System;
    using System.Runtime.Remoting.Contexts;
    using System.Text;

    using NLog;

    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public class AutoInfoScanFilter : BaseFilter
    {

        public AutoInfoScanFilter(Logger logger)
            : base(logger)
        {
            logger.Trace("AutoInfoScanFilter.ctor()");

        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command == CommandName.AutoInfoScan);
        }


        protected override string OnExecute(RequestContext context)
        {
            Logger.Debug("AutoInfoScanFilter.OnExecute()");

            var op = "";
            if (context.CommandArgs.Length > 0)
                op = context.CommandArgs[0];
            return GetAvailableChannels(op);
        }

        private string GetAvailableChannels(string commandArg)
        {
            commandArg = commandArg.Trim();

            Logger.Info("GetAvailableChannels: {0}", commandArg);
            if (commandArg == "0")
            {
                var sb = new StringBuilder();
                foreach (var ch in RequestContext.Settings.Lineup.Channels)
                {
                    sb.AppendFormat("{0};", ch.GuideNumber);
                }
                return sb.ToString();
            }

            return "OK";
        }

    }
}