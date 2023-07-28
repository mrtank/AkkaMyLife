using System.Linq;
using BlazorReporter;

namespace CellCalculation
{
    using Akka.Actor;
    using Akka.Event;

    public class Logger : ReceiveActor
    {
        public static IBlazorFeeder Feeder;
        public Logger()
        {
            Receive<Debug>(e =>
            {
                if (e.LogClass.FullName == "CellCalculation.Cell" && e.Message is string message && message.Contains("received handled message"))
                {
                    var splitted = message.Split(' ');
                    var from = splitted.Last();
                    Feeder.LogMessage(from, e.LogSource);
                    System.Diagnostics.Debug.WriteLine($"{from} => {e.LogSource} {splitted[3]}");
                }
            });
            Receive<Info>(e => this.Log(LogLevel.InfoLevel, e.ToString()));
            Receive<Warning>(e => this.Log(LogLevel.WarningLevel, e.ToString()));
            Receive<Error>(e => this.Log(LogLevel.ErrorLevel, e.ToString()));
            Receive<InitializeLogger>(_ => Sender.Tell(new LoggerInitialized()));
        }

        private void Log(LogLevel level, string message)
        {
        }
    }
}
