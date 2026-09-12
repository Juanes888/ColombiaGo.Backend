using ColombiaGo.Domain.Common;
using FluentAssertions;
using Xunit;

namespace ColombiaGo.UnitTests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result<string>.Success("comuna-13");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("comuna-13");
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ExposesTheError()
    {
        var error = Error.NotFound("Lugar.NotFound", "No existe.");
        var result = Result<string>.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<string>.Failure(Error.Validation("X", "invalid"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromValue_ProducesSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
