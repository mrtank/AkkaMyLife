using System.Threading.Tasks;

namespace BlazorReporter
{
    public class NullLogger: IBlazorFeeder
    {
        public void LogCreate(int x, int y, string path)
        {
        }

        public Task InitConnectionAsync()
        {
            return Task.CompletedTask;
        }

        public void LogSuicide(int x, int y, string path)
        {
        }

        public void LogMessage(string from, string to)
        {
        }

        public void Dispose()
        {
        }
    }
}
