namespace Muflone.Persistence.Azure.Models;

public record ResolvedEvent(byte[] Metadata, byte[] Data);