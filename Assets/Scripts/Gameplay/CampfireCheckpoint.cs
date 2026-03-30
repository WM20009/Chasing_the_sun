using UnityEngine;

[DisallowMultipleComponent]
public class CampfireCheckpoint : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 1.1f);

    private void Reset()
    {
        var collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.isTrigger = true;
        }

        gameObject.layer = LayerMask.NameToLayer(ChaseTheSunProjectSettings.CheckpointLayer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        var respawnManager = RespawnManager.Instance;
        if (respawnManager != null)
        {
            respawnManager.SetCheckpoint(this);
            Debug.Log($"【重生点已记录】当前篝火坐标: {transform.position}");
        }
    }

    public Vector3 GetSpawnPosition()
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }

        return transform.position + (Vector3)spawnOffset;
    }
}
