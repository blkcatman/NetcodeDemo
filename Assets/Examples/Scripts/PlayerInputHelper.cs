using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHelper : MonoBehaviour
{
    public Vector2 move { get; private set; }

    public void OnMove(InputValue value) => move = value.Get<Vector2>();
}
