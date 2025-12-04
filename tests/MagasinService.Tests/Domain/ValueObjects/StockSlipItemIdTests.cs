using FluentAssertions;
using MagasinService.Domain.Exceptions;
using MagasinService.Domain.ValueObjects;

namespace MagasinService.Tests.Domain.ValueObjects;

public class StockSlipItemIdTests
{
    [Fact]
    public void Of_ValidGuid_ShouldCreateStockSlipItemId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var stockSlipItemId = StockSlipItemId.Of(guid);

        // Assert
        stockSlipItemId.Should().NotBeNull();
        stockSlipItemId.Value.Should().Be(guid);
    }

    [Fact]
    public void Of_EmptyGuid_ShouldThrowDomainException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        Action act = () => StockSlipItemId.Of(emptyGuid);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*StockSlipItemId cannot be empty.*");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = StockSlipItemId.Of(guid);
        var id2 = StockSlipItemId.Of(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
        (id1 != id2).Should().BeFalse();
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = StockSlipItemId.Of(Guid.NewGuid());
        var id2 = StockSlipItemId.Of(Guid.NewGuid());

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
        id1.GetHashCode().Should().NotBe(id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldContainGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var stockSlipItemId = StockSlipItemId.Of(guid);

        // Act
        var result = stockSlipItemId.ToString();

        // Assert
        result.Should().Contain(guid.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldCreateStockSlipItemId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        StockSlipItemId stockSlipItemId = StockSlipItemId.Of(guid);

        // Assert
        stockSlipItemId.Should().NotBeNull();
        stockSlipItemId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var stockSlipItemId = StockSlipItemId.Of(guid);

        // Act
        Guid result = stockSlipItemId.Value;

        // Assert
        result.Should().Be(guid);
    }
}