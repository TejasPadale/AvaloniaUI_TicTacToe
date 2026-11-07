using TicTacToe.Models;

namespace TicTacToe.Tests;

public class TicTacToeGameTests
{
    // Plays the given cells in order. The game itself decides whose turn it is.
    private static void PlayMoves(TicTacToeGame game, params int[] cells)
    {
        foreach (var cell in cells)
            game.Play(cell);
    }

    [Fact]
    public void NewGame_XStarts_AndTheRoundIsInProgress()
    {
        var game = new TicTacToeGame();

        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Player.None, game.Winner);
        Assert.Null(game.WinningLine);
    }

    [Fact]
    public void Play_PlacesTheCurrentPlayersMark_AndSwitchesTurn()
    {
        var game = new TicTacToeGame();

        game.Play(4);

        Assert.Equal(Player.X, game.GetCell(4));
        Assert.Equal(Player.O, game.CurrentPlayer);
    }

    [Fact]
    public void Play_OnATakenCell_IsNotAllowed()
    {
        var game = new TicTacToeGame();
        game.Play(4);

        Assert.False(game.CanPlay(4));
        Assert.Throws<InvalidOperationException>(() => game.Play(4));
    }

    [Fact]
    public void CompletingALine_WinsTheRound_AndCountsTheScore()
    {
        var game = new TicTacToeGame();

        PlayMoves(game, 0, 3, 1, 4, 2); // X: 0, 1, 2 (top row)   O: 3, 4

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, game.WinningLine);
        Assert.Equal(1, game.XWins);
        Assert.Equal(0, game.OWins);
        Assert.Equal(0, game.Draws);
    }

    [Fact]
    public void O_CanWin()
    {
        var game = new TicTacToeGame();

        PlayMoves(game, 0, 3, 1, 4, 8, 5); // X: 0, 1, 8   O: 3, 4, 5 (middle row)

        Assert.Equal(Player.O, game.Winner);
        Assert.Equal(new[] { 3, 4, 5 }, game.WinningLine);
        Assert.Equal(1, game.OWins);
    }

    [Fact]
    public void AfterAWin_TheBoardIsLocked()
    {
        var game = new TicTacToeGame();
        PlayMoves(game, 0, 3, 1, 4, 2); // X wins

        for (var i = 0; i < Board.CellCount; i++)
            Assert.False(game.CanPlay(i));

        Assert.Throws<InvalidOperationException>(() => game.Play(8));
    }

    [Fact]
    public void FullBoardWithNoLine_IsADraw()
    {
        var game = new TicTacToeGame();

        // Ends as:  X | O | X
        //           X | O | O
        //           O | X | X
        PlayMoves(game, 0, 4, 8, 1, 7, 6, 2, 5, 3);

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Equal(Player.None, game.Winner);
        Assert.Null(game.WinningLine);
        Assert.Equal(1, game.Draws);
    }

    [Fact]
    public void WinningWithTheLastEmptyCell_IsAWin_NotADraw()
    {
        var game = new TicTacToeGame();

        // Ends as:  X | O | X
        //           O | X | O
        //           O | X | X     (X's 9th move fills the board AND completes the diagonal)
        PlayMoves(game, 0, 1, 2, 3, 4, 5, 7, 6, 8);

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 0, 4, 8 }, game.WinningLine);
        Assert.Equal(0, game.Draws);
    }

    [Fact]
    public void StartNewRound_ClearsTheBoard_AndKeepsTheScore()
    {
        var game = new TicTacToeGame();
        PlayMoves(game, 0, 3, 1, 4, 2); // X wins

        game.StartNewRound();

        for (var i = 0; i < Board.CellCount; i++)
            Assert.True(game.CanPlay(i));

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Player.None, game.Winner);
        Assert.Null(game.WinningLine);
        Assert.Equal(1, game.XWins);
    }

    [Fact]
    public void StartingPlayer_TakesTurnsEachRound()
    {
        var game = new TicTacToeGame();
        Assert.Equal(Player.X, game.CurrentPlayer); // round 1

        game.StartNewRound();
        Assert.Equal(Player.O, game.CurrentPlayer); // round 2

        game.StartNewRound();
        Assert.Equal(Player.X, game.CurrentPlayer); // round 3
    }

    [Fact]
    public void StartNewMatch_ResetsEverything_AndXStarts()
    {
        var game = new TicTacToeGame();
        PlayMoves(game, 0, 3, 1, 4, 2); // X wins
        game.StartNewRound();           // O would start round 2

        game.StartNewMatch();

        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(0, game.XWins);
        for (var i = 0; i < Board.CellCount; i++)
            Assert.True(game.CanPlay(i));
    }

    [Fact]
    public void CopyBoard_ReturnsACopy_NotTheRealBoard()
    {
        var game = new TicTacToeGame();
        game.Play(0);

        var copy = game.CopyBoard();
        copy.Place(1, Player.O);

        Assert.Equal(Player.X, copy[0]);
        Assert.True(game.CanPlay(1)); // the real game didn't change
    }

    [Fact]
    public void ResetScores_SetsAllScoresToZero()
    {
        var game = new TicTacToeGame();
        PlayMoves(game, 0, 3, 1, 4, 2); // round 1: X starts, X wins
        game.StartNewRound();
        PlayMoves(game, 0, 3, 1, 4, 2); // round 2: O starts, so the same moves make O win
        Assert.Equal(1, game.XWins);
        Assert.Equal(1, game.OWins);

        game.ResetScores();

        Assert.Equal(0, game.XWins);
        Assert.Equal(0, game.OWins);
        Assert.Equal(0, game.Draws);
    }
}
