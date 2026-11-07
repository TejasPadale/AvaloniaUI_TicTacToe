using TicTacToe.Models;
using TicTacToe.ViewModels;

namespace TicTacToe.Tests;

/// <summary>
/// Tests the screen logic without opening a window:
/// "clicking" a cell is the same as running its PlayCommand.
/// </summary>
public class MainViewModelTests
{
    // A computer with a fixed "random" seed and no thinking time, so tests are fast and repeatable.
    private static MainViewModel CreateVsComputer(int thinkingMilliseconds = 0) =>
        new(new ComputerPlayer(new Random(1)), TimeSpan.FromMilliseconds(thinkingMilliseconds));

    private static MainViewModel CreateVsFriend() =>
        new(new ComputerPlayer(new Random(1)), TimeSpan.Zero) { PlayAgainst = Opponent.Friend };

    private static async Task Click(MainViewModel vm, params int[] cells)
    {
        foreach (var cell in cells)
            await vm.Cells[cell].PlayCommand.ExecuteAsync(null);
    }

    private static int CountMarks(MainViewModel vm, string symbol) =>
        vm.Cells.Count(cell => cell.Symbol == symbol);

    // ---------- Against the computer (the default) ----------

    [Fact]
    public void StartsAgainstTheComputer_AndYouGoFirst()
    {
        var vm = new MainViewModel();

        Assert.Equal(Opponent.Computer, vm.PlayAgainst);
        Assert.Equal("You (X)", vm.XName);
        Assert.Equal("Computer (O)", vm.OName);
        Assert.Equal("Your turn", vm.StatusText);
        Assert.True(vm.IsXActive);
        Assert.All(vm.Cells, cell => Assert.True(cell.PlayCommand.CanExecute(null)));
        Assert.False(vm.ResetScoresCommand.CanExecute(null)); // nothing to reset yet
    }

    [Fact]
    public async Task AfterYourMove_TheComputerAnswers()
    {
        var vm = CreateVsComputer();

        await Click(vm, 4);

        Assert.Equal("X", vm.Cells[4].Symbol);
        Assert.Equal(1, CountMarks(vm, "X"));
        Assert.Equal(1, CountMarks(vm, "O"));
        Assert.Equal("Your turn", vm.StatusText);
    }

    [Fact]
    public async Task WhileTheComputerThinks_TheBoardIsLocked()
    {
        var vm = CreateVsComputer(thinkingMilliseconds: 200);

        var move = Click(vm, 4); // don't wait for the computer yet

        Assert.Equal("Computer is thinking…", vm.StatusText);
        Assert.True(vm.IsOActive);
        Assert.All(vm.Cells, cell => Assert.False(cell.PlayCommand.CanExecute(null)));

        await move;

        Assert.Equal("Your turn", vm.StatusText);
        Assert.Equal(1, CountMarks(vm, "O"));
    }

    [Fact]
    public async Task TheComputerStarts_EveryOtherRound()
    {
        var vm = CreateVsComputer();

        await vm.NewGameCommand.ExecuteAsync(null); // round 2: the computer (O) starts

        Assert.Equal(0, CountMarks(vm, "X"));
        Assert.Equal(1, CountMarks(vm, "O"));
        Assert.Equal("Your turn", vm.StatusText);
    }

    [Fact]
    public async Task WhenTheComputerWins_ItSaysSo()
    {
        var vm = CreateVsComputer();
        vm.Level = Difficulty.Hard;

        // Bad moves on purpose. Hard must answer 4, then block at 2, then wins with 2-4-6.
        await Click(vm, 0, 1, 8);

        Assert.Equal("Computer wins!", vm.StatusText);
        Assert.Equal(1, vm.OWins);
        Assert.True(vm.IsOActive);
        Assert.Equal(new[] { 2, 4, 6 }, vm.Cells.Where(c => c.IsWinning).Select(c => c.Index));
    }

    [Fact]
    public async Task NewGame_WhileTheComputerThinks_ThrowsAwayItsOldMove()
    {
        var vm = CreateVsComputer(thinkingMilliseconds: 100);

        var oldRound = Click(vm, 4);                // the computer starts thinking about round 1...
        await vm.NewGameCommand.ExecuteAsync(null); // ...but we start round 2 (the computer opens it)
        await oldRound;

        // Only the computer's opening move of round 2 is on the board.
        Assert.Equal(0, CountMarks(vm, "X"));
        Assert.Equal(1, CountMarks(vm, "O"));
    }

