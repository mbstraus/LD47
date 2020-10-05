using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int HealthAmount;

    public int CollectHealth() {
        Destroy(gameObject);
        return HealthAmount;
    }
}
