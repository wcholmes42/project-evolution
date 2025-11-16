using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 1 ===");
Console.WriteLine("The coin is flipping...");
Console.WriteLine();

var game = new RPGGame();
game.StartWithRandomCoinFlip();

if (game.IsWon)
{
    Console.WriteLine("Heads! You won! Congratulations!");
}
else
{
    Console.WriteLine("Tails! You lost. Better luck next time!");
}

Console.WriteLine();
Console.WriteLine("Press any key to play again...");
Console.ReadKey();