    [Fact]
    public async Task ChangingTheOpponent_StartsAFreshMatch()
    {
        var vm = CreateVsFriend();
        await Click(vm, 0, 3, 1, 4, 2);             // X wins
        await vm.NewGameCommand.ExecuteAsync(null); // round 2 would be O's turn to start

        vm.PlayAgainst = Opponent.Computer;

        Assert.Equal(0, vm.XWins);
        Assert.All(vm.Cells, cell => Assert.Equal("", cell.Symbol));
        Assert.Equal("You (X)", vm.XName);
        Assert.Equal("Your turn", vm.StatusText); // a new match always starts with X (you)
        Assert.False(vm.ResetScoresCommand.CanExecute(null));
    }

    // ---------- Two players on one PC ("Friend") ----------

    [Fact]
    public void AgainstAFriend_ThePlayersAreXAndO()
    {
        var vm = CreateVsFriend();

        Assert.Equal("Player X", vm.XName);
        Assert.Equal("Player O", vm.OName);
        Assert.Equal("Player X's turn", vm.StatusText);
        Assert.True(vm.IsXActive);
    }

    [Fact]
    public async Task ClickingACell_ShowsTheMark_AndPassesTheTurn()
    {
        var vm = CreateVsFriend();

        await Click(vm, 4);

        Assert.Equal("X", vm.Cells[4].Symbol);
        Assert.True(vm.Cells[4].IsX);
        Assert.False(vm.Cells[4].PlayCommand.CanExecute(null)); // the button is now disabled
        Assert.Equal("Player O's turn", vm.StatusText);
        Assert.True(vm.IsOActive);
    }

    [Fact]
    public async Task ClickingATakenCell_DoesNothing()
    {
        var vm = CreateVsFriend();

        await Click(vm, 4, 4);

        Assert.Equal("X", vm.Cells[4].Symbol);
        Assert.Equal("Player O's turn", vm.StatusText);
    }

    [Fact]
    public async Task Win_HighlightsTheLine_LocksTheBoard_AndCountsTheScore()
    {
        var vm = CreateVsFriend();

        await Click(vm, 0, 3, 1, 4, 2); // X takes the top row

        Assert.Equal("Player X wins!", vm.StatusText);
        Assert.True(vm.IsXActive); // the winner's card stays highlighted
        Assert.Equal(1, vm.XWins);
        Assert.Equal(new[] { 0, 1, 2 }, vm.Cells.Where(c => c.IsWinning).Select(c => c.Index));
        Assert.All(vm.Cells, cell => Assert.False(cell.PlayCommand.CanExecute(null)));
        Assert.True(vm.ResetScoresCommand.CanExecute(null));
    }

    [Fact]
    public async Task Draw_IsShown_AndCounted()
    {
        var vm = CreateVsFriend();

        await Click(vm, 0, 4, 8, 1, 7, 6, 2, 5, 3);

        Assert.Equal("It's a draw!", vm.StatusText);
        Assert.Equal(1, vm.Draws);
        Assert.False(vm.IsXActive || vm.IsOActive); // nobody highlighted
        Assert.DoesNotContain(vm.Cells, cell => cell.IsWinning);
    }

    [Fact]
    public async Task NewGame_ClearsTheBoard_AndTheOtherPlayerStarts()
    {
        var vm = CreateVsFriend();
        await Click(vm, 0, 3, 1, 4, 2); // X wins

        await vm.NewGameCommand.ExecuteAsync(null);

        Assert.All(vm.Cells, cell =>
        {
            Assert.Equal("", cell.Symbol);
            Assert.False(cell.IsWinning);
            Assert.True(cell.PlayCommand.CanExecute(null));
        });
        Assert.Equal("Player O's turn", vm.StatusText);
        Assert.Equal(1, vm.XWins); // the score is kept
    }

    [Fact]
    public async Task ResetScore_SetsTheScoreboardToZero()
    {
        var vm = CreateVsFriend();
        await Click(vm, 0, 3, 1, 4, 2); // X wins

        vm.ResetScoresCommand.Execute(null);

        Assert.Equal(0, vm.XWins);
        Assert.Equal(0, vm.OWins);
        Assert.Equal(0, vm.Draws);
        Assert.False(vm.ResetScoresCommand.CanExecute(null));
    }
}
