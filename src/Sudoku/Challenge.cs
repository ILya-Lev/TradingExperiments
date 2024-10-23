namespace Sudoku;

public static class Challenge
{
    /*
     * a-> 2r
     * r->b
     * b->g
     * g->a+2b

     * v->r
     */
    //initial state 0004 for abgr

    
    public record struct State(int A, int R, int B, int G, int V);

    public static IEnumerable<State> Evolve(State initial, IEnumerable<int> steps)
    {
        var actions = new[]
        {
            (State before, int shift) => before with { A = (before.A + shift) % 5, R = (before.R + shift * 2) % 5 },
            (State before, int shift) => before with { V = (before.V + shift) % 5, R = (before.R + shift) % 5 },
            (State before, int shift) => before with { B = (before.B + shift) % 5, G = (before.G + shift) % 5 },
            (State before, int shift) => before with { G = (before.G + shift) % 5, A = (before.A + shift) % 5, B = (before.B + shift * 2) % 5 },
            (State before, int shift) => before with { R = (before.R + shift) % 5, B = (before.B + shift) % 5 },
        };
        var actionIndex = 0;
        var current = initial;
        foreach (var step in steps)
        {
            current = actions[actionIndex](current, step);
            actionIndex = (actionIndex + 1) % actions.Length;
            yield return current;
        }
    }
}