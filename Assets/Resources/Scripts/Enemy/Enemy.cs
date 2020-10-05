using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int Health;
    public EnemyAI EnemyAI;

    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(BeatTick);
    }

    private void OnDestroy() {
        MusicManager.Instance.UnregisterBeatEvent(BeatTick);
    }

    public void BeatTick() {
        EnemyAI.HandleBeat();
    }

    public void TakeDamage(int damage) {
        Health -= damage;
        if (Health <= 0) {
            Destroy(gameObject);
        }
    }
}
