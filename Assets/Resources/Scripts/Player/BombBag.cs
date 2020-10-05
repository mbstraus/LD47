using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBag : MonoBehaviour
{
    public int BombCount = 1;

    public int CollectBombBag() {
        Destroy(gameObject);
        return BombCount;
    }
}
