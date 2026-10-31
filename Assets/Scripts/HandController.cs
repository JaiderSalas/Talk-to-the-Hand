using System.Collections;
using UnityEngine;

/// <summary>
/// Moves a hand object from its rest position in one of four directions and back.
/// Put one on each of the 4 hands (2 for the player, 2 for Emy).
/// Tick "facesPlayer" on Emy's hands: she looks the other way, so her
/// left/right offsets are flipped on screen.
/// </summary>
public class HandController : MonoBehaviour
{
    [Header("Movement")]
    public float distance = 0.35f;
    public float moveTime = 0.10f;
    public float holdTime = 0.25f;
    public bool facesPlayer = false;

    [Header("Feedback")]
    public Renderer handRenderer; // optional; found automatically if empty

    Vector3 restPosition;
    Color baseColor = Color.white;
    Coroutine moveRoutine;
    Coroutine flashRoutine;

    void Awake()
    {
        restPosition = transform.localPosition;
        if (handRenderer == null) handRenderer = GetComponentInChildren<Renderer>();
        if (handRenderer != null) baseColor = handRenderer.material.color;
    }

    public void Perform(Direction dir)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(restPosition + Offset(dir)));
    }

    Vector3 Offset(Direction dir)
    {
        float x = 0f, y = 0f;
        switch (dir)
        {
            case Direction.Up: y = 1f; break;
            case Direction.Down: y = -1f; break;
            case Direction.Left: x = -1f; break;
            case Direction.Right: x = 1f; break;
        }
        if (facesPlayer) x = -x; // Emy's left is the camera's right
        return new Vector3(x, y, 0f) * distance;
    }

    IEnumerator MoveRoutine(Vector3 target)
    {
        yield return Slide(transform.localPosition, target);
        yield return new WaitForSeconds(holdTime);
        yield return Slide(transform.localPosition, restPosition);
        moveRoutine = null;
    }

    IEnumerator Slide(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < moveTime)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01(t / moveTime));
            yield return null;
        }
        transform.localPosition = to;
    }

    public void Flash(Color color, float duration = 0.35f)
    {
        if (handRenderer == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(color, duration));
    }

    IEnumerator FlashRoutine(Color color, float duration)
    {
        handRenderer.material.color = color;
        yield return new WaitForSeconds(duration);
        handRenderer.material.color = baseColor;
        flashRoutine = null;
    }
}
