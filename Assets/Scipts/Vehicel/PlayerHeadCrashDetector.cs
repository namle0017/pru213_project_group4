using UnityEngine;
using UnityEngine.U2D;

public class PlayerHeadCrashDetector : MonoBehaviour
{
    [SerializeField] private GameSession gameSession;
    [SerializeField] private bool debugLogCrash = true;

    private void Start()
    {
        CacheGameSession();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsTerrain(collision.collider))
        {
            return;
        }

        CacheGameSession();

        if (gameSession == null || gameSession.IsGameOver)
        {
            return;
        }

        if (debugLogCrash)
        {
            Debug.Log("PlayerHeadCrashDetector: Player_head cham terrain -> Game Over.");
        }

        gameSession.GameOver();
    }

    private void CacheGameSession()
    {
        if (gameSession == null)
        {
            gameSession = FindFirstObjectByType<GameSession>();
        }
    }

    private bool IsTerrain(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.GetComponent<SpriteShapeController>() != null)
        {
            return true;
        }

        if (other.GetComponentInParent<SpriteShapeController>() != null)
        {
            return true;
        }

        return other is EdgeCollider2D;
    }
}
