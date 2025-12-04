namespace MagasinService.Tests.Domain.ValueObjects;

public class StockMovementIdTests
{
    [Fact]
    public void Of_ValidGuid_ShouldCreateStockMovementId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var stockMovementId = StockMovementId.Of(guid);

        // Assert
        stockMovementId.Should().NotBeNull();
        stockMovementId.Value.Should().Be(guid);
    }

    [Fact]
    public void Of_EmptyGuid_ShouldThrowDomainException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        Action act = () => StockMovementId.Of(emptyGuid);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("StockMovementId cannot be empty.");
    }

    [Fact]
    public void Of_NullGuid_ShouldThrowArgumentNullException()
    {
        // Arrange
        Guid? nullGuid = null;

        // Act
        Action act = () => StockMovementId.Of(nullGuid!.Value);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = StockMovementId.Of(guid);
        var id2 = StockMovementId.Of(guid);

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
        var id1 = StockMovementId.Of(Guid.NewGuid());
        var id2 = StockMovementId.Of(Guid.NewGuid());

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
        id1.GetHashCode().Should().NotBe(id2.GetHashCode());
    }
}