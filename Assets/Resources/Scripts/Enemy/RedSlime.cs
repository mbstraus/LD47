using DG.Tweening;
using UnityEngine;

public class RedSlime : EnemyAI
{
    public Sprite IdleFrame1;
    public Sprite IdleFrame2;
    public SpriteRenderer SpriteRenderer;
    public Vector2 CurrentDirection;
    public Transform RaycastPoint;
    public int Damage;

    public override void HandleBeat() {
        if (MusicManager.Instance.Beat % 2 == 0) {
            SpriteRenderer.sprite = IdleFrame1;
        } else {
            SpriteRenderer.sprite = IdleFrame2;
        }

        if (MusicManager.Instance.Beat % 2 == 0) {
            Vector2 target = new Vector2(transform.position.x + CurrentDirection.x, transform.position.y);
            transform.DOJump(target, 1f, 1, 0.1f);
        } else {
            Vector2 raycastPoint = new Vector2(RaycastPoint.position.x + CurrentDirection.x, RaycastPoint.position.y);
            RaycastHit2D raycast = Physics2D.Raycast(raycastPoint, Vector2.zero, 1f, LayerMask.GetMask("tile", "breakable wall", "enemy", "player"));
            if (raycast.transform != null) {
                PlayerMovement player = raycast.transform.GetComponent<PlayerMovement>();
                if (player != null) {
                    player.TakeDamage(Damage);
                }
                CurrentDirection = new Vector2(-CurrentDirection.x, CurrentDirection.y);
            }
        }
    }
}
