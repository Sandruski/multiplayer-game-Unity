using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrigger : MonoBehaviour
{
    BombController bombController;

    void Awake()
    {
        bombController = GetComponentInParent<BombController>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Physics2D.IgnoreCollision(bombController.collider2D, collision, false);
    }
}
