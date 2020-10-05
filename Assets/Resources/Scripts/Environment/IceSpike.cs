using UnityEngine;
using DG.Tweening;

public class IceSpike : MonoBehaviour
{
    public int Damage = 2;
    public Transform RaycastPoint;
    public bool IsFalling = false;

    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(BeatTick);
    }

    private void OnDestroy() {
        MusicManager.Instance.UnregisterBeatEvent(BeatTick);
    }

    public void BeatTick() {
        if (!IsFalling) {
            RaycastHit2D raycast = Physics2D.Raycast(RaycastPoint.position, Vector2.down, 20f, LayerMask.GetMask("player", "enemy"));
            if (raycast.transform != null) {
                IsFalling = true;
            }
        } else {
            transform.DOMoveY(transform.position.y - 1f, 0.1f).OnComplete(() => MoveComplete());
        }
    }

    public void MoveComplete() {
        RaycastHit2D raycast = Physics2D.Raycast(RaycastPoint.position, Vector2.zero, 1f, LayerMask.GetMask("player", "enemy", "breakable wall", "tile"));
        if (raycast.transform != null) {
            PlayerMovement player = raycast.transform.GetComponent<PlayerMovement>();
            if (player != null) {
                player.TakeDamage(Damage);
            }
            Enemy enemy = raycast.transform.GetComponent<Enemy>();
            if (enemy != null) {
                enemy.TakeDamage(Damage);
            }
            Destroy(gameObject);
        }
    }
}
