using DG.Tweening;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Vector2 Direction;
    public int Damage = 2;
    public Transform RaycastPoint;

    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(BeatTick);
    }

    private void OnDestroy() {
        MusicManager.Instance.UnregisterBeatEvent(BeatTick);
    }

    public void BeatTick() {

        transform.DOMoveX(transform.position.x + Direction.x, 0.1f).OnComplete(() => MoveComplete());
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