using System.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace GameConsole
{
    [EventSource(Name = "Microsoft-OnboardingApplication-GameConsole")]
    public sealed class GameConsoleEventSource : EventSource
    {
        public static readonly GameConsoleEventSource Current = new GameConsoleEventSource();

        static GameConsoleEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        // Instance constructor is private to enforce singleton semantics
        private GameConsoleEventSource() : base() { }


        [NonEvent]
        public void Message(string message, params object[] args)
        {
            if (IsEnabled())
            {
                string finalMessage = string.Format(message, args);
                Message(finalMessage);
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
        public void Message(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(MessageEventId, message);
            }
        }
    }
}
