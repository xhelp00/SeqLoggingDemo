using SeqLoggingDemo;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.PaymentCsv("payments.csv")     // custom sink
    .WriteTo.Seq("http://localhost:5341") // Seq server sink, server running locally in Docker
    .CreateLogger();

var users = new[] { "alice", "bob", "carol", "dave" };
var methods = new[] { "CreditCard", "PayPal", "WireTransfer" };
var rnd = new Random();

for (int i = 0; i < 50; i++)
{
    var user    = users[rnd.Next(users.Length)];
    var orderId = Guid.NewGuid().ToString();
    var total   = Math.Round(rnd.NextDouble() * 200, 2);

    // payment ID for each attempt
    var paymentId = Guid.NewGuid().ToString();
    var method    = methods[rnd.Next(methods.Length)];
    var timestamp = DateTimeOffset.UtcNow;

    var success = rnd.NextDouble() > 0.2;
    if (success)
    {
        // structured + text log
        Log.Information("Order {@Order} placed", new {
            OrderId = orderId,
            User    = user,
            Total   = total,
            Status  = "Success"
        });
        Log.Information("Payment succeeded for user {User} and order {OrderId}",
                        user, orderId);

        // custom CSV event
        Log
            .ForContext("EventType",   "PaymentProcessed")
            .ForContext("PaymentId",   paymentId)
            .ForContext("OrderId",     orderId)
            .ForContext("UserId",      user)
            .ForContext("Amount",      total)
            .ForContext("Currency",    "USD")
            .ForContext("Method",      method)
            .ForContext("Status",      "Succeeded")
            .ForContext("ProcessedAt", timestamp)
            .Information("PaymentProcessed");
    }
    else
    {
        var reason = rnd.NextDouble() > 0.5 ? "Out of stock" : "Payment declined";

        Log.Warning("Order {@Order} failed", new {
            OrderId = orderId,
            User    = user,
            Total   = total,
            Status  = "Failed",
            Reason  = reason
        });
        Log.Warning("Payment failed for order {OrderId}. Reason: {Reason}",
                    orderId, reason);
        
        Log
            .ForContext("EventType",   "PaymentProcessed")
            .ForContext("PaymentId",   paymentId)
            .ForContext("OrderId",     orderId)
            .ForContext("UserId",      user)
            .ForContext("Amount",      total)
            .ForContext("Currency",    "USD")
            .ForContext("Method",      method)
            .ForContext("Status",      "Failed")
            .ForContext("ProcessedAt", timestamp)
            .Information("PaymentProcessed");
    }
}

Log.CloseAndFlush();