using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.Universal;
public class PlayerController : MonoBehaviour
{
    //Variables
    public bool relativeMovement;       //If true the players movement is relative to the direction the character is facing
    //public float movementSpeed;
    public float movementAccRate;       //Rate at which the player accellerates
    public float movementDeccRate;       //Rate at which the player slows down when not giving movement inputs
    public float maxMovementSpeed;      //Maximum speed the player can reach
    public float rotSmooth;
    public GameObject playerObject;
    public Light2D torch;

    private bool upKey = false;
    private bool downKey = false;
    private bool rightKey = false;
    private bool leftKey = false;

    [SerializeField]
    bool inControl = true;
    int frameCount;

    public Animator animator;

    private Vector2 velocity;

    private Rigidbody2D rb;

    HashSet<GameObject> hitList;

    bool moving = false;
    int curAnim = 0;

    EquipmentManager equipmentManager;
    PlayerStats playerStats;
    GameController gameController;

    [SerializeField]
    bool canAttack = true;
    [SerializeField]
    float attackDelay = 1f;
    [SerializeField]
    float attackSpeed = 1f;
    float attackTimer;
    

    private void Start()
    {
        attackDelay = 1 / attackSpeed;
        attackTimer = 0f;

        hitList = new HashSet<GameObject>();

        //initialize velocity
        velocity = new Vector2();

        rb = gameObject.GetComponent<Rigidbody2D>();

        equipmentManager = EquipmentManager.instance;
        playerStats = PlayerStats.instance;
        gameController = GameController.instance;

        EquipTorch(equipmentManager.hasTorch);
    }

    public bool InControl {
        set
        {
            if (!value)
            {
                frameCount = 5;
            }
            inControl = value;
        }

        get
        {
            return inControl;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > attackTimer)
        {
            canAttack = true;
        }

        if (Input.GetButtonDown("Fire1") && canAttack)
        {
            Attack();
        }

        if(!moving && rb.velocity.magnitude > .1f)
        {
            moving = true;
            animator.SetBool("Moving", moving);
        }
        else if(moving && rb.velocity.magnitude < .1f)
        {
            moving = false;
            animator.SetBool("Moving", moving);
        }

        //check input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            //moving up 
            upKey = true;
        }
        else
        {
            upKey = false;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            //moving left
            leftKey = true;
        }
        else
        {
            leftKey = false;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            //moving right
            rightKey = true;
        }
        else
        {
            rightKey = false;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            //moving down
            downKey = true;
        }
        else
        {
            downKey = false;
        }

    }

    //update loop for physics
    private void FixedUpdate()
    {

        if (!inControl) {
            frameCount--;
            if (frameCount == 0)
            {
                inControl = true;
            }
            return;
        }

        //velocity = 0;
        velocity.x = 0;
        velocity.y = 0;

        // Get the direction from the player to the mouse
        Vector2 lookDir = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position).normalized;

        //movement
        if (upKey)
        {
            if (relativeMovement)
            {
                velocity += lookDir;
            }
            else
            {
                //add velocity up
                velocity += (Vector2.up);// * movementSpeed);
            }
        }
        if (leftKey)
        {
            if (relativeMovement)
            {
                velocity += Vector2.Perpendicular(lookDir);
            }
            else
            {
                //add velocity left
                velocity += (Vector2.left);// * movementSpeed);
            }
        }
        if (rightKey)
        {
            if (relativeMovement)
            {
                velocity += Vector2.Perpendicular(lookDir) * -1;
            }
            else
            {
                //add right velocity
                velocity += (Vector2.right);// * movementSpeed);
            }
        }
        if (downKey)
        {
            if (relativeMovement)
            {
                velocity += lookDir * -1;
            }
            else
            {
                //add down velocity
                velocity += (Vector2.down);// * movementSpeed);
            }
        }

        Debug.Log($"Velocity: {rb.velocity} | Size: {rb.velocity.magnitude}");

        bool hasMovementInput = velocity.magnitude != 0;
        bool shouldStop = rb.velocity.magnitude <= movementDeccRate;

        //If the player is not giving input and shouldn't stop this frame, slow to a stop
        if (!hasMovementInput && !shouldStop)
        {
            velocity = rb.velocity.normalized * -1 * movementDeccRate;
        }
        else
        {
            velocity = velocity.normalized * movementAccRate;
        }

        //move character by adding input velocity to current velocity
        rb.velocity += velocity;

        if(rb.velocity.magnitude > maxMovementSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxMovementSpeed;
        }

        //Make sure the player stops
        if(shouldStop && !hasMovementInput)
        {
            rb.velocity = Vector2.zero;
        }
        
        //look toward mouse
        transform.up = lookDir;
        
    }


    //TODO: check that swapping weapons doesn't make this behave unpredictably
    //TODO: non enemy objects should maybe be hitable too. Like breakable objects
    //maintain items within collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !hitList.Contains(collision.gameObject))
        {
            hitList.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && hitList.Contains(collision.gameObject))
        {
            hitList.Remove(collision.gameObject);
        }
    }



    void Attack()
    {
        //set timer
        canAttack = false;
        attackTimer = Time.time + attackDelay;

        //play attack animation
        animator.SetTrigger("Attack");
        

        //if something is hit, damage it
        GameObject[] temp = new GameObject[hitList.Count];
        hitList.CopyTo(temp);
        foreach (var obj in temp)
        {
            //Hit enemy
            if (obj.CompareTag("Enemy"))
            {
       
                obj.GetComponent<CharacterStats>().TakeDamage(PlayerStats.instance.dmg.GetValue);
            }
        }
    }

    public void SetAnimations()
    {
        if (equipmentManager == null)
        {
            //Debug.LogError("Equipment Manager is Null!");
            equipmentManager = EquipmentManager.instance;
            playerStats = PlayerStats.instance;
        }
        bool hasHelmet = equipmentManager.currentEquipment[(int)EquipmentSlot.Head] != null;
        bool hasSword = equipmentManager.currentEquipment[(int)EquipmentSlot.Weapon].name != playerStats.handsWeapon.name;


        int newAnim = 0;
        if (!hasHelmet)
        {
            //no helmet
            if (!hasSword)
            {
                //no helmet, no sword
                //set zero for cur anim
            }
            else
            {
                //no helmet, has sword
                newAnim = 1;
            }
        }
        else
        {
            //has helmet
            if (!hasSword)
            {
                //has helmet, no sword
                newAnim = 2;
            }
            else
            {
                //helmet & sword
                newAnim = 3;
            }
        }

        if (curAnim != newAnim)
        {
            //swap animations
            animator.SetInteger("CurrAnimation", newAnim);
            animator.SetTrigger("Swap");

            curAnim = newAnim;
        }
    }

    public void EquipTorch(bool torchEquiped)
    {
        torch.enabled = torchEquiped;
        gameController.torchUI.SetActive(torchEquiped);

        if (torchEquiped)
        {
            LightTorch(playerStats.torchLife != 0);
        }
    }

    public void LightTorch(bool showUI)
    {
        torch.enabled = showUI;
        gameController.lightUI.SetActive(showUI);
    }

    public float AttackSpeed
    {
        get
        {
            return attackSpeed;
        }

        set
        {
            //TODO: check that this can't be abused by quickly swapping weapons
            //if it can, reset the timer here as well.
            attackDelay = 1 / value;

        }
    }
}
