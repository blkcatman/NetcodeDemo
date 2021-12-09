#nullable enable

using Unity.Netcode;
using UnityEngine;

public class BulletSyncBehaviour : NetworkBehaviour
{
    [SerializeField]
    private float speed = 30f;

    [SerializeField]
    private float lifeTime = 4f;

    private float elapsedTime;

    private void UpdateMoving(float delta)
    {
        var direction = transform.forward;
        transform.position += direction * speed * delta;
    }

    private void UpdateLifeTime(float delta)
    {
        if (elapsedTime > lifeTime)
        {
            if (IsServer)
            {
                RemoveBulletOnServer();
            }
            else
            {
                SubmitRemovingBulletRequestServerRpc();
            }
        }
    }

    private void RemoveBulletOnServer()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }

    [ServerRpc]
    private void SubmitRemovingBulletRequestServerRpc()
    {
        RemoveBulletOnServer();
    }

    private void Update()
    {
        var delta = Time.deltaTime;

        elapsedTime += delta;
        if (IsOwner)
        {
            UpdateMoving(delta);
            UpdateLifeTime(delta);
        }
    }
}
