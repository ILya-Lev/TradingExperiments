using System.Collections.Concurrent;

namespace Udemy.Fin.Stat;

public class UrnModel(int[] State, int[][] Deltas)
{
    private static readonly ParallelOptions _parallelismOptions = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount - 1
    };

    public IReadOnlyList<double> GetProbabilityOfPickingColorAt(int attempt)
    {
        var states = PickOne()
            .Select((el, idx) => (el, idx))
            .ToDictionary
            (
                item => item.idx, //color
                item => new ConcurrentBag<(double transferProbability, UrnModel state)>() { item.el }
            );
        
        for (int generation = 1; generation < attempt; generation++)
        {
            var nextStates = states.ToDictionary(p => p.Key, p => new ConcurrentBag<(double, UrnModel)>());

            foreach (var (_, stateChain) in states)
            {
                Parallel.ForEach(stateChain, _parallelismOptions, (pair, loopState, index) =>
                    //foreach (var (tp, state) in pair.Value)
                {
                    var transfers = pair.state.PickOne().ToArray();

                    for (int color = 0; color < transfers.Length; color++)
                    {
                        var np = pair.transferProbability * transfers[color].transferProbability;
                        nextStates[color].Add((np, transfers[color].nextState));
                    }
                });
            }

            states = nextStates;
        }

        return states.Select(p => p.Value.Sum(transfer => transfer.transferProbability)).ToArray();
    }

    public IEnumerable<(double transferProbability, UrnModel nextState)> PickOne()
    {
        var total = State.Sum() * 1.0;
        for (int i = 0; i < State.Length; i++)
        {
            var transferProbability = State[i] / total;
            var nextState = State.Zip(Deltas[i], (s, d) => s + d).ToArray();
            yield return (transferProbability, new UrnModel(nextState, Deltas));
        }
    }
}