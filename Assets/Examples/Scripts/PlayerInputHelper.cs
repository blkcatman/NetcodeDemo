using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHelper : MonoBehaviour
{
    [SerializeField]
    private float stickDeadZone = 0.1f;

    public Vector2 Move { get; private set; }

    public bool HasMoveInput => Move.magnitude > stickDeadZone;

    public void OnMove(InputValue value) => Move = value.Get<Vector2>();
}
