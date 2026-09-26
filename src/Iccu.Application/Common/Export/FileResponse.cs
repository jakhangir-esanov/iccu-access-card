namespace Iccu.Application.Common.Export;

public sealed record FileResponse(byte[] Content, string ContentType, string FileName);
