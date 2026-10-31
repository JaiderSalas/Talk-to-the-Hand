using System.Collections.Generic;
using UnityEngine;

public static class SequenceGenerator
{
    /// <summary>
    /// Creates a random list of Emy's moves (in Emy's point of view).
    /// verticalOnly = true uses only Up/Down. Used in the "mirror" stage so the
    /// player learns the hand swap first, before the left/right flip is added.
    /// </summary>
    public static List<Move> Generate(int length, bool verticalOnly)
    {
        var list = new List<Move>(length);
        for (int i = 0; i < length; i++)
        {
            Hand h = Random.value < 0.5f ? Hand.Left : Hand.Right;
            Direction d;
            if (verticalOnly)
                d = Random.value < 0.5f ? Direction.Up : Direction.Down;
            else
                d = (Direction)Random.Range(0, 4);
            list.Add(new Move(h, d));
        }
        return list;
    }
}
