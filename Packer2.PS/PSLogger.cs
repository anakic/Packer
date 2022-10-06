using Microsoft.Extensions.Logging;
using Packer2.Library.DataModel;
using System.Management.Automation;

namespace Packer2.PS
{
    internal class PSLogger<T> : ILogger<T>
    {
        private readonly Cmdlet cmdlet;

        public PSLogger(Cmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var str = formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Trace:
                    cmdlet.WriteVerbose(str);
                    break;
                case LogLevel.Debug:
                    cmdlet.WriteDebug(str);
                    break;
                case LogLevel.Information:
                    cmdlet.WriteInformation(new InformationRecord(str, state.ToString()));
                    break;
                case LogLevel.Warning:
                    cmdlet.WriteWarning(str);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    cmdlet.WriteError(new ErrorRecord(exception ?? new Exception(str), eventId.ToString(), ErrorCategory.InvalidOperation, state));
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }
        }
    }
}