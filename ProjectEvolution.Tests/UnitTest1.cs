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

    [Fact]
    public void CoinFlipGame_CanWin()
    {
        // Arrange
        var game = new RPGGame();

        // Act
        game.StartWithCoinFlip(heads: true);

        // Assert
        Assert.True(game.IsWon);
    }

    [Fact]
    public void CoinFlipGame_CanLose()
    {
        // Arrange
        var game = new RPGGame();

        // Act
        game.StartWithCoinFlip(heads: false);

        // Assert
        Assert.False(game.IsWon);
    }

    [Fact]
    public void CoinFlipGame_RandomOutcome_ProducesBothResults()
    {
        // Arrange & Act
        var results = new List<bool>();
        for (int i = 0; i < 100; i++)
        {
            var game = new RPGGame();
            game.StartWithRandomCoinFlip();
            results.Add(game.IsWon);
        }

        // Assert - over 100 iterations, we should get both true and false
        Assert.Contains(true, results);
        Assert.Contains(false, results);
    }
}
