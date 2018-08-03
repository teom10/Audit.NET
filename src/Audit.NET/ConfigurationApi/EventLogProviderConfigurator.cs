using System;

namespace Audit.Core.ConfigurationApi
{
    public class EventLogProviderConfigurator : IEventLogProviderConfigurator
    {
        public string _logName = "Application";
        public string _sourcePath = "Application";
        public string _machineName = ".";
        public Func<AuditEvent, string> _messageBuilder;


        public IEventLogProviderConfigurator LogName(string logName)
        {
            _logName = logName;
            return this;
        }

        public IEventLogProviderConfigurator MachineName(string machineName)
        {
            _machineName = machineName;
            return this;
        }

        public IEventLogProviderConfigurator SourcePath(string sourcePath)
        {
            _sourcePath = sourcePath;
            return this;
        }

        public IEventLogProviderConfigurator MessageBuilder(Func<AuditEvent, string> messageBuilder)
        {
            _messageBuilder = messageBuilder;
            return this;
        }
    }
}
