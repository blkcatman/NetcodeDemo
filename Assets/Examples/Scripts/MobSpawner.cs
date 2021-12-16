#nullable enable

using Unity.Netcode;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{

    [SerializeField]
    private float spawnDelay = 7f;
    
    [SerializeField]
    private GameObject? mobPrefab;
    

    private float elapsedTime;

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > spawnDelay)
        {
            if (mobPrefab != null)
            {
                var mob = Instantiate(mobPrefab);
                mob?.GetComponent<NetworkObject>().Spawn();
            }
            elapsedTime -= spawnDelay;
        }
    }
}
