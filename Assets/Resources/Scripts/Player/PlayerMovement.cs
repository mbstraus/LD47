using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public bool IsMoving = false;
    private Vector2 MovementDirection = Vector2.zero;
    // Default to be facing left if no user input has been received.
    private Vector2 LastMoveDirection = Vector2.left;
    public Transform RaycastPoint;
    public float MoveDelay = 0.1f;
    public bool IsFalling = false;
    public bool IsAnimating = false;
    public bool HasMovedDuringBeat = false;
    public int BombCount = 0;
    public int Health = 4;

    public ParticleSystem LandingParticle;
    public Transform PlayerWeaponGraphic;
    public Bomb BombPrefab;
    public SpriteRenderer PlayerSpriteRenderer;
    public TextMeshProUGUI HealthValueUI;
    public TextMeshProUGUI BombCountUI;
    public AudioSource AudioSource;
    
    // Audio Clips
    public AudioClip TakeDamageAudioClip;
    public AudioClip AscendLadderAudioClip;
    public AudioClip LandAudioClip;
    public AudioClip DeathClip;
    public AudioClip PickupClip;

    // Since the animation is beat based, it's probably easier to just manually change the sprites over on each beat.
    public Sprite IdleFrame1;
    public Sprite IdleFrame2;
    public Sprite Falling;
    public Sprite DeadSprite;


    private void Start() {
        MusicManager.Instance.RegisterBeatEvent(HandleBeat);
        HealthValueUI.text = Health.ToString();
        BombCountUI.text = BombCount.ToString();
    }

    private void Update() {
        if (IsSquareOpen(new Vector2(0f, -1f), RaycastPoint.position, 1f) && !IsAnimating) {
            IsFalling = true;
        } else {
            if (IsFalling && !IsOnLadder(Vector2.zero, RaycastPoint.position, 1f)) {
                AudioSource.PlayOneShot(LandAudioClip);
                LandingParticle.Play();
            }
            IsFalling = false;
        }

        RaycastHit2D raycast = Physics2D.Raycast(RaycastPoint.position, Vector2.zero, 1f, LayerMask.GetMask("bomb bag", "health pickup"));
        if (raycast.transform != null) {
            BombBag bombBag = raycast.transform.GetComponent<BombBag>();
            if (bombBag != null) {
                AudioSource.PlayOneShot(PickupClip);
                BombCount += bombBag.CollectBombBag();
                BombCountUI.text = BombCount.ToString();
            }
            HealthPickup healthPickup = raycast.transform.GetComponent<HealthPickup>();
            if (healthPickup != null) {
                AudioSource.PlayOneShot(PickupClip);
                Health += healthPickup.CollectHealth();
                HealthValueUI.text = Health.ToString();
            }
        }

        if (Health <= 0) {
            StartCoroutine(DeathAnimation());

        }
    }

    public void HandleBeat() {
        if (IsSquareOpen(new Vector2(0f, -1f), RaycastPoint.position, 1f) && !IsOnLadder(Vector2.zero, RaycastPoint.position, 1f) && !HasMovedDuringBeat) {
            ExecuteDescendMove();
        }
        if (IsFalling) {
            PlayerSpriteRenderer.sprite = Falling;
        } else if (MusicManager.Instance.Beat % 2 == 0) {
            PlayerSpriteRenderer.sprite = IdleFrame1;
        } else {
            PlayerSpriteRenderer.sprite = IdleFrame2;
        }
        HasMovedDuringBeat = false;

        if (transform.position.y % 1f != 0) {
            // To correct a bug where the player sometimes gets stuck in the ground...
            transform.position = new Vector2(transform.position.x, Mathf.Ceil(transform.position.y));
        }
    }

    public void Move(InputAction.CallbackContext context) {
        // TODO: Update this to handle holding the key down.
        if (!IsMoving) {
            Vector2 movement = context.ReadValue<Vector2>();
            if (movement.x != 0 || movement.y != 0) {
                SetIsMoving(true);
                IsAnimating = true;
                MovementDirection = movement;
                ExecuteMove(MovementDirection);
            }
        }
    }

    public void PlantBomb(InputAction.CallbackContext context) {
        bool isOnBeat = MusicManager.Instance.IsInputAllowed();
        if (!isOnBeat) {
            Debug.Log("Movement blocked due to off beat.");
        } else {
            HasMovedDuringBeat = true;
            if (BombCount > 0) {
                Bomb bomb = Instantiate(BombPrefab);
                bomb.transform.position = transform.position;
                BombCount -= 1;
                BombCountUI.text = BombCount.ToString();
            }
        }
    }

    public void ExecuteMove(Vector2 direction) {
        if (direction.y > 0 && CanAscend()) {
            // TODO: Implement ladders
            ExecuteAscendMove();
        } else if (direction.y < 0 && CanDescend()) {
            ExecuteDescendMove();
        } else if (direction.x != 0 && CanMove(direction)) {
            ExecuteRegularMove(direction);
        } else {
            StartCoroutine(EnableMovement());
        }
    }
    private void ExecuteRegularMove(Vector2 direction) {
        Enemy enemy = null;
        Vector3 target;
        if (IsSquareOpen(direction, RaycastPoint.position, 1f)) {
            RaycastHit2D raycast = Physics2D.Raycast(new Vector2(RaycastPoint.position.x + direction.x, RaycastPoint.position.y), Vector2.down);
            target = new Vector2(transform.position.x + direction.x, transform.position.y);
            enemy = GetSquareEnemy(direction, RaycastPoint.position, 1f);
        } else {
            target = new Vector2(transform.position.x + direction.x, transform.position.y + 1f);
            enemy = GetSquareEnemy(direction, new Vector2(RaycastPoint.position.x, RaycastPoint.position.y + 1f), 1f);
        }
        if (enemy != null) {
            AttackEnemy(direction, enemy);
            StartCoroutine(EnableMovement());
        } else {
            PerformMove(target, 1f);
        }
        LastMoveDirection = direction;
    }

    private void ExecuteAscendMove() {
        Enemy enemy = null;
        Vector3 target;
        if (IsOnLadder(Vector2.up, RaycastPoint.position, 1f)) {
            enemy = GetSquareEnemy(Vector2.up, RaycastPoint.position, 1f);
            if (enemy != null) {
                AttackEnemy(Vector2.up, enemy);
            } else {
                transform.DOMoveY(transform.position.y + 1f, MoveDelay).OnComplete(() => StartCoroutine(EnableMovement()));
            }
            AudioSource.PlayOneShot(AscendLadderAudioClip);
            return;
        }
        DoorExit door = GetSquareDoor();
        if (door != null) {
            door.LoadScene();
            return;
        }
        enemy = GetSquareEnemy(LastMoveDirection, RaycastPoint.position, 1f);
        if (enemy != null) {
            // We are adjacent to an enemy, so treat the jump as a regular move action.
            ExecuteRegularMove(new Vector2(LastMoveDirection.x, 0f));
            return;
        }
        enemy = GetSquareEnemy(LastMoveDirection, RaycastPoint.position, 2f);
        if (enemy != null) {
            // An enemy is at our target jump location, so we will attack the enemy and land one space ahead.
            AttackEnemy(LastMoveDirection, enemy);
            target = new Vector3(transform.position.x + LastMoveDirection.x, transform.position.y, 0f);
            PerformMove(target, 1f);
            return;
        }
        bool isFarSquareOpen = IsSquareOpen(LastMoveDirection, RaycastPoint.position, 2f);
        bool isNearSquareOpen = IsSquareOpen(LastMoveDirection, RaycastPoint.position, 1f);
        Vector2 raycastTarget;
        float moveTargetPositionX;
        if (isFarSquareOpen) {
            raycastTarget = new Vector2(RaycastPoint.position.x + (LastMoveDirection.x * 2), RaycastPoint.position.y);
            moveTargetPositionX = transform.position.x + (LastMoveDirection.x * 2);
        } else {
            raycastTarget = new Vector2(RaycastPoint.position.x + LastMoveDirection.x, RaycastPoint.position.y);
            moveTargetPositionX = transform.position.x + LastMoveDirection.x;
        }
        // The jump target is open, so do a two space move.
        target = new Vector3(moveTargetPositionX, transform.position.y, 0f);
        PerformMove(target, 1f);
    }

    private void ExecuteDescendMove() {
        transform.DOMoveY(transform.position.y - 1f, MoveDelay).OnComplete(() => StartCoroutine(EnableMovement()));
    }

    public bool CanMove(Vector2 direction) {
        // A "move" is a left or right move to a square at the same level or a jump up one position.  If the square being moved to doesn't have
        // a tile underneath it, this will also result in the player falling down after the move, but the move itself goes to the empty space.
        // Triggered when the Left or Right key is pressed / held.  i.e. a, d, arrow left, arrow right, or gamepad left or right.
        bool isSquareOpen = IsSquareOpen(direction, RaycastPoint.position, 1f)
            || IsSquareOpen(direction, new Vector2(RaycastPoint.position.x, RaycastPoint.position.y + 1f), 1f);
        bool isOnBeat = MusicManager.Instance.IsInputAllowed();
        if (!isOnBeat) {
            return false;
        }
        if (!isSquareOpen) {
            Debug.Log("Movement blocked due to no valid movement square.");
            return false;
        }
        if (IsSquareOpen(Vector2.down, RaycastPoint.position, 1f) && !IsOnLadder(Vector2.zero, RaycastPoint.position, 1f)) {
            Debug.Log("Movement blocked due to empty block below player.");
            return false;
        }
        return true;
    }

    public bool CanAscend() {
        // Ascending can either be a jump forward, or ascending a ladder, if one is available.  Ascending a ladder takes precedence.  Jumping
        // will allow jumping one tile gaps, and will be done in whatever direction the player last moved in.  Jumping works the same as a regular
        // move, except they go two spaces rather than just one, and it takes two beats to perform the action. Jumping also must be to a square at
        // the same level, so can't raise up a level.

        bool isFarSquareOpen = IsSquareOpen(LastMoveDirection, RaycastPoint.position, 2f);
        bool isNearSquareOpen = IsSquareOpen(LastMoveDirection, RaycastPoint.position, 1f);
        bool isOnBeat = MusicManager.Instance.IsInputAllowed();
        if (!isOnBeat) {
            return false;
        }
        if (IsOnLadder(Vector2.up, RaycastPoint.position, 1f)) {
            return true;
        }
        if (GetSquareDoor() != null) {
            return true;
        }
        if (!isFarSquareOpen && !isNearSquareOpen) {
            Debug.Log("Movement blocked due to no valid movement square.");
            return false;
        }
        return true;
    }

    public bool CanDescend() {
        bool isOnBeat = MusicManager.Instance.IsInputAllowed();
        if (!isOnBeat) {
            return false;
        }
        if (IsOnLadder(Vector2.zero, RaycastPoint.position, 1f) && IsSquareOpen(Vector2.down, RaycastPoint.position, 1f)) {
            return true;
        }
        return false;
    }

    void SetIsMoving(bool value) {
        IsMoving = value;
    }

    private bool IsSquareOpen(Vector2 castDirection, Vector2 castPoint, float distance) {
        RaycastHit2D raycast = Physics2D.Raycast(castPoint, castDirection, distance, LayerMask.GetMask("tile", "breakable wall"));
        if (raycast.transform == null) {
            return true;
        }
        return false;
    }

    private Enemy GetSquareEnemy(Vector2 castDirection, Vector2 castPoint, float distance) {
        RaycastHit2D raycast = Physics2D.Raycast(castPoint, castDirection, distance, LayerMask.GetMask("enemy"));
        if (raycast.transform != null) {
            return raycast.transform.GetComponent<Enemy>();
        }
        return null;
    }

    private DoorExit GetSquareDoor() {
        RaycastHit2D raycast = Physics2D.Raycast(RaycastPoint.position, Vector2.zero, 1f, LayerMask.GetMask("exit"));
        if (raycast.transform != null) {
            return raycast.transform.GetComponent<DoorExit>();
        }
        return null;
    }

    private bool IsOnLadder(Vector2 castDirection, Vector2 castPoint, float distance) {
        RaycastHit2D raycast = Physics2D.Raycast(castPoint, castDirection, distance, LayerMask.GetMask("ladder"));
        if (raycast.transform != null) {
            return true;
        }
        return false;
    }

    public void PerformMove(Vector2 target, float jumpPower) {
        HasMovedDuringBeat = true;
        IsAnimating = true;
        transform.DOJump(target, jumpPower, 1, MoveDelay).OnComplete(() => StartCoroutine(EnableMovement()));
    }

    public void AttackEnemy(Vector2 attackDirection, Enemy enemy) {
        Quaternion weaponRotation = Quaternion.identity;
        if (attackDirection.x < 0) {
            // Attacking left
            weaponRotation = Quaternion.Euler(0f, 0f, 0f);
        } else if (attackDirection.x > 0) {
            // Attacking right
            weaponRotation = Quaternion.Euler(0f, 0f, 180f);
        } else if (attackDirection.y < 0) {
            // Attacking below
            weaponRotation = Quaternion.Euler(0f, 0f, 270f);
        } else if (attackDirection.y > 0) {
            // Attacking up
            weaponRotation = Quaternion.Euler(0f, 0f, 90f);
        }
        PlayerWeaponGraphic.rotation = weaponRotation;
        PlayerWeaponGraphic.gameObject.SetActive(true);
        StartCoroutine(DisableWeaponGraphic());

        enemy.TakeDamage(1);
    }

    IEnumerator EnableMovement() {
        yield return new WaitForSeconds(MoveDelay);
        SetIsMoving(false);
        IsAnimating = false;
    }

    IEnumerator DisableWeaponGraphic() {
        yield return new WaitForSeconds(MoveDelay);
        PlayerWeaponGraphic.gameObject.SetActive(false);
    }

    IEnumerator DeathAnimation() {
        PlayerSpriteRenderer.sprite = DeadSprite;
        MusicManager.Instance.SendingBeatEvents = false;
        AudioSource.PlayOneShot(DeathClip);
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Scenes/LevelSelect");
    }

    public void TakeDamage(int damage) {
        Health -= damage;
        HealthValueUI.text = Health.ToString();
        AudioSource.PlayOneShot(TakeDamageAudioClip);
    }
}
