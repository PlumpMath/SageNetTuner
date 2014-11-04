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

        private readonly Dictionary<string, RequestCommand> _commands;

        public ParseRequestFilter(Logger logger)
        {
            logger.Trace("ParseRequestFilter.ctor()");

            _logger = logger;


            _commands = new Dictionary<string, RequestCommand>
                            {
                                { "NOOP", RequestCommand.Noop },
                                { "START", RequestCommand.Start },
                                { "BUFFER", RequestCommand.Start },
                                { "BUFFER_SWITCH", RequestCommand.Start },
                                { "STOP", RequestCommand.Stop},
                                { "GET_FILE_SIZE", RequestCommand.GetFileSize },
                                { "VERSION", RequestCommand.Version },
                                { "AUTOINFOSCAN", RequestCommand.AutoInfoScan },
                                { "PORT", RequestCommand.Port },
                                { "GET_SIZE", RequestCommand.GetSize },
                                { "FIRMWARD", RequestCommand.Firmware }
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

            context.RequestCommandName = commandName;

            RequestCommand requestCommand;
            if (_commands.ContainsKey(commandName))
                requestCommand = _commands[commandName];
            else
                requestCommand = RequestCommand.Unknown;


            context.RequestCommand = requestCommand;
            context.CommandArgs = commandArgs;

            return executeNext(context);

        }
    }
}
