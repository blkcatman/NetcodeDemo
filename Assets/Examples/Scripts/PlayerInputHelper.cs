using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHelper : MonoBehaviour
{
    [SerializeField]
    private float stickDeadZone = 0.1f;

    [SerializeField]
    private float triggerDeadZone = 0.4f;

    public Vector2 Move { get; private set; }

    public float Fire { get; private set; }

    public bool HasMoveInput => Move.magnitude > stickDeadZone;

    public bool HasFireInput => Fire > triggerDeadZone;

    public void OnMove(InputValue value) => Move = value.Get<Vector2>();

    public void OnFire(InputValue value) => Fire = value.Get<float>();
}
