namespace Iccu.Application.StoredFiles.UploadFile;

using FluentValidation;

internal sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const int FileNameMaxLength = 300;

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(FileNameMaxLength);
    }
}
