using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads fixed keys and raises events.
/// Left hand:  W A S D
/// Right hand: Arrow keys
/// Confirm:    Space
/// Needs the Input System package and Active Input Handling set to
/// "Input System Package (New)" or "Both".
/// </summary>
public class InputReader : MonoBehaviour
{
    public event Action<Move> OnMove;
    public event Action OnConfirm;
    public event Action OnRetry;
    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Left hand
        if (kb.wKey.wasPressedThisFrame) Fire(Hand.Left, Direction.Up);
        if (kb.sKey.wasPressedThisFrame) Fire(Hand.Left, Direction.Down);
        if (kb.aKey.wasPressedThisFrame) Fire(Hand.Left, Direction.Left);
        if (kb.dKey.wasPressedThisFrame) Fire(Hand.Left, Direction.Right);

        // Right hand
        if (kb.upArrowKey.wasPressedThisFrame) Fire(Hand.Right, Direction.Up);
        if (kb.downArrowKey.wasPressedThisFrame) Fire(Hand.Right, Direction.Down);
        if (kb.leftArrowKey.wasPressedThisFrame) Fire(Hand.Right, Direction.Left);
        if (kb.rightArrowKey.wasPressedThisFrame) Fire(Hand.Right, Direction.Right);

        if (kb.spaceKey.wasPressedThisFrame) OnConfirm?.Invoke();
        if (kb.rKey.wasPressedThisFrame) OnRetry?.Invoke();
    }

    void Fire(Hand hand, Direction dir)
    {
        OnMove?.Invoke(new Move(hand, dir));
    }
}
