﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SageNetTuner.Filters
{
    using NLog;

    using SageNetTuner.Model;

    public class NoopFilter : BaseFilter
    {
        public NoopFilter(Logger logger)
            : base(logger)
        {
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command == CommandName.Noop);
        }

        protected override string OnExecute(RequestContext context)
        {
            return "OK";
        }
    }
}
