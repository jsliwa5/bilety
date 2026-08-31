namespace PTickets.Shared.Contracts.FileStorage;

using MediatR;

public record GetFileQuery(FileId FileId) : IRequest<FileStreamResult>;
