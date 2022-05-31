using System;
using System.Collections.Generic;

public static class RoundsGenerator
{
    public static IEnumerator<int> InfiniteRounds()
    {
        var possibleRoundsList = new List<int>() { 1, 4, 5 };
        var rand = new Random();
        int? index = null;
        while (true)
        {
            var newIndex = rand.Next(possibleRoundsList.Count - (index == null ? 0 : 1));
            index = index != null && newIndex >= index ? newIndex + 1 : newIndex;
            yield return possibleRoundsList[(int) index];
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
