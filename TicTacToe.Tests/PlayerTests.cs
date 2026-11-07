using TicTacToe.Models;

namespace TicTacToe.Tests;

public class PlayerTests
{
    [Theory]
    [InlineData(Player.X, Player.O)]
    [InlineData(Player.O, Player.X)]
    [InlineData(Player.None, Player.None)]
    public void Opponent_ReturnsTheOtherPlayer(Player player, Player expected)
    {
        Assert.Equal(expected, player.Opponent());
    }
}
