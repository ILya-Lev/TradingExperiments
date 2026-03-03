namespace Udemy.Fin.Stat;

public static class ElevatorProblemStateCounter
{
    public static long GetStates(int floors, int people)
    {
        if (people == 0) return 0;
        if (floors == 1) return 1;
        if (people == 1) return floors;

        return floors * Enumerable.Range(1, people)
            .Select(leaving => GetStates(floors - 1, people - leaving))
            .Sum();
    }
}

