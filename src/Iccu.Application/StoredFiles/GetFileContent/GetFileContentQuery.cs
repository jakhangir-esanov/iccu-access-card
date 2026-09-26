namespace Iccu.Application.StoredFiles.GetFileContent;

using Dapper;
using Iccu.Domain.Common;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Storage;

public sealed record GetFileContentQuery(Guid Id) : IQuery<FileContentResponse>;

public sealed record FileContentResponse(Stream Content, string ContentType, string OriginalName);

internal sealed class GetFileContentQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IFileStore fileStore) : IQueryHandler<GetFileContentQuery, FileContentResponse>
{
    public async Task<Result<FileContentResponse>> Handle(GetFileContentQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                storage_path AS {nameof(ContentRow.StoragePath)},
                content_type AS {nameof(ContentRow.ContentType)},
                original_name AS {nameof(ContentRow.OriginalName)}
            FROM iccu.stored_files
            WHERE id = @Id
            """;

        ContentRow? row = await connection.QuerySingleOrDefaultAsync<ContentRow>(
            new CommandDefinition(sql, new { request.Id }, cancellationToken: cancellationToken));

        if (row is null)
        {
            return Result.Failure<FileContentResponse>(StoredFileErrors.NotFound);
        }

        Stream? content = await fileStore.OpenReadAsync(row.StoragePath, cancellationToken);
        if (content is null)
        {
            return Result.Failure<FileContentResponse>(StoredFileErrors.NotFound);
        }

        return new FileContentResponse(content, row.ContentType, row.OriginalName);
    }

    private sealed record ContentRow(string StoragePath, string ContentType, string OriginalName);
}
