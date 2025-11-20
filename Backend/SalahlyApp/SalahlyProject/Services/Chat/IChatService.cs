using System.Threading;
using System.Threading.Tasks;

namespace SalahlyProject.Services.Chat
{
    public interface IChatService
    {
        Task<(string Answer, bool IsFallback)> AskAsync(string question, string? context, CancellationToken cancellationToken = default);
    }
}
