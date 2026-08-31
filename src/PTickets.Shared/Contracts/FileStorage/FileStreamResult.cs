namespace PTickets.Shared.Contracts.FileStorage;

public record FileStreamResult(Stream Content, string FileName, string ContentType);
