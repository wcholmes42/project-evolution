namespace ProjectEvolution.Tests;

using ProjectEvolution.Game;

public class GameTests
{
    [Fact]
    public void StartGame_ImmediatelyWins()
    {
        // Arrange
        var game = new RPGGame();

        // Act
        game.Start();

        // Assert
        Assert.True(game.IsWon);
    }
}
