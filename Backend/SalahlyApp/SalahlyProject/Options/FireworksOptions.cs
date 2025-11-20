namespace SalahlyProject.Options
{
    public class FireworksOptions
    {
        public const string SectionName = "Fireworks";

        public string BaseUrl { get; set; } = "https://api.fireworks.ai/inference/v1";
        public string ApiKey { get; set; } = "fw_3ZbZueFQEaBhpmDWaWdLCNUc";
        public string Model { get; set; } = "accounts/fireworks/models/llama-v3p1-8b-instruct";
    }
}
