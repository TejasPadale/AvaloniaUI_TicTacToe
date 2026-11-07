using TicTacToe.Models;

namespace TicTacToe.Tests;

public class BoardTests
{
    [Fact]
    public void NewBoard_IsEmpty_AndHasNoWinner()
    {
        var board = new Board();

        for (var i = 0; i < Board.CellCount; i++)
            Assert.True(board.IsEmpty(i));

        Assert.False(board.IsFull());
        Assert.Null(board.GetWinningLine()); // three empty cells in a row are NOT a win
    }

    [Fact]
    public void Place_PutsTheMarkInTheCell()
    {
        var board = new Board();

        board.Place(4, Player.X);

        Assert.Equal(Player.X, board[4]);
        Assert.False(board.IsEmpty(4));
    }

    [Fact]
    public void Place_OnATakenCell_Throws()
    {
        var board = new Board();
        board.Place(4, Player.X);

        Assert.Throws<InvalidOperationException>(() => board.Place(4, Player.O));
    }

    [Fact]
    public void Place_None_Throws()
    {
        var board = new Board();

        Assert.Throws<ArgumentException>(() => board.Place(0, Player.None));
    }

    [Theory]
    [InlineData(0, 1, 2)] // top row
    [InlineData(3, 4, 5)] // middle row
    [InlineData(6, 7, 8)] // bottom row
    [InlineData(0, 3, 6)] // left column
    [InlineData(1, 4, 7)] // middle column
    [InlineData(2, 5, 8)] // right column
    [InlineData(0, 4, 8)] // diagonal from top left
    [InlineData(2, 4, 6)] // diagonal from top right
    public void ThreeInARow_IsAWin(int a, int b, int c)
    {
        var board = new Board();
        board.Place(a, Player.X);
        board.Place(b, Player.X);
        board.Place(c, Player.X);

        Assert.Equal(new[] { a, b, c }, board.GetWinningLine());
    }

    [Fact]
    public void O_CanWinToo()
    {
        var board = new Board();
        board.Place(2, Player.O);
        board.Place(4, Player.O);
        board.Place(6, Player.O);

        Assert.Equal(new[] { 2, 4, 6 }, board.GetWinningLine());
    }

    [Fact]
    public void TwoInARow_IsNotAWin()
    {
        var board = new Board();
        board.Place(0, Player.X);
        board.Place(1, Player.X);

        Assert.Null(board.GetWinningLine());
    }

    [Fact]
    public void MixedLine_IsNotAWin()
    {
        var board = new Board();
        board.Place(0, Player.X);
        board.Place(1, Player.X);
        board.Place(2, Player.O);

        Assert.Null(board.GetWinningLine());
    }

    [Fact]
    public void FullBoardWithNoLine_IsFull_AndHasNoWinner()
    {
        // X | O | X
        // X | O | O
        // O | X | X
        Player[] layout =
        [
            Player.X, Player.O, Player.X,
            Player.X, Player.O, Player.O,
            Player.O, Player.X, Player.X
        ];
        var board = new Board();
        for (var i = 0; i < layout.Length; i++)
            board.Place(i, layout[i]);

        Assert.True(board.IsFull());
        Assert.Null(board.GetWinningLine());
    }

    [Fact]
    public void Remove_EmptiesTheCellAgain()
    {
        var board = new Board();
        board.Place(4, Player.X);

        board.Remove(4);

        Assert.True(board.IsEmpty(4));
    }

    [Fact]
    public void GetEmptyCells_ListsOnlyTheEmptyCells()
    {
        var board = new Board();
        board.Place(0, Player.X);
        board.Place(4, Player.O);

        Assert.Equal(new[] { 1, 2, 3, 5, 6, 7, 8 }, board.GetEmptyCells());
    }

    [Fact]
    public void Clone_IsASeparateCopy()
    {
        var board = new Board();
        board.Place(0, Player.X);

        var copy = board.Clone();
        copy.Place(1, Player.O);

        Assert.Equal(Player.X, copy[0]); // the copy starts with the original's marks...
        Assert.True(board.IsEmpty(1));   // ...but changing the copy doesn't change the original
    }

    [Fact]
    public void Clear_EmptiesEveryCell()
    {
        var board = new Board();
        board.Place(0, Player.X);
        board.Place(4, Player.O);

        board.Clear();

        for (var i = 0; i < Board.CellCount; i++)
            Assert.True(board.IsEmpty(i));
    }
}
