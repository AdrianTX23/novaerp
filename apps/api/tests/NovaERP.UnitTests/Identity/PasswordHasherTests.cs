using FluentAssertions;
using NovaERP.Infrastructure.Identity;

namespace NovaERP.UnitTests.Identity;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_then_Verify_succeeds_for_correct_password()
    {
        var hash = _sut.Hash("Test1234");

        _sut.Verify("Test1234", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_fails_for_wrong_password()
    {
        var hash = _sut.Hash("Test1234");

        _sut.Verify("Wrong9999", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_produces_different_output_for_same_password_due_to_salt()
    {
        _sut.Hash("Test1234").Should().NotBe(_sut.Hash("Test1234"));
    }
}
