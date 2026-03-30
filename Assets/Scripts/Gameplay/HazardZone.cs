using UnityEngine;

[DisallowMultipleComponent]
public class HazardZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other.GetComponentInParent<PlayerController2D>());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.collider.GetComponentInParent<PlayerController2D>());
    }

    private void TryKill(PlayerController2D player)
    {
        if (player == null) return;

        var respawnManager = RespawnManager.Instance;
        if (respawnManager != null)
        {
            respawnManager.KillPlayer(player);
        }
        else 
        {
            // 兜底方案：万一找不到管理器，强制角色死亡重置
            player.HandleDeathStarted();
            player.RespawnAt(player.InitialSpawnPosition);
            player.HandleDeathFinished();
        }
    }
}
