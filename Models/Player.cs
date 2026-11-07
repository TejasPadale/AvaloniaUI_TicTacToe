namespace TicTacToe.Models;

/// <summary>
/// Who owns a cell on the board (or whose turn it is).
/// </summary>
public enum Player
{
    // Keep None first: it has the value 0, which is the default value,
    // so a brand-new board array starts with every cell empty.
    None,
    X,
    O
}

public static class PlayerExtensions
{
    /// <summary>The other player: X gives O, O gives X. None stays None.</summary>
    public static Player Opponent(this Player player) => player switch
    {
        Player.X => Player.O,
        Player.O => Player.X,
        _ => Player.None
    };
}
