using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bomb : MonoBehaviour
{
    public int BeatsToExplode = 5;
    public float CurrentScale = 0.5f;
    public float TargetScale = 1f;
    public int Damage = 2;
    public bool IsExploding = false;
    public SpriteRenderer SpriteRenderer;
    public SpriteRenderer ExplosionRenderer;
    public AudioSource AudioSource;
    public AudioClip ExplosionSound;

    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(BeatTick);
    }

    private void OnDestroy() {
        MusicManager.Instance.UnregisterBeatEvent(BeatTick);
    }

    public void BeatTick() {
        if (IsExploding) {
            return;
        }
        // Need to grow the 0.5 scale for BeatsToExplode, so this needs to calculate that increment and increase the scale with each beat.
        float scaleFactor = 0.5f / BeatsToExplode;
        CurrentScale += scaleFactor;
        if (CurrentScale >= TargetScale) {
            Explode();
            return;
        }
        SpriteRenderer.transform.DOScale(CurrentScale, 0.3f);
    }

    public void Explode() {
        IsExploding = true;
        SpriteRenderer.gameObject.SetActive(false);
        ExplosionRenderer.gameObject.SetActive(true);
        AudioSource.PlayOneShot(ExplosionSound);
        ExplosionRenderer.transform.DORotate(new Vector3(0f, 0f, 180f), 0.3f).OnComplete(() => Destroy(gameObject));

        // This is the absolute worst way to do this, but whatever.
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(2f, 2f), 0f);
        foreach (Collider2D collider in colliders) {
            PlayerMovement playerMovement = collider.GetComponent<PlayerMovement>();
            if (playerMovement != null) {
                // Found a player... muahahaha.
                Debug.Log("Explosion damaged player.");
                playerMovement.TakeDamage(Damage);
                continue;
            }
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null) {
                // Found an enemy... kaboom.
                Debug.Log("Explosion damaged enemy.");
                enemy.TakeDamage(Damage);
                continue;
            }
            BreakableWall wall = collider.GetComponent<BreakableWall>();
            if (wall != null) {
                // Found a breakable wall... siyanara.
                Debug.Log("Explosion damaged wall.");
                wall.TakeExplosion();
                continue;
            }
        }
    }
}
