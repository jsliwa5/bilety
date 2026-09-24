namespace PTickets.Modules.FileStorage.Contracts;

public record FileStreamResult(Stream Content, string FileName, string ContentType);
