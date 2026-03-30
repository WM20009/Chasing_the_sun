using UnityEngine;

public class PlayerAnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerState _lastState;
    private bool _hasAppliedState;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void Apply(PlayerState state, int facingDirection)
    {
        CacheReferences();
        if (animator == null || spriteRenderer == null)
        {
            return;
        }

        if (!_hasAppliedState || state != _lastState)
        {
            animator.Play(state.ToString(), 0, 0f);
            _lastState = state;
            _hasAppliedState = true;
        }

        spriteRenderer.flipX = state != PlayerState.PushLeft && facingDirection < 0;
    }

    public void ForceState(PlayerState state, int facingDirection)
    {
        _hasAppliedState = false;
        Apply(state, facingDirection);
    }

    public void ResetVisualState()
    {
        _hasAppliedState = false;
    }

    private void CacheReferences()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
}
