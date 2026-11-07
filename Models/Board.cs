using System;
using System.Collections.Generic;

namespace TicTacToe.Models;

/// <summary>
/// The 3x3 game board. It only stores which player owns each cell and
/// answers questions about it (is it full? has someone won?).
/// It knows nothing about turns, scores or the UI.
///
/// Cells are numbered 0-8, left to right, top to bottom:
///   0 | 1 | 2
///   3 | 4 | 5
///   6 | 7 | 8
/// </summary>
public class Board
{
    public const int CellCount = 9;

    // Every way to get three in a row: 3 rows, 3 columns, 2 diagonals.
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8], // rows
        [0, 3, 6], [1, 4, 7], [2, 5, 8], // columns
        [0, 4, 8], [2, 4, 6]             // diagonals
    ];

    // One entry per cell. A new array is filled with Player.None (the default).
    private readonly Player[] _cells = new Player[CellCount];

    /// <summary>Read who owns a cell, e.g. board[4] is the center cell.</summary>
    public Player this[int index] => _cells[index];

    public bool IsEmpty(int index) => _cells[index] == Player.None;

    /// <summary>Puts X or O into an empty cell.</summary>
    public void Place(int index, Player player)
    {
        if (player == Player.None)
            throw new ArgumentException("Only X or O can be placed.", nameof(player));

        if (!IsEmpty(index))
            throw new InvalidOperationException($"Cell {index} is already taken.");

        _cells[index] = player;
    }

    /// <summary>True when there are no empty cells left.</summary>
    public bool IsFull()
    {
        foreach (var cell in _cells)
        {
            if (cell == Player.None)
                return false;
        }

        return true;
    }

    /// <summary>Empties every cell, ready for a new round.</summary>
    public void Clear() => Array.Clear(_cells);

    /// <summary>Empties one cell again. The computer uses this to undo moves it is only trying out.</summary>
    public void Remove(int index) => _cells[index] = Player.None;

    /// <summary>The numbers of all empty cells.</summary>
    public List<int> GetEmptyCells()
    {
        var empty = new List<int>();
        for (var i = 0; i < CellCount; i++)
        {
            if (IsEmpty(i))
                empty.Add(i);
        }

        return empty;
    }

    /// <summary>A copy of this board. Changing the copy does not change the original.</summary>
    public Board Clone()
    {
        var copy = new Board();
        Array.Copy(_cells, copy._cells, CellCount);
        return copy;
    }

    /// <summary>
    /// Returns the three cell numbers of a completed line (for example [0, 4, 8]),
    /// or null if nobody has three in a row yet.
    /// </summary>
    public IReadOnlyList<int>? GetWinningLine()
    {
        foreach (var line in WinningLines)
        {
            var first = _cells[line[0]];

            // Three empty cells are "equal" too, so check that the first one isn't empty.
            if (first != Player.None &&
                first == _cells[line[1]] &&
                first == _cells[line[2]])
            {
                return line;
            }
        }

        return null;
    }
}
