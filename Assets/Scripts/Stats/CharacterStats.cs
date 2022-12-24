using System.Collections;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //Note: this might be better if it is removed. Or remove the other HealthSystem scripts
    public int maxHealth = 100;
    public int currentHealth { get; private set; }

    public Stat dmg;
    public Stat armor;

    public float colorTimer = 0f;
    public float dmgColorTime = .1f;

    public bool isRed = false;


    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {


        damage = damage /(1 + armor.GetValue);

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //player dies this might use other system as well
            Die();
        }

        //flicker red
        ColorRed();

        Debug.Log(transform.name + " takes " + damage + " damage.");
    }

    public virtual void ColorWhite()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
        isRed = false;
    }

    public virtual void ColorRed()
    {
        isRed = true;
        colorTimer = Time.time + dmgColorTime;
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void Update()
    {
        //if timer up color white
        if (isRed && colorTimer < Time.time)
        {
            ColorWhite();
        }
    }

    public virtual void Die()
    {
        //die in some way
        //this method is meant to be overwritten
        Debug.Log(transform.name + " has died.");
    }

    //Heal the player imediately for some amount of health
    public void Heal(int healAmnt)
    {
        //if healing would result in health greater than current health, just heal to max health
        if ((currentHealth + healAmnt) >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += healAmnt;
        }
    }

    //Heal the character over time for total amount of health 
    public void Heal(int healAmnt, int time)
    {
        int ogHealth = currentHealth;
        //divide health by the time to get a unit health for ever second
        int healthPerTick = healAmnt / time;

        StopAllCoroutines();
        //heal over time
        for (int i = 0; i < time; i++)
        {
            StartCoroutine("Tick", healthPerTick);
        }
    }

    private IEnumerator Tick(int healAmnt)
    {
        //if healing would result in health greater than current health, just heal to max health
        if ((currentHealth + healAmnt) >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += healAmnt;
        }

        //NOTE: this only waits for one second on each tick because the tick time is determined above
        yield return new WaitForSeconds(1f);
    }
}
