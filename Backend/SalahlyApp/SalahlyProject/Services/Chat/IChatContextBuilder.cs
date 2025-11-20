using System.Threading;
using System.Threading.Tasks;

namespace SalahlyProject.Services.Chat
{
    public interface IChatContextBuilder
    {
        Task<string> BuildContextAsync(string question, string? providedContext, CancellationToken cancellationToken = default);
    }
}
