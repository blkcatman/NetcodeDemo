#nullable enable

using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSyncTransformBehaviour : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5f;
    
    private PlayerInputHelper? playerInputHelper = null;

    private GameObject? playerLocalDummy = null;

    [SerializeField]
    private UnityEvent<GameObject>? onTrackingObjectPresented;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerInputHelper = FindObjectOfType<PlayerInputHelper>();

            onTrackingObjectPresented?.Invoke(gameObject);

            SetRandomPosition();
        }
    }

    private void SetRandomPosition()
    {
        var position = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
        transform.position = position;
    }

    private void UpdatePlayerInputs(float delta)
    {
        if (playerInputHelper == null) return;
        
        if (playerInputHelper!.HasMoveInput)
        {
            var move = playerInputHelper!.Move;
            var direction = new Vector3(move.x, 0f, move.y);
                    
            var position = transform.position;
            transform.LookAt(direction + position, Vector3.up);
            transform.position += direction * speed * delta;
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
