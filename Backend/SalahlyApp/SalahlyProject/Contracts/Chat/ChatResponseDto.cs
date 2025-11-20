namespace SalahlyProject.Contracts.Chat
{
    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public bool IsFallback { get; set; }
            = false;
    }
}
