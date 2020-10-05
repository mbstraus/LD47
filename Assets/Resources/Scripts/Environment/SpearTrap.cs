using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearTrap : MonoBehaviour
{
    public int Damage = 6;
    public Transform RaycastPoint;
    public bool HasTriggered = false;
    public bool IsActive = false;
    public Sprite InactiveSprite;
    public Sprite ActiveSprite;
    public SpriteRenderer SpriteRenderer;

    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(BeatTick);
    }

    private void OnDestroy() {
        MusicManager.Instance.UnregisterBeatEvent(BeatTick);
    }

    public void BeatTick() {
        if (!IsActive && !HasTriggered) {
            RaycastHit2D raycast = Physics2D.Raycast(RaycastPoint.position, Vector2.zero, 1f, LayerMask.GetMask("player"));
            if (raycast.transform != null) {
                SpriteRenderer.sprite = ActiveSprite;
                PlayerMovement player = raycast.transform.GetComponent<PlayerMovement>();
                player.TakeDamage(Damage);
                HasTriggered = true;
                IsActive = true;
            }
        } else if (IsActive) {
            SpriteRenderer.sprite = InactiveSprite;
            IsActive = false;
        }
    }
}
