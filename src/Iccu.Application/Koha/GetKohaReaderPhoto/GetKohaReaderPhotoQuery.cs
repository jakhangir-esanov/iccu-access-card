namespace Iccu.Application.Koha.GetKohaReaderPhoto;

using Dapper;
using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Domain.StoredFiles;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Abstractions.Storage;
using Iccu.Application.StoredFiles.GetFileContent;

public sealed record GetKohaReaderPhotoQuery(int CardNumber) : IQuery<FileContentResponse>;

internal sealed class GetKohaReaderPhotoQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IFileStore fileStore) : IQueryHandler<GetKohaReaderPhotoQuery, FileContentResponse>
{
    public async Task<Result<FileContentResponse>> Handle(GetKohaReaderPhotoQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                f.storage_path AS {nameof(PhotoRow.StoragePath)},
                f.content_type AS {nameof(PhotoRow.ContentType)}
            FROM iccu.readers r
            JOIN iccu.stored_files f ON f.id = r.photo_file_id
            WHERE r.card_number = @CardNumber
              AND r.deleted_at IS NULL
            """;

        PhotoRow? row = await connection.QuerySingleOrDefaultAsync<PhotoRow>(
            new CommandDefinition(sql, new { request.CardNumber }, cancellationToken: cancellationToken));

        if (row is null)
        {
            return Result.Failure<FileContentResponse>(ReaderErrors.NotFound);
        }

        Stream? content = await fileStore.OpenReadAsync(row.StoragePath, cancellationToken);
        if (content is null)
        {
            return Result.Failure<FileContentResponse>(StoredFileErrors.NotFound);
        }

        return new FileContentResponse(content, row.ContentType);
    }

    private sealed record PhotoRow(string StoragePath, string ContentType);
}
