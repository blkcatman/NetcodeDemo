#nullable enable

using UnityEngine;

public class PlayerChaseCameraMounter : MonoBehaviour
{
    public void Mount(GameObject cameraTarget)
    {
        var playerCamera = FindObjectOfType<PlayerChaseCamera>();
        playerCamera?.SetAsPlayer(cameraTarget);
        playerCamera?.TryActivatePlayerCamera(Camera.main, out _);
    }
}
