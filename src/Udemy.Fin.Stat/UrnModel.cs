namespace Udemy.Fin.Stat;

public class UrnModel(int[] State, int[][] Deltas)
{
    public double GetProbabilityOfPickingColorAt(int colorIndex, int attempt)
    {
        var states = PickOne()
            .Select((el, idx) => (el, idx))
            .ToDictionary
            (
                item => item.idx, //color
                item => new[] { item.el }.ToList()
            );
        
        for (int generation = 1; generation < attempt; generation++)
        {
            var nextStates = states.ToDictionary(p => p.Key, p => new List<(double, UrnModel)>());

            foreach (var (_, stateChain) in states)
            {
                foreach (var (tp, state) in stateChain)
                {
                    var transfers = state.PickOne().ToArray();

                    for (int color = 0; color < transfers.Length; color++)
                    {
                        var nextTransferProbability = tp * transfers[color].transferProbability;
                        nextStates[color].Add((nextTransferProbability, transfers[color].nextState));
                    }
                }
            }

            states = nextStates;
        }

        return states[colorIndex].Sum(transfer => transfer.transferProbability);
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