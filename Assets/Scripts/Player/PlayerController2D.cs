using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float pushSpeed = 2.5f;
    [SerializeField] private float climbSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 2.6f;
    [SerializeField] private float baseGravityScale = 4f;
    [SerializeField] private float fallGravityMultiplier = 2.3f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.8f;
    [SerializeField] private float maxFallSpeed = 14f;
    [SerializeField] private float ladderDetachHorizontalSpeed = 4f;

    [Header("Detection")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.68f, 0.10f);
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private Vector2 pushCheckSize = new Vector2(0.30f, 1.00f);
    [SerializeField] private Vector2 pushCheckOffset = new Vector2(0.55f, 0.85f);
    [SerializeField] private float ladderAttachThreshold = 0.35f;
    [SerializeField] private float apexVelocityThreshold = 0.25f;

    [Header("State Timing")]
    [SerializeField] private float jumpStartDuration = 0.14f;
    [SerializeField] private float landDuration = 0.08f;

    [Header("References")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private CapsuleCollider2D bodyCollider;
    [SerializeField] private PlayerAnimationDriver animationDriver;
    [SerializeField] private RespawnManager respawnManager;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask pushableLayers;

    private readonly List<LadderZone> _ladderZones = new List<LadderZone>();

    private Vector3 _initialSpawnPosition;
    private Quaternion _initialSpawnRotation;
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpPressed;
    private bool _jumpHeld;
    private bool _wasGrounded;
    private bool _isGrounded;
    private bool _isClimbing;
    private bool _isDead;
    private int _facingDirection = 1;
    private float _jumpStartTimer;
    private float _landTimer;
    private LadderZone _activeLadder;
    private PushableBox _currentPushTarget;

    public Vector3 InitialSpawnPosition => _initialSpawnPosition;

    private void Awake()
    {
        CacheReferences();
        ConfigureDefaults();

        _initialSpawnPosition = transform.position;
        _initialSpawnRotation = transform.rotation;

        if (respawnManager != null)
        {
            respawnManager.RegisterPlayer(this);
        }
    }

    private void Start()
    {
        if (respawnManager == null)
        {
            respawnManager = FindObjectOfType<RespawnManager>();
        }

        if (respawnManager != null)
        {
            respawnManager.RegisterPlayer(this);
        }
    }

    private void OnValidate()
    {
        CacheReferences();
        ConfigureDefaults();
    }

    private void Update()
    {
        if (_isDead)
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
            _jumpPressed = false;
            _jumpHeld = false;
            return;
        }

        _horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            _horizontalInput -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            _horizontalInput += 1f;
        }

        _verticalInput = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            _verticalInput += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _verticalInput -= 1f;
        }

        _jumpPressed |= Input.GetKeyDown(KeyCode.Space);
        _jumpHeld = Input.GetKey(KeyCode.Space);

        if (Mathf.Abs(_horizontalInput) > 0.01f && !_isClimbing)
        {
            _facingDirection = _horizontalInput > 0f ? 1 : -1;
        }

        if (_jumpStartTimer > 0f)
        {
            _jumpStartTimer -= Time.deltaTime;
        }

        if (_landTimer > 0f)
        {
            _landTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();

        if (_isDead)
        {
            body.velocity = Vector2.zero;
            ApplyAnimation(PlayerState.Dead);
            _wasGrounded = _isGrounded;
            return;
        }

        if (_isClimbing)
        {
            if (Mathf.Abs(_horizontalInput) > 0.01f)
            {
                DetachFromLadder(_horizontalInput * ladderDetachHorizontalSpeed);
            }
            else
            {
                HandleClimbing();
                ApplyAnimation(PlayerState.Climb);
                _wasGrounded = _isGrounded;
                _jumpPressed = false;
                return;
            }
        }

        TryAttachToLadder();
        if (_isClimbing)
        {
            HandleClimbing();
            ApplyAnimation(PlayerState.Climb);
            _wasGrounded = _isGrounded;
            _jumpPressed = false;
            return;
        }

        HandleMovement();
        HandleJump();
        ApplyGravityModifiers();
        ApplyAnimation(ResolveCurrentState());

        _wasGrounded = _isGrounded;
        _jumpPressed = false;
    }

    public void NotifyEnterLadder(LadderZone ladderZone)
    {
        if (ladderZone != null && !_ladderZones.Contains(ladderZone))
        {
            _ladderZones.Add(ladderZone);
        }
    }

    public void NotifyExitLadder(LadderZone ladderZone)
    {
        if (ladderZone == null)
        {
            return;
        }

        _ladderZones.Remove(ladderZone);
        if (_activeLadder == ladderZone && _isClimbing)
        {
            DetachFromLadder(0f);
        }
    }

    public void HandleDeathStarted()
    {
        _isDead = true;
        _isClimbing = false;
        _activeLadder = null;
        _currentPushTarget = null;
        body.velocity = Vector2.zero;
        body.gravityScale = baseGravityScale;
        body.angularVelocity = 0f;

        if (animationDriver != null)
        {
            animationDriver.ForceState(PlayerState.Dead, _facingDirection);
        }
    }

    public void HandleDeathFinished()
    {
        _isDead = false;
    }

    public void RespawnAt(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        transform.rotation = _initialSpawnRotation;

        body.position = worldPosition;
        body.rotation = _initialSpawnRotation.eulerAngles.z;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.gravityScale = baseGravityScale;

        _isDead = false;
        _isClimbing = false;
        _activeLadder = null;
        _currentPushTarget = null;
        _jumpPressed = false;
        _jumpHeld = false;
        _jumpStartTimer = 0f;
        _landTimer = 0f;
        _wasGrounded = false;
        _isGrounded = false;

        if (animationDriver != null)
        {
            animationDriver.ForceState(PlayerState.Idle, _facingDirection);
        }
    }

    private void HandleMovement()
    {
        _currentPushTarget = FindPushTarget(_horizontalInput);
        var isPushing = Mathf.Abs(_horizontalInput) > 0.01f && _isGrounded && _currentPushTarget != null;
        var speed = isPushing ? pushSpeed : moveSpeed;
        var velocity = body.velocity;
        velocity.x = _horizontalInput * speed;
        body.velocity = velocity;
    }

    private void HandleJump()
    {
        if (!_jumpPressed || !_isGrounded)
        {
            return;
        }

        var jumpVelocity = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * baseGravityScale * jumpHeight);
        var velocity = body.velocity;
        velocity.y = jumpVelocity;
        body.velocity = velocity;

        _isGrounded = false;
        _jumpStartTimer = jumpStartDuration;
        _landTimer = 0f;
    }

    private void HandleClimbing()
    {
        if (_activeLadder == null)
        {
            DetachFromLadder(0f);
            return;
        }

        // 强行锁死X轴（对齐梯子）
        var position = transform.position;
        position.x = _activeLadder.AttachX;
        transform.position = position;
        body.position = position;
        
        body.velocity = new Vector2(0f, _verticalInput * climbSpeed);
        body.gravityScale = 0f;

        // 【核心修改3】向上爬到顶部退出：直接让脚底(transform.position.y)与梯子顶部对齐，并保持静止
        if (_verticalInput > 0f && transform.position.y >= _activeLadder.TopY)
        {
            var topExit = transform.position;
            topExit.y = _activeLadder.TopY; 
            transform.position = topExit;
            body.position = topExit;
            
            DetachFromLadder(0f);
            body.velocity = Vector2.zero; // 强行设置静止状态
            return;
        }

        // 【核心修改4】向下爬到底部退出：接触地面或是到达梯子底部
        if (_verticalInput < 0f && (_isGrounded || transform.position.y <= _activeLadder.BottomY))
        {
            var bottomExit = transform.position;
            // 如果是因为到梯子底部悬空而退出，则对齐梯子底端；若是触地退出则保留当前高度
            if (!_isGrounded)
            {
                bottomExit.y = _activeLadder.BottomY;
            }
            transform.position = bottomExit;
            body.position = bottomExit;
            
            DetachFromLadder(0f);
            body.velocity = Vector2.zero; // 强行设置静止状态
            return;
        }
    }

    private void ApplyGravityModifiers()
    {
        if (_isClimbing)
        {
            body.gravityScale = 0f;
            return;
        }

        if (body.velocity.y < -0.01f)
        {
            body.gravityScale = baseGravityScale * fallGravityMultiplier;
        }
        else if (body.velocity.y > 0.01f && !_jumpHeld)
        {
            body.gravityScale = baseGravityScale * lowJumpGravityMultiplier;
        }
        else
        {
            body.gravityScale = baseGravityScale;
        }

        if (body.velocity.y < -maxFallSpeed)
        {
            body.velocity = new Vector2(body.velocity.x, -maxFallSpeed);
        }
    }

    private void TryAttachToLadder()
    {
        if (_ladderZones.Count == 0) return;
        
        // 【核心修改1】 必须有明确的W或S输入，且速度不能太小
        if (Mathf.Abs(_verticalInput) < 0.01f) return;

        LadderZone bestLadder = null;
        var bestScore = 0f;
        var playerBounds = bodyCollider.bounds;

        for (var i = _ladderZones.Count - 1; i >= 0; i--)
        {
            if (_ladderZones[i] == null)
            {
                _ladderZones.RemoveAt(i);
                continue;
            }

            var overlapScore = _ladderZones[i].GetOverlapScore(playerBounds);
            if (overlapScore > bestScore)
            {
                bestScore = overlapScore;
                bestLadder = _ladderZones[i];
            }
        }

        if (bestLadder == null || bestScore < ladderAttachThreshold) return;

        bool isPressingUp = _verticalInput > 0f;
        bool isPressingDown = _verticalInput < 0f;

        // 【核心修改2】防止在梯子正上方按W错误吸附，或在正下方按S错误吸附
        // transform.position.y 代表玩家脚底
        if (isPressingUp && transform.position.y >= bestLadder.TopY - 0.1f) return;
        if (isPressingDown && transform.position.y <= bestLadder.BottomY + 0.1f) return;

        _activeLadder = bestLadder;
        _isClimbing = true;
        _currentPushTarget = null;
        body.velocity = Vector2.zero;
        body.gravityScale = 0f;

        var snappedPosition = transform.position;
        snappedPosition.x = bestLadder.AttachX;
        transform.position = snappedPosition;
        body.position = snappedPosition;
    }

    private void DetachFromLadder(float horizontalVelocity)
    {
        _isClimbing = false;
        _activeLadder = null;
        body.gravityScale = baseGravityScale;
        body.velocity = new Vector2(horizontalVelocity, body.velocity.y);
    }

    private PushableBox FindPushTarget(float horizontalAxis)
    {
        if (Mathf.Abs(horizontalAxis) < 0.01f)
        {
            return null;
        }

        var direction = horizontalAxis > 0f ? 1f : -1f;
        var origin = (Vector2)transform.position + new Vector2(pushCheckOffset.x * direction, pushCheckOffset.y);
        var hit = Physics2D.OverlapBox(origin, pushCheckSize, 0f, pushableLayers);
        return hit != null ? hit.GetComponent<PushableBox>() : null;
    }

    private void UpdateGroundedState()
    {
        var center = (Vector2)transform.position + new Vector2(0f, (groundCheckSize.y * 0.5f) - groundCheckDistance);
        _isGrounded = Physics2D.OverlapBox(center, groundCheckSize, 0f, groundLayers);

        if (!_wasGrounded && _isGrounded)
        {
            _landTimer = landDuration;
        }
    }

    private PlayerState ResolveCurrentState()
    {
        if (_isDead)
        {
            return PlayerState.Dead;
        }

        if (_isClimbing)
        {
            return PlayerState.Climb;
        }

        if (_landTimer > 0f && _isGrounded)
        {
            return PlayerState.Land;
        }

        if (!_isGrounded)
        {
            if (_jumpStartTimer > 0f)
            {
                return PlayerState.JumpStart;
            }

            if (body.velocity.y > apexVelocityThreshold)
            {
                return PlayerState.JumpRise;
            }

            if (Mathf.Abs(body.velocity.y) <= apexVelocityThreshold)
            {
                return PlayerState.JumpApex;
            }

            return PlayerState.JumpFall;
        }

        if (_currentPushTarget != null && Mathf.Abs(_horizontalInput) > 0.01f)
        {
            return _horizontalInput > 0f ? PlayerState.PushRight : PlayerState.PushLeft;
        }

        if (Mathf.Abs(body.velocity.x) > 0.05f)
        {
            return PlayerState.Run;
        }

        return PlayerState.Idle;
    }

    private void ApplyAnimation(PlayerState state)
    {
        if (animationDriver != null)
        {
            animationDriver.Apply(state, _facingDirection);
        }
    }

    private void ConfigureDefaults()
    {
        if (groundLayers == 0)
        {
            groundLayers = ChaseTheSunProjectSettings.MaskFromNames(
                ChaseTheSunProjectSettings.GroundLayer,
                ChaseTheSunProjectSettings.PushableLayer);
        }

        if (pushableLayers == 0)
        {
            pushableLayers = ChaseTheSunProjectSettings.MaskFromNames(ChaseTheSunProjectSettings.PushableLayer);
        }
    }

    private void CacheReferences()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<CapsuleCollider2D>();
        }

        if (animationDriver == null)
        {
            animationDriver = GetComponent<PlayerAnimationDriver>();
        }

        if (respawnManager == null)
        {
            respawnManager = FindObjectOfType<RespawnManager>();
        }
    }

    private void Reset()
    {
        CacheReferences();
        ConfigureDefaults();

        gameObject.layer = LayerMask.NameToLayer(ChaseTheSunProjectSettings.PlayerLayer);

        if (body != null)
        {
            body.gravityScale = baseGravityScale;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (bodyCollider != null)
        {
            bodyCollider.direction = CapsuleDirection2D.Vertical;
            bodyCollider.offset = new Vector2(0f, 0.875f);
            bodyCollider.size = new Vector2(0.75f, 1.75f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 1f, 0.1f, 0.35f);
        var groundCenter = transform.position + new Vector3(0f, (groundCheckSize.y * 0.5f) - groundCheckDistance, 0f);
        Gizmos.DrawCube(groundCenter, groundCheckSize);

        Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.35f);
        var direction = _facingDirection >= 0 ? 1f : -1f;
        var pushCenter = transform.position + new Vector3(pushCheckOffset.x * direction, pushCheckOffset.y, 0f);
        Gizmos.DrawCube(pushCenter, pushCheckSize);
    }
}
