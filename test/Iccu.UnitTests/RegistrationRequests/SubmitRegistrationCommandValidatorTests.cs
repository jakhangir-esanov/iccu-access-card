namespace Iccu.UnitTests.RegistrationRequests;

using Iccu.Domain.RegistrationRequests;
using Iccu.UnitTests.Fakes;
using Iccu.Application.RegistrationRequests.SubmitRegistration;

public class SubmitRegistrationCommandValidatorTests
{
    private readonly SubmitRegistrationCommandValidator _validator = new(new FakeClock());

    [Fact]
    public void Validate_WithConsent_Passes()
    {
        var command = new SubmitRegistrationCommand(TestData.Pupil(), Guid.NewGuid(), ConsentGiven: true);

        Assert.True(_validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validate_WithoutConsent_ReportsConsentRequired()
    {
        var command = new SubmitRegistrationCommand(TestData.Pupil(), Guid.NewGuid(), ConsentGiven: false);

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.ErrorCode == RegistrationRequestErrors.ConsentRequired.Code);
    }
}
