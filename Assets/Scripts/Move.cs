using System;

public enum Hand { Left, Right }
public enum Direction { Up, Down, Left, Right }

/// <summary>
/// One hand action: which hand, which direction.
/// For Emy the values are in HER point of view (her left hand, her left side).
/// </summary>
[Serializable]
public struct Move
{
    public Hand hand;
    public Direction direction;

    public Move(Hand hand, Direction direction)
    {
        this.hand = hand;
        this.direction = direction;
    }

    /// <summary>
    /// The mirror rule. Emy faces the player, so:
    ///  - her left hand is the player's right hand (and the opposite)
    ///  - her left side is the player's right side (and the opposite)
    ///  - up and down stay the same
    /// Call this on Emy's move to get what the PLAYER must press.
    /// </summary>
    public Move Mirror()
    {
        Hand h = hand == Hand.Left ? Hand.Right : Hand.Left;
        Direction d = direction;
        if (d == Direction.Left) d = Direction.Right;
        else if (d == Direction.Right) d = Direction.Left;
        return new Move(h, d);
    }

    public bool Matches(Move other)
    {
        return hand == other.hand && direction == other.direction;
    }

    /// <summary>Text Emy shows on screen (her point of view).</summary>
    public string ToInstruction()
    {
        string h = hand.ToString().ToUpper();
        switch (direction)
        {
            case Direction.Up: return $"My {h} hand goes UP";
            case Direction.Down: return $"My {h} hand goes DOWN";
            case Direction.Left: return $"My {h} hand goes to MY LEFT";
            default: return $"My {h} hand goes to MY RIGHT";
        }
    }

    public override string ToString()
    {
        return $"{hand}-{direction}";
    }
}
