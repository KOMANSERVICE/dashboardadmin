namespace MagasinService.Tests.Domain.ValueObjects;

public class StockSlipIdTests
{
    [Fact]
    public void Of_ValidGuid_ShouldCreateStockSlipId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var stockSlipId = StockSlipId.Of(guid);

        // Assert
        stockSlipId.Should().NotBeNull();
        stockSlipId.Value.Should().Be(guid);
    }

    [Fact]
    public void Of_EmptyGuid_ShouldThrowDomainException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        Action act = () => StockSlipId.Of(emptyGuid);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("StockSlipId cannot be empty.");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = StockSlipId.Of(guid);
        var id2 = StockSlipId.Of(guid);

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
        var id1 = StockSlipId.Of(Guid.NewGuid());
        var id2 = StockSlipId.Of(Guid.NewGuid());

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
        id1.GetHashCode().Should().NotBe(id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var stockSlipId = StockSlipId.Of(guid);

        // Act
        var result = stockSlipId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }
}