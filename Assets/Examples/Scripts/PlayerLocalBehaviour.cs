using UnityEngine;

public class PlayerLocalBehaviour : MonoBehaviour
{
    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }
}
