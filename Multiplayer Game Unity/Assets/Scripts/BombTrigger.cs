using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrigger : MonoBehaviour
{
    #region Private
    private BombController bombController;
    #endregion

    void Start()
    {
        bombController = GetComponentInParent<BombController>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Physics2D.IgnoreCollision(bombController.myCollider, collision, false);
    }
}
