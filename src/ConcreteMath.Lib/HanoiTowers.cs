namespace ConcreteMath.Lib;

public static class HanoiTowers
{
    public readonly record struct Step(int From, int To, int BlockSize);

    public static IEnumerable<Step> Solve(int blocks)
    {
        return Solve(blocks, 1, 3, 2);
    }

    private static IEnumerable<Step> Solve(int blocks, int from, int to, int spare)
    {
        if (blocks < 1) return [];
        if (blocks == 1) return [new(from, to, blocks)];

        return Solve(blocks - 1, from, spare, to)
            .Concat([new(from, to, blocks)])
            .Concat(Solve(blocks - 1, spare, to, from));
    }
}