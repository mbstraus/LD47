using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public int SightDistance = 6;
    public Transform RaycastPoint;
    public Vector2 Direction;
    public bool HasTrapTriggered = false;
    public int TrapDamage = 2;

    public AudioSource ArrowTrapAudioSource;
    public AudioClip FireClip;
    public Arrow ArrowPrefab;

    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(BeatTick);
    }

    private void OnDestroy() {
        MusicManager.Instance.UnregisterBeatEvent(BeatTick);
    }

    public void BeatTick() {
        if (HasTrapTriggered) {
            return;
        }
        RaycastHit2D raycast = Physics2D.Raycast(RaycastPoint.position, Direction, SightDistance, LayerMask.GetMask("player", "enemy"));
        if (raycast.transform != null) {
            Arrow arrow = Instantiate(ArrowPrefab);
            arrow.transform.position = transform.position;
            arrow.Damage = TrapDamage;
            arrow.Direction = Direction;
            HasTrapTriggered = true;
            ArrowTrapAudioSource.PlayOneShot(FireClip);
        }
    }
}
