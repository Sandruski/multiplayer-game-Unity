using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingBrickController : MonoBehaviour
{
    [HideInInspector]
    public bool isAlive;

    Animator animator;

    void Awake()
    {
        isAlive = false;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isAlive)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                Despawn();
            }
        }
    }

    public void Spawn(Vector3 position)
    {
        transform.position = position;

        isAlive = true;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }
}
