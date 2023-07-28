using System.Threading.Tasks;

namespace BlazorReporter
{
    using System;

    public interface IBlazorFeeder: IDisposable
    {
        void LogCreate(int x, int y, string path);
        Task InitConnectionAsync();
        void LogSuicide(int x, int y, string path);
        void LogMessage(string from, string to);
    }
}