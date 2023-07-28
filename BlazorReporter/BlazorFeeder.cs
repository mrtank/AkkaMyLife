using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorReporter
{
    public class BlazorFeeder : IBlazorFeeder
    {
        private bool disposedValue;
        private HubConnection _connection;

        public async Task InitConnectionAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/blazor_reporter")
                //.ConfigureLogging(logging =>
                //{
                //    // Log to the Console
                //    logging.AddDebug();

                //    // This will set ALL logging to Debug level
                //    logging.SetMinimumLevel(LogLevel.Trace);
                //})
                .AddMessagePackProtocol()
                .Build();
            await _connection.StartAsync();
        }

        public void LogSuicide(int x, int y, string path)
        {
            LogSuicideAsync(x, y, path).Wait();
        }

        private async Task LogSuicideAsync(int x, int y, string path)
        {
            SuicideLog message = new SuicideLog {X = x, Y = y, Path = path};
            try
            {
                await _connection.InvokeAsync("LogSuicide", message);
            }
            catch
            {
                // ignored
            }
        }

        public void LogCreate(int x, int y, string path)
        {
            LogCreateAsync(x, y, path).Wait();
        }

        private async Task LogCreateAsync(int x, int y, string path)
        {
            CreateLog message = new CreateLog {Path = path, X = x, Y = y};
            try
            {
                await _connection.InvokeAsync("LogCreate", message);
            }
            catch
            {
                // ignored
            }
        }

        public void LogMessage(string from, string to)
        {
            LogMessageAsync(from, to).Wait();
        }

        private async Task LogMessageAsync(string from, string to)
        {
            MessageLog message = new MessageLog {From = from, To = to};
            try
            {
                await _connection.InvokeAsync("LogMessage", message);
            }
            catch
            {
                // ignored
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Task.Run(() => _connection.StopAsync());
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BlazorFeeder()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
