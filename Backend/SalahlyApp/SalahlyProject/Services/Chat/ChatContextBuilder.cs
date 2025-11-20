using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SalahlyProject.Services.Chat
{
    public class ChatContextBuilder : IChatContextBuilder
    {
        private readonly string _rootPath;

        private static readonly string[] AllowedExtensions =
        {
            ".cs", ".ts", ".html", ".md"
        };

        private static readonly string[] BaseContextFiles =
        {
            Path.Combine("SalahlyProject", "Docs", "customer-assistant-context.md"),
            Path.Combine("SalahlyProject", "Docs", "customer-faq.md"),
            Path.Combine("SalahlyProject", "Docs", "technician-faq.md")
        };

        private static readonly string[] AllowedDirectories =
        {
            "Salahly.DAL", "Salahly.DSL", "SalahlyProject"
        };

        private static readonly IReadOnlyDictionary<string, string[]> KeywordBuckets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["registration"] = new[] { "register", "signup", "customer", "craftsman" },
            ["login"] = new[] { "login", "signin", "auth" },
            ["service"] = new[] { "service", "request", "booking" },
            ["technician"] = new[] { "technician", "craftsman" },
            ["greeting"] = new[] { "hello", "hi", "hey", "greetings" },
            ["general"] = new[] { "auth", "registration", "service", "controller", "customer" }
        };

        private const int MaxContextLength = 4000;

        public ChatContextBuilder()
        {
            _rootPath = ResolveSolutionRoot();
        }

        public Task<string> BuildContextAsync(string question, string? providedContext, CancellationToken cancellationToken = default)
        {
            var normalizedProvidedContext = string.IsNullOrWhiteSpace(providedContext)
                ? null
                : NormalizeWhitespace(providedContext);

            var keywords = ExtractKeywords(question);
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(normalizedProvidedContext))
            {
                builder.AppendLine("// Conversation Context");
                builder.AppendLine(normalizedProvidedContext);
                builder.AppendLine();
            }

            AppendBaseContext(builder);

            foreach (var snippet in CollectRelevantSnippets(keywords, cancellationToken))
            {
                builder.AppendLine($"// File: {Path.GetRelativePath(_rootPath, snippet.Path)}");
                builder.AppendLine(snippet.Content);
                builder.AppendLine();
            }

            return Task.FromResult(builder.Length > 0 ? builder.ToString() : string.Empty);
        }

        private static HashSet<string> ExtractKeywords(string question)
        {
            var text = question.ToLowerInvariant();
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in KeywordBuckets)
            {
                if (entry.Value.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                {
                    keywords.Add(entry.Key);
                }
            }

            if (keywords.Count == 0)
            {
                keywords.Add("general");
            }

            return keywords;
        }

        private static string NormalizeWhitespace(string value)
        {
            return string.Join("\n", value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.TrimEnd()));
        }

        private bool IsUnderAllowedDirectory(string filePath)
        {
            var relativePath = Path.GetRelativePath(_rootPath, filePath);
            if (relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                return false;
            }

            var firstSegment = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
            return firstSegment != null && AllowedDirectories.Any(directory => string.Equals(directory, firstSegment, StringComparison.OrdinalIgnoreCase));
        }

        private static string ResolveSolutionRoot()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null && !File.Exists(Path.Combine(current.FullName, "SalahlyApp.sln")))
            {
                current = current.Parent;
            }

            return current?.FullName ?? AppContext.BaseDirectory;
        }

        private void AppendBaseContext(StringBuilder builder)
        {
            foreach (var relativePath in BaseContextFiles)
            {
                var fullPath = Path.Combine(_rootPath, relativePath);
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                try
                {
                    var content = File.ReadAllText(fullPath);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }

                    builder.AppendLine($"// File: {Path.GetRelativePath(_rootPath, fullPath)}");
                    builder.AppendLine(content);
                    builder.AppendLine();
                }
                catch (IOException)
                {
                    // Skip files that cannot be read.
                }
            }
        }

        private IEnumerable<(string Path, string Content)> CollectRelevantSnippets(HashSet<string> keywords, CancellationToken cancellationToken)
        {
            var scoredFiles = new List<(string Path, string Content, int Score)>();

            foreach (var file in Directory.EnumerateFiles(_rootPath, "*.*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!AllowedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!IsUnderAllowedDirectory(file))
                {
                    continue;
                }

                string content;
                try
                {
                    content = File.ReadAllText(file);
                }
                catch (IOException)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                var score = CalculateKeywordScore(content, keywords);
                if (score <= 0)
                {
                    continue;
                }

                if (content.Length > MaxContextLength)
                {
                    content = content.Substring(0, MaxContextLength);
                }

                scoredFiles.Add((file, content, score));
            }

            return scoredFiles
                .OrderByDescending(entry => entry.Score)
                .ThenBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .Select(entry => (entry.Path, entry.Content));
        }

        private static int CalculateKeywordScore(string content, HashSet<string> keywords)
        {
            if (keywords.Count == 0)
            {
                return 1;
            }

            var score = 0;
            foreach (var keyword in keywords)
            {
                var occurrences = CountOccurrences(content, keyword);
                score += occurrences;
            }

            return score;
        }

        private static int CountOccurrences(string content, string keyword)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            var index = 0;
            var count = 0;

            while ((index = content.IndexOf(keyword, index, comparison)) >= 0)
            {
                count++;
                index += keyword.Length;
            }

            return count;
        }
    }
}
