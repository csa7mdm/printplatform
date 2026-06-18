using FluentAssertions;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Tests.Shared;

public sealed class ResultTests
{
    [Fact]
    public void Success_result_IsSuccess_is_true()
    {
        var result = Result.Success<string>("hello");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Failure_result_IsFailure_is_true()
    {
        var error  = Error.NotFound("Order.NotFound", "Order was not found.");
        var result = Result.Failure<string>(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Accessing_Value_on_failure_throws()
    {
        var result = Result.Failure<int>(Error.Failure("X", "bad"));
        var act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Implicit_conversion_from_value_yields_success()
    {
        Result<int> result = 42;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Implicit_conversion_from_error_yields_failure()
    {
        Result<int> result = Error.Conflict("X", "conflict");
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
