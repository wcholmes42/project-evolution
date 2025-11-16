using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution ===");
Console.WriteLine("Starting the game...");

var game = new RPGGame();
game.Start();

if (game.IsWon)
{
    Console.WriteLine("You won! Congratulations!");
}
else
{
    Console.WriteLine("Game Over!");
}
