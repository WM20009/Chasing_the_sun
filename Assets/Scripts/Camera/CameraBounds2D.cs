using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class CameraBounds2D : MonoBehaviour
{
    [Header("便捷设置")]
    [Tooltip("把你的场景背景图片(SpriteRenderer)拖到这里，然后右键组件选择'自动对齐边界到背景'")]
    [SerializeField] private SpriteRenderer backgroundReference;

    [HideInInspector]
    [SerializeField] private BoxCollider2D boundsCollider;

    private void Reset()
    {
        CacheCollider();
        boundsCollider.isTrigger = true;
        boundsCollider.size = new Vector2(24f, 12f);
    }

    // 这是一个非常实用的编辑器魔法，可以在脚本的右键菜单里多出一个选项
    [ContextMenu("自动对齐边界到背景")]
    public void FitToBackground()
    {
        CacheCollider();
        if (backgroundReference != null)
        {
            // 直接把这个节点的位置和碰撞体大小，严丝合缝地对齐到背景图上
            transform.position = backgroundReference.transform.position;
            boundsCollider.offset = Vector2.zero;
            boundsCollider.size = backgroundReference.bounds.size;
            Debug.Log("✅ 相机边界已成功对齐到背景图大小！不再穿帮啦。");
        }
        else
        {
            Debug.LogWarning("⚠️ 请先将你的背景图拖入 Background Reference 槽位中！");
        }
    }

    public Vector3 ClampPosition(Camera cameraComponent, Vector3 desiredPosition)
    {
        CacheCollider();

        if (cameraComponent == null || !cameraComponent.orthographic || boundsCollider == null)
        {
            return desiredPosition;
        }

        var bounds = boundsCollider.bounds;
        var verticalHalf = cameraComponent.orthographicSize;
        var horizontalHalf = verticalHalf * cameraComponent.aspect;

        // 防止相机视口比边界还大导致的锁死卡顿（上一回合修复的核心）
        if (bounds.size.x < horizontalHalf * 2f || bounds.size.y < verticalHalf * 2f)
        {
            return desiredPosition;
        }

        // 计算相机的运动极限坐标
        var minX = bounds.min.x + horizontalHalf;
        var maxX = bounds.max.x - horizontalHalf;
        var minY = bounds.min.y + verticalHalf;
        var maxY = bounds.max.y - verticalHalf;

        // 核心逻辑：将相机的目标坐标“钳制”在这个计算好的最大/最小范围内
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        return desiredPosition;
    }

    private void OnDrawGizmosSelected()
    {
        CacheCollider();
        if (boundsCollider == null) return;

        // 在 Scene 视图里画出一个半透明的青色框，方便你直观看到相机的活动范围
        Gizmos.color = new Color(0.1f, 0.8f, 0.8f, 0.35f);
        Gizmos.DrawCube(boundsCollider.bounds.center, boundsCollider.bounds.size);
        Gizmos.color = new Color(0.1f, 0.8f, 0.8f, 0.95f);
        Gizmos.DrawWireCube(boundsCollider.bounds.center, boundsCollider.bounds.size);
    }

    private void CacheCollider()
    {
        if (boundsCollider == null)
        {
            boundsCollider = GetComponent<BoxCollider2D>();
        }
    }
}