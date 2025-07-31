namespace FACEPALM.Services
{
    public interface ILogger
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogDebug(string message, params object[] args);
    }

    public class ConsoleLogger : ILogger
    {
        private readonly string _categoryName;

        public ConsoleLogger(string categoryName)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        }

        public void LogInformation(string message, params object[] args)
        {
            WriteLog("INFO", message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            WriteLog("WARN", message, args, ConsoleColor.Yellow);
        }

        public void LogError(string message, params object[] args)
        {
            WriteLog("ERROR", message, args, ConsoleColor.Red);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            WriteLog("ERROR", $"{string.Format(message, args)}\nException: {exception}", [], ConsoleColor.Red);
        }

        public void LogDebug(string message, params object[] args)
        {
            WriteLog("DEBUG", message, args, ConsoleColor.Gray);
        }

        private void WriteLog(string level, string message, object[] args, ConsoleColor? color = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            var logMessage = $"[{timestamp}] [{level}] [{_categoryName}] {formattedMessage}";

            if (color.HasValue)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.WriteLine(logMessage);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                Console.WriteLine(logMessage);
            }
        }
    }
}