#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator? animator;

    private bool speedEnebled;
    private bool motionSpeedEnebled;

    private Vector3 beforePosition;

    private bool updateRotation = true;

    public void SetRotationUpdate(bool flag) => updateRotation = flag;

    private void UpdateAnimation(Vector3 direction, float delta)
    {
        var norm = direction.magnitude;
        if (speedEnebled && animator != null)
        {
            animator.SetFloat("Speed", (norm / delta) * 1f );
        }
    }

    private void Start()
    {
        if (animator != null)
        {

            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == "MotionSpeed")
                {
                    motionSpeedEnebled = true;
                }

                if (parameter.name == "Speed")
                {
                    speedEnebled = true;
                }
            }

            if (motionSpeedEnebled)
            {
                animator.SetFloat("MotionSpeed", 1.0f);
            }
        }

        beforePosition = transform.position;
    }

    private void Update()
    {
        var delta = Time.deltaTime;
        var position = transform.position;
        var direction = (position - beforePosition);
        transform.LookAt(direction.normalized + position, Vector3.up);
        UpdateAnimation(direction, delta);
        beforePosition = transform.position;
    }
}
