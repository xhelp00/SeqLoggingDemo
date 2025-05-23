using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace SeqLoggingDemo;

public class PaymentCsvSink : ILogEventSink, IDisposable
{
    readonly StreamWriter _writer;

    public PaymentCsvSink(string path)
    {
        _writer = new StreamWriter(path, append: true) { AutoFlush = true };
        if (new FileInfo(path).Length == 0)
            _writer.WriteLine("PaymentId,OrderId,UserId,Amount,Currency,Method,Status,ProcessedAt");
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("EventType", out var et) 
            && et.ToString().Trim('"') == "PaymentProcessed")
        {
            // List out the CSV columns once
            var columns = new[] {
                "PaymentId", "OrderId", "UserId", "Amount",
                "Currency", "Method", "Status", "ProcessedAt"
            };

            // Build each value (or empty) and join
            var values = columns.Select(name =>
                logEvent.Properties.TryGetValue(name, out var prop)
                    ? prop.ToString().Trim('"')
                    : string.Empty
            );

            _writer.WriteLine(string.Join(",", values));
        }
    }

    public void Dispose() => _writer.Dispose();
}

// 2) Extension method for easy configuration
public static class PaymentCsvSinkExtensions
{
    public static LoggerConfiguration PaymentCsv(
        this LoggerSinkConfiguration cfg,
        string path)
    {
        return cfg.Sink(new PaymentCsvSink(path));
    }
}