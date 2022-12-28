using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    public int lives = 3;   //default number of lives to three

    [HideInInspector]
    public Stat torchBurn;  //rate at which the torch burns (this is always one and should not be modified) instead modify torch modifiers
    [Range(0f, 500f)]
    public float torchLife = 100f;
    private float burnTick;

    public PolygonCollider2D attackHitBox { private set; get; }

    public HashSet<GameObject> hitList { private set; get; }

    public Weapon handsWeapon;

    EquipmentManager equipmentManager;

    GameObject playerObj;
    PlayerController playerCont;

    #region Singleton
    public static PlayerStats instance;
    private void Awake()
    {

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("PlayerStats instance already exists, but you are trying to create another one");
        }

        instance.Heal(maxHealth);
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {

        hitList = new HashSet<GameObject>();
        EquipmentManager.instance.onEquipmentChanged += OnEquipmentChanged;
        if(playerObj == null)
        {
            //find player
            playerObj = FindObjectOfType<PlayerController>().playerObject;
            playerCont = playerObj.GetComponentInParent<PlayerController>();
        }

        //start game with hands 
        //TODO: should check for other default item maybe
        //TODO: this seems very inneficient
        while(equipmentManager == null)
        {
            equipmentManager = EquipmentManager.instance;
        }
        equipmentManager.Equip(handsWeapon);
    }

    //set lives
    public void SetLives(int value)
    {
        lives = value;
    }

    //add life to the player
    public void AddLife()
    {
        lives++;
    }


    //remove a life from the player and return true if the player has no more lives
    public bool RemoveLife()
    {
        lives--;
        if (lives <= 0)
        {
            lives = 0;
            return true;
        }
        return false;
    }

    //TODO: check that swapping weapons doesn't make this behave unpredictably
    //maintain items within collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hitList.Contains(collision.gameObject))
        {
            hitList.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (hitList.Contains(collision.gameObject))
        {
            hitList.Remove(collision.gameObject);
        }
    }

    //change stats when equipment is changed
    void OnEquipmentChanged(Equipment newItem, Equipment oldItem)
    {
        if (newItem != null && newItem.equipmentSlot == EquipmentSlot.Weapon)
        {
            if (oldItem == null || newItem.name != oldItem.name) {
                attackHitBox = ((Weapon)newItem).hitBox.GetComponent<PolygonCollider2D>();
  
                playerObj.GetComponentInParent<PolygonCollider2D>().points = attackHitBox.points;
                playerCont.AttackSpeed = ((Weapon)newItem).attackSpeed;
                
            }
        }

        if (newItem != null)
        {
            armor.AddModifier(newItem.armorModifier);
            dmg.AddModifier(newItem.damageModifier);
            torchBurn.AddModifier(newItem.torchModifier);
        }

        if (oldItem != null)
        {
            armor.RemoveModifier(oldItem.armorModifier);
            dmg.RemoveModifier(oldItem.damageModifier); 
            torchBurn.RemoveModifier(oldItem.torchModifier);
        }

        if (equipmentManager.hasTorch)
        {
            StopCoroutine("Burn");  //probably redundant, but shouldn't matter
            StartCoroutine("Burn");
        }
        else
        {
            StopCoroutine("Burn");
        }

        //when all weapons are unequiped swap to hands (and not just unequiping hands)
        if(equipmentManager.curWeapon == null)
        {
            equipmentManager.Equip(handsWeapon);
        }

        //SetPlayerAnimations();
    }

    //TODO this needs to be updated to more dynamic system before adding more items/animations
    //void SetPlayerAnimations()
    //{
    //    playerObj.GetComponentInParent<PlayerController>().SetAnimations(curAnim);
    //}

    private IEnumerator Burn()
    {
        while (true)
        {
            burnTick =  (float)torchBurn.GetValue;
            Debug.Log($"torchlife ({torchLife}) - burnTick ({burnTick}) = {torchLife - burnTick}");
            torchLife -= burnTick;
            if(torchLife <= 0)
            {
                BurnOut();
                torchLife = 0;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public override void ColorRed()
    {
        isRed = true;
        colorTimer = Time.time + dmgColorTime;
        playerObj.GetComponent<SpriteRenderer>().color = Color.red;
        Debug.Log($"color {playerObj.name} red");
    }

    public override void ColorWhite()
    {
        isRed = false;
        playerObj.GetComponent<SpriteRenderer>().color = Color.white;
        Debug.Log("color white");
    }

    //TODO: This needs to be tested to see if the torch burn out, feels right. 
    private void BurnOut()
    {
        Debug.Log("torch burned out");
        playerCont.LightTorch(false);
    }


    public override void Die()
    {
        base.Die();

        //check if the player has any lives left
        //NOTE: Remove life removes a life and checks if that is that the last life
        if (RemoveLife())
        {
            GameController.instance.GameOver();
        }
        else
        {
            Respawn();
        }
    }

    //TODO: this can probably be more polished with bettter camera movement and such
    public void Respawn()
    {
        GameObject player = playerObj.transform.parent.gameObject;
        //hide player controller
        player.SetActive(false);

        //reset player position to start
        player.transform.position = GameController.instance.mapMaker.CenterNode.position;

        Heal(maxHealth);

        player.SetActive(true);

        Debug.Log("respawn player");
    }

    //Taking Damage that applies a force to the character, scaled using armor value
    public void DmgForce(Vector3 initialForce)
    {
        //remove player control while knocked back 
        PlayerController playerCont = playerObj.GetComponentInParent<PlayerController>();
        playerCont.InControl = false;

        //Knock player back
        playerObj.GetComponentInParent<Rigidbody2D>().velocity += (Vector2)initialForce/(1 + armor.GetValue);
    }

    public GameObject PlayerObj
    {
        get
        {
            if (playerObj == null)
            {
                //find player
                playerObj = GameObject.FindGameObjectWithTag("Player");

            }

            return playerObj;
        }
    }
}
