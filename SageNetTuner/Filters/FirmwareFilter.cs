using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SageNetTuner.Filters
{
    using System.Reflection;

    using NLog;

    using SageNetTuner.Model;

    using Tamarack.Pipeline;

    public class FirmwareFilter : BaseFilter
    {
        public FirmwareFilter(Logger logger)
            : base(logger)
        {
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command == CommandName.Firmware);
        }


        protected override string OnExecute(RequestContext context)
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}
