namespace PTickets.Shared.Contracts.FileStorage;

using MediatR;

public record StoreFileCommand(
    Stream Content,
    string FileName,
    string ContentType) : IRequest<FileId>;
