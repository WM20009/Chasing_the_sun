using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableBox : RespawnableDynamic
{
    [SerializeField] private float mass = 2f;
    [SerializeField] private float linearDrag = 4f;

    protected override void Awake()
    {
        base.Awake();

        if (attachedBody != null)
        {
            attachedBody.mass = mass;
            attachedBody.drag = linearDrag;
            attachedBody.angularDrag = 4f;
            attachedBody.freezeRotation = true;
            attachedBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            attachedBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Reset()
    {
        gameObject.layer = LayerMask.NameToLayer(ChaseTheSunProjectSettings.PushableLayer);

        var body = GetComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider2D = GetComponent<BoxCollider2D>();
        collider2D.isTrigger = false;
    }
}
