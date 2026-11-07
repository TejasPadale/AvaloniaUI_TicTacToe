using TicTacToe.Models;

namespace TicTacToe.Tests;

public class ComputerPlayerTests
{
    // Builds a board from a little picture, row by row: X, O, or . for empty. Spaces are ignored.
    // Example: "XX. .O. ..." means X in cells 0 and 1, O in cell 4.
    private static Board BoardFrom(string picture)
    {
        var cells = picture.Replace(" ", "");
        var board = new Board();
        for (var i = 0; i < Board.CellCount; i++)
        {
            if (cells[i] == 'X')
                board.Place(i, Player.X);
            else if (cells[i] == 'O')
                board.Place(i, Player.O);
        }

        return board;
    }

    [Fact]
    public void Easy_AlwaysPicksAnEmptyCell()
    {
        var computer = new ComputerPlayer(new Random(1));
        var board = BoardFrom("XOX OX. ...");

        for (var i = 0; i < 100; i++)
            Assert.True(board.IsEmpty(computer.ChooseMove(board, Player.O, Difficulty.Easy)));
    }

    [Theory]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void TakesTheWin_WhenItCan(Difficulty level)
    {
        // O O .
        // X X .     O can win at 2. (X threatens 5 too, but winning comes first.)
        // X . .
        var board = BoardFrom("OO. XX. X..");

        var move = new ComputerPlayer(new Random(1)).ChooseMove(board, Player.O, level);

        Assert.Equal(2, move);
    }

    [Theory]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void BlocksYourWin(Difficulty level)
    {
        // X X .
        // . O .     X is about to win at 2, so O must block there.
        // . . .
        var board = BoardFrom("XX. .O. ...");

        var move = new ComputerPlayer(new Random(1)).ChooseMove(board, Player.O, level);

        Assert.Equal(2, move);
    }

    [Fact]
    public void Hard_NeverLoses_WhateverYouDo()
    {
        var computer = new ComputerPlayer(new Random(1));

        PlayEveryGame(new Board(), computer, turn: Player.X); // you (X) start
        PlayEveryGame(new Board(), computer, turn: Player.O); // the computer (O) starts
    }

    // Plays out every possible game: at each of your (X) turns, every empty cell is tried.
    private static void PlayEveryGame(Board board, ComputerPlayer computer, Player turn)
    {
        var line = board.GetWinningLine();
        if (line is not null)
        {
            Assert.Equal(Player.O, board[line[0]]); // if anyone won, it must be the computer
            return;
        }

        if (board.IsFull())
            return; // a draw is fine

        if (turn == Player.O)
        {
            var move = computer.ChooseMove(board, Player.O, Difficulty.Hard);
            board.Place(move, Player.O);
            PlayEveryGame(board, computer, Player.X);
            board.Remove(move);
        }
        else
        {
            foreach (var cell in board.GetEmptyCells())
            {
                board.Place(cell, Player.X);
                PlayEveryGame(board, computer, Player.O);
                board.Remove(cell);
            }
        }
    }

    [Fact]
    public void ChooseMove_LeavesTheBoardAsItWas()
    {
        var board = BoardFrom("X.. .O. ..X");
        var before = Enumerable.Range(0, Board.CellCount).Select(i => board[i]).ToArray();

        new ComputerPlayer(new Random(1)).ChooseMove(board, Player.O, Difficulty.Hard);

        var after = Enumerable.Range(0, Board.CellCount).Select(i => board[i]).ToArray();
        Assert.Equal(before, after);
    }

    [Fact]
    public void ChooseMove_OnAFullBoard_Throws()
    {
        var board = BoardFrom("XOX XOO OXX");

        Assert.Throws<InvalidOperationException>(
            () => new ComputerPlayer().ChooseMove(board, Player.O, Difficulty.Easy));
    }
}
