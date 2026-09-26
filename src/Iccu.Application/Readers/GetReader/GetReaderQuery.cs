namespace Iccu.Application.Readers.GetReader;

using Dapper;
using Iccu.Domain.Common;
using Iccu.Domain.Readers;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Messaging;

public sealed record GetReaderQuery(Guid Id) : IQuery<ReaderResponse>;

internal sealed class GetReaderQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetReaderQuery, ReaderResponse>
{
    public async Task<Result<ReaderResponse>> Handle(GetReaderQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                r.id AS {nameof(ReaderResponse.Id)},
                r.photo_file_id AS {nameof(ReaderResponse.PhotoFileId)},
                lpad(r.card_number::text, 7, '0') AS {nameof(ReaderResponse.CardNumber)},
                r.category AS {nameof(ReaderResponse.Category)},
                r.last_name AS {nameof(ReaderResponse.LastName)},
                r.first_name AS {nameof(ReaderResponse.FirstName)},
                r.middle_name AS {nameof(ReaderResponse.MiddleName)},
                r.birth_date AS {nameof(ReaderResponse.BirthDate)},
                r.phone AS {nameof(ReaderResponse.Phone)},
                r.document_type AS {nameof(ReaderResponse.DocumentType)},
                r.document_number AS {nameof(ReaderResponse.DocumentNumber)},
                r.source AS {nameof(ReaderResponse.Source)},
                r.issued_on AS {nameof(ReaderResponse.IssuedOn)},
                r.expires_on AS {nameof(ReaderResponse.ExpiresOn)},
                r.expires_on < @Today AS {nameof(ReaderResponse.IsExpired)},
                r.print_count AS {nameof(ReaderResponse.PrintCount)},
                r.last_printed_at AS {nameof(ReaderResponse.LastPrintedAt)},
                r.created_at AS {nameof(ReaderResponse.CreatedAt)},
                creator.full_name AS {nameof(ReaderResponse.CreatedByName)},
                r.updated_at AS {nameof(ReaderResponse.UpdatedAt)}
            FROM iccu.readers r
            LEFT JOIN iccu.users creator ON creator.id = r.created_by
            WHERE r.id = @Id AND r.deleted_at IS NULL
            """;

        ReaderResponse? reader = await connection.QuerySingleOrDefaultAsync<ReaderResponse>(
            new CommandDefinition(
                sql,
                new { request.Id, dateTimeProvider.Today },
                cancellationToken: cancellationToken));

        return reader is null
            ? Result.Failure<ReaderResponse>(ReaderErrors.NotFound)
            : reader;
    }
}
