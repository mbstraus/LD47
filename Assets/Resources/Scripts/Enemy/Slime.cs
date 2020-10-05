using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : EnemyAI
{
    public Sprite IdleFrame1;
    public Sprite IdleFrame2;
    public SpriteRenderer SpriteRenderer;

    public override void HandleBeat() {
        if (MusicManager.Instance.Beat % 2 == 0) {
            SpriteRenderer.sprite = IdleFrame1;
        } else {
            SpriteRenderer.sprite = IdleFrame2;
        }
    }
}
