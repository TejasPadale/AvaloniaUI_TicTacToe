using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TicTacToe.Models;

namespace TicTacToe.ViewModels;

/// <summary>
/// Connects the game rules (<see cref="TicTacToeGame"/>) to the window.
/// The game is the "single source of truth": after every action we copy its
/// state into the properties below, and the window updates itself through bindings.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    // Against the computer, you are always X and the computer is always O.
    private const Player HumanMark = Player.X;
    private const Player ComputerMark = Player.O;

    private readonly TicTacToeGame _game = new();
    private readonly ComputerPlayer _computer;
    private readonly TimeSpan _computerThinkingTime;

    // Goes up every time the board is reset. If the computer was still "thinking"
    // when that happened, its move belongs to the old board and is thrown away.
    private int _boardVersion;

    public MainViewModel() : this(new ComputerPlayer(), TimeSpan.FromMilliseconds(600))
    {
    }

    /// <summary>Tests use this to get repeatable computer moves without waiting.</summary>
    public MainViewModel(ComputerPlayer computer, TimeSpan computerThinkingTime)
    {
        _computer = computer;
        _computerThinkingTime = computerThinkingTime;

        var cells = new List<CellViewModel>();
        for (var i = 0; i < Board.CellCount; i++)
            cells.Add(new CellViewModel(i, PlayCellAsync));
        Cells = cells;

        Refresh();
    }

    /// <summary>The 9 squares, in order 0-8. The window shows them in a 3x3 grid.</summary>
    public IReadOnlyList<CellViewModel> Cells { get; }

    // ----- Settings (the two drop-downs) -----

    public IReadOnlyList<Opponent> Opponents { get; } = Enum.GetValues<Opponent>();
    public IReadOnlyList<Difficulty> Levels { get; } = Enum.GetValues<Difficulty>();

    /// <summary>"Play against" drop-down. Changing it starts a fresh match (see OnPlayAgainstChanged).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVsComputer))]
    [NotifyPropertyChangedFor(nameof(XName))]
    [NotifyPropertyChangedFor(nameof(OName))]
    public partial Opponent PlayAgainst { get; set; } = Opponent.Computer;

    /// <summary>"Level" drop-down. A change is used from the computer's next move.</summary>
    [ObservableProperty]
    public partial Difficulty Level { get; set; } = Difficulty.Medium;

    public bool IsVsComputer => PlayAgainst == Opponent.Computer;

    // ----- Scoreboard -----

    public string XName => IsVsComputer ? "You (X)" : "Player X";
    public string OName => IsVsComputer ? "Computer (O)" : "Player O";

    [ObservableProperty]
    public partial int XWins { get; set; }

    [ObservableProperty]
    public partial int OWins { get; set; }

    [ObservableProperty]
    public partial int Draws { get; set; }

    // ----- Whose turn -----

    /// <summary>The player to highlight: whose turn it is, or the winner once the round is over.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsXActive))]
    [NotifyPropertyChangedFor(nameof(IsOActive))]
    public partial Player ActivePlayer { get; set; }

    public bool IsXActive => ActivePlayer == Player.X;
    public bool IsOActive => ActivePlayer == Player.O;

    /// <summary>"Your turn", "Computer is thinking…", "You win!", "Player O's turn", ...</summary>
    [ObservableProperty]
    public partial string StatusText { get; set; } = "";

    private bool IsComputerTurn =>
        IsVsComputer && _game.Status == GameStatus.InProgress && _game.CurrentPlayer == ComputerMark;

    // ----- Actions -----

    // Runs automatically when PlayAgainst changes (the toolkit generates the call).
    // The old score means something else now, so start a fresh match: X starts, scores at zero.
    partial void OnPlayAgainstChanged(Opponent value)
    {
        _boardVersion++;
        _game.StartNewMatch();
        Refresh();
    }

    /// <summary>"New Game" button: clear the board; the other player starts this time.</summary>
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task NewGame()
    {
        _boardVersion++;
        _game.StartNewRound();
        Refresh();

        await LetComputerPlayAsync(); // the computer may be the one who starts this round
    }

    /// <summary>"Reset Score" button. Only enabled when there is a score to reset.</summary>
    [RelayCommand(CanExecute = nameof(HasAnyScore))]
    private void ResetScores()
    {
        _game.ResetScores();
        Refresh();
    }

    private bool HasAnyScore() => XWins + OWins + Draws > 0;

    /// <summary>Called by a CellViewModel when you click its square.</summary>
    private async Task PlayCellAsync(int index)
    {
        // The button should already be disabled, but never trust the UI alone.
        if (IsComputerTurn || !_game.CanPlay(index))
            return;

        _game.Play(index);
        Refresh();

        await LetComputerPlayAsync();
    }

    /// <summary>If it's the computer's turn: wait a moment (so you see it "think"), then play.</summary>
    private async Task LetComputerPlayAsync()
    {
        if (!IsComputerTurn)
            return;

        var version = _boardVersion;
        await Task.Delay(_computerThinkingTime);

        // While we waited, "New Game" may have been pressed or the opponent changed.
        if (version != _boardVersion || !IsComputerTurn)
            return;

        var move = _computer.ChooseMove(_game.CopyBoard(), ComputerMark, Level);
        _game.Play(move);
        Refresh();
    }

    /// <summary>Copies the game's state into the properties the window shows.</summary>
    private void Refresh()
    {
        foreach (var cell in Cells)
        {
            cell.Owner = _game.GetCell(cell.Index);
            cell.IsPlayable = _game.CanPlay(cell.Index) && !IsComputerTurn; // locked while the computer thinks
            cell.IsWinning = _game.WinningLine?.Contains(cell.Index) ?? false;
        }

        ActivePlayer = _game.Status switch
        {
            GameStatus.InProgress => _game.CurrentPlayer,
            GameStatus.Won => _game.Winner,
            _ => Player.None // draw: nobody is highlighted
        };

        StatusText = GetStatusText();

        XWins = _game.XWins;
        OWins = _game.OWins;
        Draws = _game.Draws;

        // Tell the "Reset Score" button to re-check whether it should be enabled.
        ResetScoresCommand.NotifyCanExecuteChanged();
    }

    private string GetStatusText()
    {
        if (_game.Status == GameStatus.Draw)
            return "It's a draw!";

        if (!IsVsComputer)
        {
            return _game.Status == GameStatus.Won
                ? $"Player {_game.Winner} wins!"
                : $"Player {_game.CurrentPlayer}'s turn";
        }

        if (_game.Status == GameStatus.Won)
            return _game.Winner == HumanMark ? "You win!" : "Computer wins!";

        return _game.CurrentPlayer == HumanMark ? "Your turn" : "Computer is thinking…";
    }
}
