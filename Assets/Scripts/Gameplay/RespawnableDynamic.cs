using UnityEngine;

[DisallowMultipleComponent]
public class RespawnableDynamic : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D attachedBody;

    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;
    private Vector3 _spawnScale;
    private Vector2 _spawnVelocity;
    private float _spawnAngularVelocity;

    protected virtual void Awake()
    {
        CacheComponents();
        CaptureRespawnState();
        RegisterWithRespawnManager();
    }

    protected virtual void Start()
    {
        RegisterWithRespawnManager();
    }

    private void RegisterWithRespawnManager()
    {
        var respawnManager = FindObjectOfType<RespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.RegisterDynamic(this);
        }
    }

    protected virtual void OnDestroy()
    {
        var respawnManager = FindObjectOfType<RespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.UnregisterDynamic(this);
        }
    }

    public virtual void CaptureRespawnState()
    {
        CacheComponents();

        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;
        _spawnScale = transform.localScale;

        if (attachedBody != null)
        {
            _spawnVelocity = attachedBody.velocity;
            _spawnAngularVelocity = attachedBody.angularVelocity;
        }
    }

    public virtual void RestoreRespawnState()
    {
        CacheComponents();

        transform.position = _spawnPosition;
        transform.rotation = _spawnRotation;
        transform.localScale = _spawnScale;

        if (attachedBody != null)
        {
            attachedBody.position = _spawnPosition;
            attachedBody.rotation = _spawnRotation.eulerAngles.z;
            attachedBody.velocity = _spawnVelocity;
            attachedBody.angularVelocity = _spawnAngularVelocity;
            attachedBody.Sleep();
        }
    }

    protected void CacheComponents()
    {
        if (attachedBody == null)
        {
            attachedBody = GetComponent<Rigidbody2D>();
        }
    }
}
