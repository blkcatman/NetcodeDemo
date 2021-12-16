#nullable enable

using UnityEngine;

public class BulletLocalBehaviour : MonoBehaviour
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
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        var delta = Time.deltaTime;

        elapsedTime += delta;
        UpdateMoving(delta);
        UpdateLifeTime(delta);
    }
}