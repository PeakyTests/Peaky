using Microsoft.Extensions.Logging;

namespace Peaky
{
    internal class PeakyLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) =>
            new PeakyLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
