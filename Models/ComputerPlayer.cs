using System;
using System.Collections.Generic;

namespace TicTacToe.Models;

/// <summary>
/// Chooses moves for the computer.
///   Easy   - picks any empty cell at random.
///   Medium - wins if it can, otherwise blocks your win, otherwise random.
///   Hard   - looks ahead at every possible game (the "minimax" algorithm), so it never loses.
/// </summary>
public class ComputerPlayer
{
    private readonly Random _random;

    public ComputerPlayer() : this(Random.Shared)
    {
    }

    /// <summary>Tests pass a Random with a fixed seed, so the "random" moves are always the same.</summary>
    public ComputerPlayer(Random random)
    {
        _random = random;
    }

    /// <summary>
    /// Returns the cell (0-8) the computer wants to play as <paramref name="me"/>.
    /// The board is changed while "thinking", but always put back exactly as it was.
    /// </summary>
    public int ChooseMove(Board board, Player me, Difficulty difficulty)
    {
        if (board.IsFull())
            throw new InvalidOperationException("There are no empty cells left.");

        return difficulty switch
        {
            Difficulty.Easy => ChooseRandomMove(board),

            Difficulty.Medium => FindWinningMove(board, me)              // 1. win if possible
                                 ?? FindWinningMove(board, me.Opponent()) // 2. otherwise block
                                 ?? ChooseRandomMove(board),              // 3. otherwise random

            Difficulty.Hard => ChooseBestMove(board, me),

            _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
        };
    }

    private int ChooseRandomMove(Board board)
    {
        var empty = board.GetEmptyCells();
        return empty[_random.Next(empty.Count)];
    }

    /// <summary>A cell that gives <paramref name="player"/> three in a row right now, or null if there is none.</summary>
    private static int? FindWinningMove(Board board, Player player)
    {
        foreach (var cell in board.GetEmptyCells())
        {
            board.Place(cell, player); // try the move...
            var wins = board.GetWinningLine() is not null;
            board.Remove(cell);        // ...and take it back

            if (wins)
                return cell;
        }

        return null;
    }

    /// <summary>Tries every empty cell and keeps the ones with the best minimax score.</summary>
    private int ChooseBestMove(Board board, Player me)
    {
        var bestScore = int.MinValue;
        var bestMoves = new List<int>();

        foreach (var cell in board.GetEmptyCells())
        {
            board.Place(cell, me);
            var score = Minimax(board, me, me.Opponent(), depth: 1);
            board.Remove(cell);

            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
            }

            if (score == bestScore)
                bestMoves.Add(cell);
        }

        // Several moves can be equally good: pick one at random so every game isn't the same.
        return bestMoves[_random.Next(bestMoves.Count)];
    }

    /// <summary>
    /// Scores the board from the computer's point of view, assuming both sides play perfectly
    /// from here on: above 0 = the computer wins, below 0 = it loses, 0 = a draw.
    /// Using the depth makes it prefer winning sooner and losing later.
    /// </summary>
    private static int Minimax(Board board, Player me, Player turn, int depth)
    {
        var line = board.GetWinningLine();
        if (line is not null)
            return board[line[0]] == me ? 10 - depth : depth - 10;

        if (board.IsFull())
            return 0;

        // On the computer's turn it picks the highest score; on the opponent's turn
        // we assume the opponent picks the lowest (the best move for them).
        var best = turn == me ? int.MinValue : int.MaxValue;
        foreach (var cell in board.GetEmptyCells())
        {
            board.Place(cell, turn);
            var score = Minimax(board, me, turn.Opponent(), depth + 1);
            board.Remove(cell);

            best = turn == me ? Math.Max(best, score) : Math.Min(best, score);
        }

        return best;
    }
}
