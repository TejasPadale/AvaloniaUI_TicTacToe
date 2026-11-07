using System;
using System.Collections.Generic;

namespace TicTacToe.Models;

/// <summary>
/// The rules of Tic Tac Toe: whose turn it is, whether the round is won or drawn,
/// who starts the next round, and the score. It doesn't care whether a move came
/// from a person or the computer. Like <see cref="Board"/>, it knows nothing about the UI.
/// </summary>
public class TicTacToeGame
{
    private readonly Board _board = new();

    /// <summary>Whose turn it is right now.</summary>
    public Player CurrentPlayer { get; private set; } = Player.X;

    /// <summary>Who started this round. It swaps every new round (X, O, X, ...).</summary>
    public Player StartingPlayer { get; private set; } = Player.X;

    public GameStatus Status { get; private set; } = GameStatus.InProgress;

    /// <summary>The winner of this round, or None if nobody has won.</summary>
    public Player Winner { get; private set; } = Player.None;

    /// <summary>The three winning cells, or null if nobody has won.</summary>
    public IReadOnlyList<int>? WinningLine { get; private set; }

    // The score. It lives only as long as this object (so only while the app is open).
    public int XWins { get; private set; }
    public int OWins { get; private set; }
    public int Draws { get; private set; }

    /// <summary>Who owns a cell (0-8).</summary>
    public Player GetCell(int index) => _board[index];

    /// <summary>A copy of the board, so the computer can try out moves without touching the real game.</summary>
    public Board CopyBoard() => _board.Clone();

    /// <summary>True if the current player is allowed to play in this cell.</summary>
    public bool CanPlay(int index) => Status == GameStatus.InProgress && _board.IsEmpty(index);

    /// <summary>The current player puts their mark in a cell.</summary>
    public void Play(int index)
    {
        if (!CanPlay(index))
            throw new InvalidOperationException($"Cell {index} can't be played right now.");

        _board.Place(index, CurrentPlayer);

        // Check for a win BEFORE checking for a full board:
        // a move can fill the last cell and complete a line at the same time.
        var line = _board.GetWinningLine();
        if (line is not null)
        {
            Status = GameStatus.Won;
            Winner = CurrentPlayer;
            WinningLine = line;

            if (Winner == Player.X)
                XWins++;
            else
                OWins++;
        }
        else if (_board.IsFull())
        {
            Status = GameStatus.Draw;
            Draws++;
        }
        else
        {
            CurrentPlayer = CurrentPlayer.Opponent();
        }
    }

    /// <summary>Clears the board for a new round. The other player starts this time.</summary>
    public void StartNewRound()
    {
        StartingPlayer = StartingPlayer.Opponent();
        ResetBoard();
    }

    /// <summary>Starts over completely: empty board, all scores zero, X starts.</summary>
    public void StartNewMatch()
    {
        StartingPlayer = Player.X;
        ResetScores();
        ResetBoard();
    }

    private void ResetBoard()
    {
        _board.Clear();
        CurrentPlayer = StartingPlayer;
        Status = GameStatus.InProgress;
        Winner = Player.None;
        WinningLine = null;
    }

    /// <summary>Sets all scores back to zero. The board is not touched.</summary>
    public void ResetScores()
    {
        XWins = 0;
        OWins = 0;
        Draws = 0;
    }
}
