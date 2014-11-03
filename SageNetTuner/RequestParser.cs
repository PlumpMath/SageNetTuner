namespace SageNetTuner.Model
{
    using System;
    using System.Collections.Generic;

    using NLog;

    using SageNetTuner.Contracts;

    public class RequestParser : IRequestParser
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, CommandName> _commands;

        public RequestParser()
        {
            _commands = new Dictionary<string, CommandName>
                            {
                                { "NOOP", CommandName.Noop },
                                { "START", CommandName.Start },
                                { "BUFFER", CommandName.Start },
                                { "BUFFER_SWITCH", CommandName.Start },
                                { "GET_FILE_SIZE", CommandName.GetFileSize },
                                { "VERSION", CommandName.Version },
                                { "AUTOINFOSCAN", CommandName.AutoInfoScan },
                                { "PORT", CommandName.Port },
                                { "GET_SIZE", CommandName.GetSize },
                                { "FIRMWARD", CommandName.Firmware }
                            };
        }

        public RequestContext Parse(string request)
        {

            //example Start rquest
            //START SageDCT-HDHomeRun Prime Tuner 131A21AF-1 Digital TV Tuner|752|2826835203582|D:\Recordings\PropertyBrothers-BeatrizBrandon-17756746-0.ts|Great

            var commandName = request.Split(new[] { ' ' }, (StringSplitOptions)StringSplitOptions.RemoveEmptyEntries)[0];

            var commandArgs = request
                .Replace(commandName, "")
                .Trim()
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (Logger.IsTraceEnabled)
            {
                Logger.Trace("  Command={0}", commandName);
                Logger.Trace("  CommandArgs:");
                for (int i = 0; i < commandArgs.Length; i++)
                {
                    Logger.Trace("    {0}:{1}", i, commandArgs[i]);
                }
            }


            CommandName command;
            if (_commands.ContainsKey(commandName)) 
                command= _commands[commandName];
            else 
                command = CommandName.Unknown;

            return new RequestContext(command, commandArgs);
            
        }
    }
}