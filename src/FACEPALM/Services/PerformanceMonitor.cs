using System.Diagnostics;

namespace FACEPALM.Services
{
    public interface IPerformanceMonitor
    {
        void StartOperation(string operationName);
        void EndOperation(string operationName);
        void LogMemoryUsage(string context);
        void LogPerformanceMetrics();
        PerformanceMetrics GetMetrics(string operationName);
    }

    public class PerformanceMetrics
    {
        public string OperationName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public long StartMemoryBytes { get; set; }
        public long EndMemoryBytes { get; set; }
        public long PeakMemoryBytes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public long MemoryDelta => EndMemoryBytes - StartMemoryBytes;
        public string FormattedDuration => Duration.ToString(@"mm\:ss\.fff");
        public string FormattedMemoryDelta => FormatBytes(MemoryDelta);
        public string FormattedPeakMemory => FormatBytes(PeakMemoryBytes);

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            var absBytes = Math.Abs(bytes);
            int suffixIndex = 0;
            double value = absBytes;

            while (value >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1024;
                suffixIndex++;
            }

            var sign = bytes < 0 ? "-" : "";
            return $"{sign}{value:F2} {suffixes[suffixIndex]}";
        }
    }

    public class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly Dictionary<string, PerformanceMetrics> _activeOperations = new();
        private readonly Dictionary<string, PerformanceMetrics> _completedOperations = new();
        private readonly ILogger _logger;
        private readonly Process _currentProcess;

        public PerformanceMonitor(ILogger? logger = null)
        {
            _logger = logger ?? new ConsoleLogger(nameof(PerformanceMonitor));
            _currentProcess = Process.GetCurrentProcess();
        }

        public void StartOperation(string operationName)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

            var metrics = new PerformanceMetrics
            {
                OperationName = operationName,
                StartTime = DateTime.UtcNow,
                StartMemoryBytes = GetCurrentMemoryUsage()
            };

            _activeOperations[operationName] = metrics;
            
            _logger.LogDebug("Started operation: {OperationName} | Memory: {StartMemory}", 
                operationName, metrics.FormattedPeakMemory);
        }

        public void EndOperation(string operationName)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

            if (!_activeOperations.TryGetValue(operationName, out var metrics))
            {
                _logger.LogWarning("Attempted to end operation that was not started: {OperationName}", operationName);
                return;
            }

            metrics.EndTime = DateTime.UtcNow;
            metrics.Duration = metrics.EndTime - metrics.StartTime;
            metrics.EndMemoryBytes = GetCurrentMemoryUsage();
            metrics.PeakMemoryBytes = GetPeakMemoryUsage();

            _activeOperations.Remove(operationName);
            _completedOperations[operationName] = metrics;

            _logger.LogInformation("Completed operation: {OperationName} | Duration: {Duration} | Memory Delta: {MemoryDelta} | Peak Memory: {PeakMemory}",
                operationName, metrics.FormattedDuration, metrics.FormattedMemoryDelta, metrics.FormattedPeakMemory);
        }

        public void LogMemoryUsage(string context)
        {
            var currentMemory = GetCurrentMemoryUsage();
            var peakMemory = GetPeakMemoryUsage();
            
            _logger.LogDebug("Memory usage [{Context}]: Current: {CurrentMemory} | Peak: {PeakMemory}", 
                context, PerformanceMetrics.FormatBytes(currentMemory), PerformanceMetrics.FormatBytes(peakMemory));
        }

        public void LogPerformanceMetrics()
        {
            if (_completedOperations.Count == 0)
            {
                _logger.LogInformation("No completed operations to report");
                return;
            }

            _logger.LogInformation("=== Performance Summary ===");
            
            foreach (var kvp in _completedOperations.OrderBy(x => x.Value.StartTime))
            {
                var metrics = kvp.Value;
                _logger.LogInformation("{OperationName}: {Duration} | Memory: {MemoryDelta} | Peak: {PeakMemory}",
                    metrics.OperationName, metrics.FormattedDuration, metrics.FormattedMemoryDelta, metrics.FormattedPeakMemory);
            }

            var totalDuration = _completedOperations.Values.Sum(m => m.Duration.TotalMilliseconds);
            var totalMemoryDelta = _completedOperations.Values.Sum(m => m.MemoryDelta);
            var maxPeakMemory = _completedOperations.Values.Max(m => m.PeakMemoryBytes);

            _logger.LogInformation("Total Duration: {TotalDuration:F2}ms | Total Memory Delta: {TotalMemoryDelta} | Max Peak: {MaxPeakMemory}",
                totalDuration, PerformanceMetrics.FormatBytes(totalMemoryDelta), PerformanceMetrics.FormatBytes(maxPeakMemory));
        }

        public PerformanceMetrics GetMetrics(string operationName)
        {
            ArgumentException.ThrowIfNullOrEmpty(operationName, nameof(operationName));

            if (_completedOperations.TryGetValue(operationName, out var completedMetrics))
            {
                return completedMetrics;
            }

            if (_activeOperations.TryGetValue(operationName, out var activeMetrics))
            {
                // Return current state for active operations
                activeMetrics.EndTime = DateTime.UtcNow;
                activeMetrics.Duration = activeMetrics.EndTime - activeMetrics.StartTime;
                activeMetrics.EndMemoryBytes = GetCurrentMemoryUsage();
                activeMetrics.PeakMemoryBytes = GetPeakMemoryUsage();
                return activeMetrics;
            }

            throw new InvalidOperationException($"Operation '{operationName}' not found");
        }

        private long GetCurrentMemoryUsage()
        {
            try
            {
                _currentProcess.Refresh();
                return _currentProcess.WorkingSet64;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get current memory usage: {Error}", ex.Message);
                return 0;
            }
        }

        private long GetPeakMemoryUsage()
        {
            try
            {
                _currentProcess.Refresh();
                return _currentProcess.PeakWorkingSet64;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to get peak memory usage: {Error}", ex.Message);
                return 0;
            }
        }

        public void Dispose()
        {
            _currentProcess?.Dispose();
        }
    }
}