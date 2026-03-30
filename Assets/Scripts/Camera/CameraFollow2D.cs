using UnityEngine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool useBounds = true; // 【新增】边界限制开关，调试时可以关掉
    [SerializeField] private CameraBounds2D cameraBounds;
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Camera _cameraComponent;
    private Vector3 _velocity;

    private void Awake()
    {
        _cameraComponent = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        // 自动寻找主角
        if (target == null)
        {
            var player = FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                // 如果当前场景真没有主角，就不执行跟随逻辑
                return; 
            }
        }

        var desiredPosition = target.position + offset;

        // 如果启用了边界限制，才进行 Clamp 计算
        if (useBounds)
        {
            if (cameraBounds == null)
            {
                cameraBounds = FindObjectOfType<CameraBounds2D>();
            }

            if (cameraBounds != null)
            {
                desiredPosition = cameraBounds.ClampPosition(_cameraComponent, desiredPosition);
            }
        }

        // 平滑移动相机
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime);
    }
}