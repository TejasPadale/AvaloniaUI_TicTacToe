using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TicTacToe.Models;

namespace TicTacToe.ViewModels;

/// <summary>
/// One of the 9 squares on screen. The window binds a Button to it.
/// </summary>
public partial class CellViewModel : ViewModelBase
{
    // Given to us by MainViewModel: "call this when the cell is clicked".
    // It returns a Task because, against the computer, a click also waits for the computer's answer.
    private readonly Func<int, Task> _onPlay;

    public CellViewModel(int index, Func<int, Task> onPlay)
    {
        Index = index;
        _onPlay = onPlay;
    }

    /// <summary>Cell number 0-8 (0 = top left, 8 = bottom right).</summary>
    public int Index { get; }

    /// <summary>Who has played here. When it changes, the properties below change too.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Symbol))]
    [NotifyPropertyChangedFor(nameof(IsX))]
    [NotifyPropertyChangedFor(nameof(IsO))]
    public partial Player Owner { get; set; }

    /// <summary>The text shown on the button: "X", "O" or nothing.</summary>
    public string Symbol => Owner == Player.None ? "" : Owner.ToString();

    // Used by the window to pick the colour (blue for X, orange for O).
    public bool IsX => Owner == Player.X;
    public bool IsO => Owner == Player.O;

    /// <summary>True for the three squares of the winning line.</summary>
    [ObservableProperty]
    public partial bool IsWinning { get; set; }

    /// <summary>Can this square be clicked right now? When it changes, the button re-checks if it is enabled.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    public partial bool IsPlayable { get; set; }

    // [RelayCommand] generates a "PlayCommand" property that the Button uses.
    // Because this method returns a Task, the generated command is an async command.
    // The button is disabled whenever IsPlayable is false.
    [RelayCommand(CanExecute = nameof(IsPlayable))]
    private Task Play() => _onPlay(Index);
}
