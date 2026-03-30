using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class LadderZone : MonoBehaviour
{
    [SerializeField] private Collider2D ladderCollider;

    public float AttachX
    {
        get
        {
            CacheCollider();
            return ladderCollider.bounds.center.x;
        }
    }

    public float TopY
    {
        get
        {
            CacheCollider();
            return ladderCollider.bounds.max.y;
        }
    }

    public float BottomY
    {
        get
        {
            CacheCollider();
            return ladderCollider.bounds.min.y;
        }
    }

    private void Reset()
    {
        CacheCollider();
        if (ladderCollider != null)
        {
            ladderCollider.isTrigger = true;
        }

        gameObject.layer = LayerMask.NameToLayer(ChaseTheSunProjectSettings.ClimbableLayer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.NotifyEnterLadder(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.NotifyExitLadder(this);
        }
    }

    public float GetOverlapScore(Bounds playerBounds)
    {
        CacheCollider();
        var ladderBounds = ladderCollider.bounds;
        var verticalOverlap = Mathf.Max(0f, Mathf.Min(ladderBounds.max.y, playerBounds.max.y) - Mathf.Max(ladderBounds.min.y, playerBounds.min.y));
        return verticalOverlap / Mathf.Max(0.0001f, playerBounds.size.y);
    }

    public bool Contains(Bounds playerBounds)
    {
        CacheCollider();
        return ladderCollider.bounds.Intersects(playerBounds);
    }

    private void CacheCollider()
    {
        if (ladderCollider == null)
        {
            ladderCollider = GetComponent<Collider2D>();
        }
    }
}
