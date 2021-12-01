using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Critter : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameEvents.Instance.ScoreIncreased(42);
            GameEvents.Instance.CritterTriggerEntered();
            Destroy(this.gameObject);
        }
    }
}
