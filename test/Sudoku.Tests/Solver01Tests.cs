using FluentAssertions;
using Sudoku01;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Integration")]
public class Solver01Tests(ITestOutputHelper output)
{
    private static Cell[] DigitsToCells(int[][] digits) => digits
        .SelectMany((row, r) => row.Select((d, c) => new Sudoku01.Cell(r, c, d)))
        .ToArray();

    private static string Print(Sudoku01.Field field)
        =>" " + string.Join(" ", field.Cells
            .Select(c => c.Column == 8 ? $"{c.Digit}{Environment.NewLine}" : $"{c.Digit}"));

    [Fact]
    public void Solve_Easy001_Solved()
    {
        var digits = new int[][]
        {
            new []{0,0,5,3,4,2,0,8,0},
            new []{4,0,0,0,0,0,0,0,0},
            new []{6,8,0,1,0,0,0,5,0},
            
            new []{0,0,4,0,1,0,0,3,0},
            new []{1,3,2,6,0,5,0,0,0},
            new []{0,9,0,7,0,4,1,0,2},
            
            new []{0,4,0,0,0,1,7,0,0},
            new []{2,0,6,4,7,3,8,0,1},
            new []{0,7,1,9,0,6,0,4,0},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }
    
    [Fact]
    public void Solve_Medium001_Solved()
    {
        var digits = new int[][]
        {
            new []{8,0,2,4,0,0,1,5,7},
            new []{1,0,6,7,5,3,0,0,0},
            new []{0,0,5,0,0,8,3,9,6},
            
            new []{0,2,9,0,0,5,0,6,0},
            new []{0,0,8,0,6,0,4,0,5},
            new []{0,0,1,3,7,0,0,0,0},
            
            new []{0,1,0,8,0,7,5,0,0},
            new []{0,0,7,0,4,0,0,0,0},
            new []{9,0,4,5,0,0,0,7,2},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]
    public void Solve_Hard001_Solved()
    {
        var digits = new int[][]
        {
            new []{0,8,0,7,0,0,0,0,6},
            new []{0,0,0,2,0,0,0,4,0},
            new []{7,5,1,4,9,0,0,3,0},
            
            new []{0,0,4,0,6,0,3,0,8},
            new []{3,0,0,1,0,5,4,0,0},
            new []{0,0,0,0,0,4,5,6,0},
            
            new []{0,7,0,0,0,0,0,0,0},
            new []{0,3,8,0,4,0,2,0,0},
            new []{0,0,0,0,0,7,6,1,0},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]//double split fit - allows to solve
    public void Solve_Expert001_Solved()
    {
        var digits = new int[][]
        {
            new []{6,0,0,0,3,0,0,9,0},
            new []{0,0,0,7,0,4,5,8,3},
            new []{0,3,0,5,0,0,1,0,0},
            
            new []{5,6,8,4,0,0,9,0,0},
            new []{0,4,0,0,0,0,6,0,0},
            new []{0,0,7,6,0,3,0,0,0},
            
            new []{0,0,1,0,0,0,0,0,2},
            new []{9,0,0,8,7,0,3,5,0},
            new []{0,0,0,0,0,6,8,0,0},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]//double split fit - allows to solve
    public void Solve_Master001_Solved()
    {
        var digits = new int[][]
        {
            new []{0,2,0,5,0,8,9,0,3},
            new []{6,8,0,1,9,0,0,0,0},
            new []{0,0,0,3,4,0,0,0,7},
            
            new []{0,0,1,9,0,0,0,4,5},
            new []{0,0,0,0,0,0,8,0,0},
            new []{3,0,9,0,0,0,0,0,0},
            
            new []{0,0,2,0,0,5,0,0,0},
            new []{0,0,0,7,0,0,1,6,0},
            new []{7,0,0,0,1,0,5,0,8},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]//double split fit - allows to solve
    public void Solve_Extreme001_Solved()
    {
        var digits = new int[][]
        {
            new []{4,0,0,0,0,0,7,0,0},
            new []{0,9,0,1,0,0,0,0,0},
            new []{0,0,3,0,0,9,6,0,8},
            
            new []{0,0,2,0,0,0,0,1,0},
            new []{6,0,0,5,0,0,8,0,2},
            new []{0,0,0,0,0,8,0,7,0},
            
            new []{0,0,6,0,0,1,9,0,3},
            new []{0,0,0,0,0,0,0,2,0},
            new []{5,0,0,0,4,0,0,0,0},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]//double split fit - allows to solve
    public void Solve_Extreme002_Solved()
    {
        var digits = new int[][]
        {
            new []{3,0,0,0,8,0,9,0,0},
            new []{0,8,4,7,0,0,6,0,0},
            new []{5,0,0,0,0,0,0,4,0},
            
            new []{0,0,0,5,0,0,0,0,9},
            new []{0,3,0,0,0,0,0,0,0},
            new []{0,6,8,0,9,0,0,1,0},
            
            new []{0,9,1,0,5,0,0,6,0},
            new []{0,0,0,0,0,2,7,0,0},
            new []{4,0,0,0,0,0,0,0,0},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]//double split fit - allows to solve
    public void Solve_Extreme003_Solved()
    {
        var digits = new int[][]
        {
            new []{0,0,8,0,4,0,0,0,0},
            new []{0,0,2,0,0,1,4,0,3},
            new []{0,0,0,0,0,0,8,0,0},
            
            new []{0,0,0,9,0,0,5,6,0},
            new []{5,0,0,0,8,7,0,0,0},
            new []{0,0,0,2,0,0,0,9,0},
            
            new []{0,3,1,0,0,0,0,0,0},
            new []{0,9,0,0,0,2,7,0,0},
            new []{0,0,7,5,0,0,0,0,9},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();
    }

    [Fact]//double split fit - allows to solve
    public void Solve_Extreme004_Solved()
    {
        var digits = new int[][]
        {
            new []{0,9,8,0,0,4,2,5,0},
            new []{0,0,0,2,0,0,0,0,3},
            new []{0,0,5,0,0,0,0,0,8},
            
            new []{0,0,3,4,0,0,1,7,0},
            new []{6,0,0,0,0,9,0,0,0},
            new []{0,0,0,0,0,0,0,0,2},
            
            new []{0,3,0,0,7,0,0,0,0},
            new []{0,0,0,0,0,0,0,8,0},
            new []{0,0,7,1,0,0,5,4,0},
        };

        var initial = new Sudoku01.Field(DigitsToCells(digits));

        var solved = Solver01.Solve(initial)!;

        output.WriteLine(Print(solved));
        solved.IsSolved.Should().BeTrue();
        solved.FindInconsistencies().Should().BeEmpty();

        //var steps = MaxPointsFiller.GetFillingSequence(initial, solved).ToArray();
        //foreach (var (r,c,d) in steps)
        //{
        //    output.WriteLine($"{r} {c} {d}");
        //}
    }
}