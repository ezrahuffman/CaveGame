using UnityEngine;

public class BasicEnemyStats : CharacterStats
{
    public override void Die()
    {
        base.Die();

        Debug.Log("enemy has died");
        gameObject.SetActive(false);
    }

    public override void ColorRed()
    {
        isRed = true;
        colorTimer = Time.time + dmgColorTime;
        GetComponentInChildren<SpriteRenderer>().color = Color.red;
    }

    public override void ColorWhite()
    {
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
        isRed = false;
    }
}
