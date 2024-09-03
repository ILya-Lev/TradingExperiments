namespace BlackScholesMerton;

public class BinomialTreeModel
{
    private const int RoundDigits = 8;
    public Node Root { get; }
    public IReadOnlyList<Node> Leaves { get; }

    /// <summary>
    /// Martingale probability of going up the tree
    /// i.e. S(i) = E[discount * S(i+1)] = discount * (q*S(i)*u + (1-q)*S(i)*d)
    /// as the tree is binary (up/down moves only)
    /// => q = (1/discount - d) / (u-d);
    /// </summary>
    public double Q { get; }

    /// <summary> price change when going up the tree </summary>
    public double U { get; }
    
    /// <summary> price change when going down the tree </summary>
    public double D { get; }

    /// <summary> a discount to be applied on each step for payoff calculation based on child nodes </summary>
    public double Discount { get; }

    /// <summary> payoff of a given node if executed right now - expects current node's Strike price only </summary>
    public Func<double, double> InstantPayoff { get; }

    /// <summary>
    /// calculates payoffs for each node of cox-Ross-Rubinstein model
    /// </summary>
    /// <param name="isEuropean">tells if the option could be exercised right now (false) or not (true), affects payoff usage</param>
    /// <param name="n">number of steps</param>
    /// <param name="s">initial strike price S(0)</param>
    /// <param name="u">going up factor</param>
    /// <param name="d">going down factor</param>
    /// <param name="discount">generates discount factor after 1 step</param>
    /// <param name="instantPayoff">payoff of a given node if executed right now </param>
    public BinomialTreeModel(bool isEuropean
        , int n, double s, double u, double d
        , double discount, Func<double, double> instantPayoff)
    {
        if (!(u > 1 / discount && 1 / discount > d))
            throw new Exception($"Invalid parameters - the relation is not followed: {u} > {1 / discount} > {d}");

        U = u;
        D = d;
        Discount = Math.Round(discount, RoundDigits);
        InstantPayoff = instantPayoff;
        Q = Math.Round((1 / discount - d) / (u - d), RoundDigits);
        
        Root = new() { Strike = s };
        Leaves = BuildTreeAndAssignStrikePrices(n);

        AssignPayoffs(GetPayoff(isEuropean));
    }
    
    private List<Node> BuildTreeAndAssignStrikePrices(int n)
    {
        var layer = new List<Node>(){Root};
        for (int step = 0; step < n; step++)
        {
            //shows than each new layer has just +1 node; makes sense as it's path independent model
            var nextLayer = new List<Node> { CreateUpChild(layer[0]) };

            foreach (var node in layer)
            {
                node.Up = nextLayer.Last();
                nextLayer.Last().ParentUp = node;

                node.Down = CreateDownChild(node);
                nextLayer.Add(node.Down);
            }
            layer = nextLayer;
        }
        return layer;
    }

    private Node CreateUpChild(Node node) => new() { ParentUp = node, Strike = node.Strike * U };
    private Node CreateDownChild(Node node) => new() { ParentDown = node, Strike = node.Strike * D };
    
    private Func<Node, double> GetPayoff(bool isEuropean) => isEuropean ? GetEuropeanPayoff : GetAmericanPayoff;
    private double GetEuropeanPayoff(Node node) => Discount * (node.Up!.Payoff * Q + node.Down!.Payoff * (1 - Q));
    private double GetAmericanPayoff(Node node) => Math.Max(InstantPayoff(node.Strike), GetEuropeanPayoff(node));
    
    private void AssignPayoffs(Func<Node, double> payoff)
    {
        foreach (var leaf in Leaves)
            leaf.Payoff = Math.Round(InstantPayoff(leaf.Strike), RoundDigits);

        var layer = Leaves.ToList();
        while (layer.Count > 1)
        {
            var previousLayer = new List<Node>();
            foreach (var leaf in layer.Skip(1))//shows that each previous (parent) layer has 1 node less than current
            {
                var node = leaf.ParentDown!;
                node.Payoff = Math.Round(payoff(node), RoundDigits);
                previousLayer.Add(node);
            }    
            layer = previousLayer;
        }
    }

    public record Node
    {
        public Node? ParentUp { get; set; }
        public Node? ParentDown { get; set; }
        public Node? Up { get; set; }
        public Node? Down { get; set; }
        public double Strike { get; init; }
        public double Payoff { get; set; }
    }
}