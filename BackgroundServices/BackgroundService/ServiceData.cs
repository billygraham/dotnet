using System.Collections.Concurrent;

namespace BackgroundService;

public class ServiceData
{
   public ConcurrentBag<string> Data { get; set; } = new();
}
