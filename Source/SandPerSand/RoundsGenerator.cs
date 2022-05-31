using System;
using System.Collections.Generic;

public static class RoundsGenerator
{
    public static IEnumerator<int> InfiniteRounds()
    {
        var possibleRoundsList = new List<int>() { 1, 2 };
        var rand = new Random();
        while (true)
        {
            yield return possibleRoundsList[rand.Next(possibleRoundsList.Count)];
        }
    }

    public static IEnumerator<int> Rounds(int count)
    {
        var rounds = InfiniteRounds();
        for (var i = 0; i < count; ++i)
        {
            rounds.MoveNext();
            yield return rounds.Current;
        }
    }
}
