using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Integration")]
public class SolverTests(ITestOutputHelper output)
{
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

        var initial = new Field(digits);

        var solved = Solver.Solve(initial);

        output.WriteLine(solved.Print());
        solved.IsSolved.Should().BeTrue();
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

        var initial = new Field(digits);

        var solved = Solver.Solve(initial);

        output.WriteLine(solved.Print());
        solved.IsSolved.Should().BeTrue();
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

        var initial = new Field(digits);

        var solved = Solver.Solve(initial);

        output.WriteLine(solved.Print());
        solved.IsSolved.Should().BeTrue();
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

        var initial = new Field(digits);

        var solved = Solver.Solve(initial);

        output.WriteLine(solved.Print());
        solved.IsSolved.Should().BeTrue();
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

        var initial = new Field(digits);

        var solved = Solver.Solve(initial);

        output.WriteLine(solved.Print());
        solved.IsSolved.Should().BeTrue();
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

        var initial = new Field(digits);

        var solved = Solver.Solve(initial);

        output.WriteLine(solved.Print());
        solved.IsSolved.Should().BeTrue();
    }
}