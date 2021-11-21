#nullable enable

using UnityEngine;

public class PlayerChaseCamera : MonoBehaviour
{
    [SerializeField]
    private Camera? playerCamera;
    
    private GameObject? playerTarget;

    public void SetAsPlayer(in GameObject playerObject)
    {
        playerTarget = playerObject;
    }

    public bool TryActivatePlayerCamera(in Camera currentCamera, out Camera? activatedCamera)
    {
        if (playerCamera != null)
        {
            currentCamera.enabled = false;
            playerCamera.enabled = true;
            activatedCamera = playerCamera;
            return true;
        }
        else
        {
            activatedCamera = null;
            return false;
        }
    }
    
    void Update()
    {
        Vector3? playerPosition = playerTarget?.transform.position;
        if (playerPosition.HasValue)
        {
            var delta = Time.deltaTime;
            var current = transform.position;
            var target = playerPosition.Value;
            transform.position = Vector3.Lerp(current, target, delta * 5f);
        }
    }
}
