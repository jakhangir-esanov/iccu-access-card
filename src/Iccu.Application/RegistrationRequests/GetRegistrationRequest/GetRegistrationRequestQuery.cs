namespace Iccu.Application.RegistrationRequests.GetRegistrationRequest;

using Dapper;
using Iccu.Domain.Common;
using Iccu.Application.Common.Data;
using Iccu.Domain.RegistrationRequests;
using Iccu.Application.Common.Messaging;

public sealed record GetRegistrationRequestQuery(Guid Id) : IQuery<RegistrationRequestResponse>;

internal sealed class GetRegistrationRequestQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetRegistrationRequestQuery, RegistrationRequestResponse>
{
    public async Task<Result<RegistrationRequestResponse>> Handle(
        GetRegistrationRequestQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        var sql =
            $"""
            SELECT
                q.id AS {nameof(RegistrationRequestResponse.Id)},
                q.photo_file_id AS {nameof(RegistrationRequestResponse.PhotoFileId)},
                lpad(q.code::text, 4, '0') AS {nameof(RegistrationRequestResponse.Code)},
                q.status AS {nameof(RegistrationRequestResponse.Status)},
                q.category AS {nameof(RegistrationRequestResponse.Category)},
                q.last_name AS {nameof(RegistrationRequestResponse.LastName)},
                q.first_name AS {nameof(RegistrationRequestResponse.FirstName)},
                q.middle_name AS {nameof(RegistrationRequestResponse.MiddleName)},
                q.birth_date AS {nameof(RegistrationRequestResponse.BirthDate)},
                q.phone AS {nameof(RegistrationRequestResponse.Phone)},
                q.document_type AS {nameof(RegistrationRequestResponse.DocumentType)},
                q.document_number AS {nameof(RegistrationRequestResponse.DocumentNumber)},
                q.submitted_at AS {nameof(RegistrationRequestResponse.SubmittedAt)},
                q.expires_at AS {nameof(RegistrationRequestResponse.ExpiresAt)},
                q.reviewed_at AS {nameof(RegistrationRequestResponse.ReviewedAt)},
                reviewer.full_name AS {nameof(RegistrationRequestResponse.ReviewedByName)},
                q.rejection_reason AS {nameof(RegistrationRequestResponse.RejectionReason)},
                q.reader_id AS {nameof(RegistrationRequestResponse.ReaderId)},
                existing.id AS {nameof(RegistrationRequestResponse.RegisteredReaderId)},
                lpad(existing.card_number::text, 7, '0') AS {nameof(RegistrationRequestResponse.RegisteredReaderCardNumber)}
            FROM iccu.registration_requests q
            LEFT JOIN iccu.users reviewer ON reviewer.id = q.reviewed_by
            LEFT JOIN iccu.readers existing
                ON existing.document_type = q.document_type
               AND existing.document_number = q.document_number
               AND existing.deleted_at IS NULL
            WHERE q.id = @Id
            """;

        RegistrationRequestResponse? registrationRequest = await connection.QuerySingleOrDefaultAsync<RegistrationRequestResponse>(
            new CommandDefinition(sql, new { request.Id }, cancellationToken: cancellationToken));

        return registrationRequest is null
            ? Result.Failure<RegistrationRequestResponse>(RegistrationRequestErrors.NotFound)
            : registrationRequest;
    }
}
