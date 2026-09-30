namespace Iccu.Application.Readers.ExportReaders;

using Dapper;
using Iccu.Domain.Common;
using System.Globalization;
using Iccu.Domain.Common.Enums;
using Iccu.Application.Common.Data;
using Iccu.Application.Common.Clock;
using Iccu.Application.Common.Export;
using Iccu.Application.Common.Messaging;
using Iccu.Application.Readers.GetReaders;

public sealed record ExportReadersQuery(
    string? Search = null,
    ReaderCategory? Category = null,
    RegistrationSource? Source = null,
    CardStatus? Status = null,
    Gender? Gender = null,
    Citizenship? Citizenship = null,
    DateOnly? RegisteredFrom = null,
    DateOnly? RegisteredTo = null) : IQuery<FileResponse>;

internal sealed class ExportReadersQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<ExportReadersQuery, FileResponse>
{
    private const string FileNameFormat = "kitobxonlar-{0:yyyyMMdd}.xlsx";

    public async Task<Result<FileResponse>> Handle(ExportReadersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        (string whereSql, DynamicParameters param) = GetReadersQueryHandler.BuildFilter(
            request.Search,
            request.Category,
            request.Source,
            request.Status,
            request.Gender,
            request.Citizenship,
            request.RegisteredFrom,
            request.RegisteredTo,
            dateTimeProvider);

        var sql =
            $"""
            {GetReadersQueryHandler.ReadersCte}
            SELECT
                readers.card_number AS {nameof(ReaderExportRow.CardNumber)},
                readers.last_name AS {nameof(ReaderExportRow.LastName)},
                readers.first_name AS {nameof(ReaderExportRow.FirstName)},
                readers.middle_name AS {nameof(ReaderExportRow.MiddleName)},
                readers.category AS {nameof(ReaderExportRow.Category)},
                readers.birth_date AS {nameof(ReaderExportRow.BirthDate)},
                readers.gender AS {nameof(ReaderExportRow.Gender)},
                readers.citizenship AS {nameof(ReaderExportRow.Citizenship)},
                readers.phone AS {nameof(ReaderExportRow.Phone)},
                readers.issued_on AS {nameof(ReaderExportRow.IssuedOn)},
                readers.expires_on AS {nameof(ReaderExportRow.ExpiresOn)},
                readers.source AS {nameof(ReaderExportRow.Source)},
                readers.print_count AS {nameof(ReaderExportRow.PrintCount)},
                creator.full_name AS {nameof(ReaderExportRow.CreatedByName)}
            FROM readers
            LEFT JOIN iccu.users creator ON creator.id = readers.created_by
            {whereSql}
            ORDER BY readers.card_sequence
            """;

        var rows = await connection.QueryAsync<ReaderExportRow>(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken));

        DateOnly today = dateTimeProvider.Today;

        byte[] content = ExcelWriter.ToWorkbook(
            ReaderExportSheet.SheetName,
            ReaderExportSheet.Header,
            rows.Select(row => ReaderExportSheet.ToCells(row, today)));

        string fileName = string.Format(CultureInfo.InvariantCulture, FileNameFormat, today.ToDateTime(TimeOnly.MinValue));

        return new FileResponse(content, ExcelWriter.ContentType, fileName);
    }
}
