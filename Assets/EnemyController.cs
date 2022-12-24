using UnityEngine;

public class EnemyController : MonoBehaviour
{
    PlayerStats playerStats;
    CharacterStats stats;
    public float forceMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<CharacterStats>();

        if (PlayerStats.instance != null)
        {
            playerStats = PlayerStats.instance;
        }
        else
        {
            Debug.LogError("no instance of PlayerStats found!!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //collide with player -> dmg player
        if(collision.collider.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        //dmg player
        playerStats.TakeDamage(stats.dmg.GetValue);

        //knock player back (just a little bit)
        Vector2 dir = (playerStats.PlayerObj.transform.position - transform.position);
        playerStats.DmgForce(dir.normalized * forceMultiplier);

    }
}
