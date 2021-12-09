#nullable enable

using Unity.Netcode;
using UnityEngine;

public class PlayerSyncShootingBehaviour : NetworkBehaviour
{
    [SerializeField]
    private GameObject? bulletPrefab;

    [SerializeField]
    private float fireRate = 0.5f;

    [SerializeField]
    private GameObject? gunRoot;

    private PlayerInputHelper? playerInputHelper = null;

    private float fireDelta;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerInputHelper = FindObjectOfType<PlayerInputHelper>();
        }
    }

    private void GenerateBulletOnServer(Vector3 position, Quaternion rotation)
    {
        var bullet = Instantiate(
            bulletPrefab,
            position,
            rotation
        );
        bullet?.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void SubmitGeneratingBulletRequestServerRpc(Vector3 position, Quaternion rotation)
    {
        GenerateBulletOnServer(position, rotation);
    }

    private void UpdatePlayerInputs(float delta)
    {
        if (playerInputHelper == null || gunRoot == null) return;

        fireDelta += delta;

        if (fireDelta > fireRate && playerInputHelper!.HasFireInput)
        {
            fireDelta = 0;
            if (IsServer)
            {
                GenerateBulletOnServer(gunRoot.transform.position, gunRoot.transform.rotation);
            }
            else
            {
                SubmitGeneratingBulletRequestServerRpc(gunRoot.transform.position, gunRoot.transform.rotation);
            }
        }
    }

    private void Update()
    {
        var delta = Time.deltaTime;

        if (IsOwner)
        {
            UpdatePlayerInputs(delta);
        }
    }
}
