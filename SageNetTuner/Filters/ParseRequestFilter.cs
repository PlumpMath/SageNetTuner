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

        private readonly Dictionary<string, CommandName> _commands;

        public ParseRequestFilter(Logger logger)
        {
            logger.Trace("ParseRequestFilter.ctor()");

            _logger = logger;


            _commands = new Dictionary<string, CommandName>
                            {
                                { "NOOP", CommandName.Noop },
                                { "START", CommandName.Start },
                                { "BUFFER", CommandName.Start },
                                { "BUFFER_SWITCH", CommandName.Start },
                                { "STOP", CommandName.Stop},
                                { "GET_FILE_SIZE", CommandName.GetFileSize },
                                { "VERSION", CommandName.Version },
                                { "AUTOINFOSCAN", CommandName.AutoInfoScan },
                                { "PORT", CommandName.Port },
                                { "GET_SIZE", CommandName.GetSize },
                                { "FIRMWARD", CommandName.Firmware }
                            };

        }

        public string Execute(RequestContext context, Func<RequestContext, string> executeNext)
        {
            if (string.IsNullOrEmpty(context.Request) ) 
                throw new Exception("RequestContext.Request cannot be blank");

            //example Start rquest
            //START SageDCT-HDHomeRun Prime Tuner 131A21AF-1 Digital TV Tuner|752|2826835203582|D:\Recordings\PropertyBrothers-BeatrizBrandon-17756746-0.ts|Great

            _logger.Debug("ParseRequestFilter.Execute()");

            var commandName = context.Request.Split(new[] { ' ' }, (StringSplitOptions)StringSplitOptions.RemoveEmptyEntries)[0];

            var commandArgs = context.Request
                .Replace(commandName, "")
                .Trim()
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (_logger.IsTraceEnabled)
            {
                _logger.Trace("  Command={0}", commandName);
                _logger.Trace("  CommandArgs:");
                for (int i = 0; i < commandArgs.Length; i++)
                {
                    _logger.Trace("    {0}:{1}", i, commandArgs[i]);
                }
            }


            CommandName command;
            if (_commands.ContainsKey(commandName))
                command = _commands[commandName];
            else
                command = CommandName.Unknown;


            context.Command = command;
            context.CommandArgs = commandArgs;

            return executeNext(context);

        }
    }
}
