using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Pathfinding;
using UnityEngine.Animations;

public class Enemy_AI : Interactable
{
    public Animator animator;

    public Transform target;
    public float speed = 200f;
    public float nextWaypointDist = 3f;
    public float alphaLevel = .9f;
    public LayerMask layerMask;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    bool following = false;

    Seeker seeker;
    Rigidbody2D rb;

    GameObject playerObj;


    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    //start following the new target
    public void Follow(Transform newTarget)
    {
        //transition to walk animation
        animator.SetTrigger("StartMoving");

        following = true;
        target = newTarget;
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    //stop following target
    public void StopFollowing()
    {
        //transition to idle animation
        animator.SetTrigger("StopMoving");

        following = false;
        CancelInvoke("UpdatePath");
    }

    //update path to target
    private void UpdatePath()
    {
        //if path is not currently beign determined, find path to target
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    //Callback for path complete
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    //inherited method from interactable, called whenever player enters area
    public override void EnteredArea(GameObject playerObj)
    {
        base.EnteredArea(playerObj);

        this.playerObj = playerObj;

        //Start following the player if this enemy visable from torch aka. lit up by torch
        if (isLit())
        {
            Follow(playerObj.transform);
        }

    }

    //Checkif this enemy is visable from torch aka lit
    bool isLit()
    {
        Vector2 playerPos = playerObj.transform.position;

        //get perpendicular unit vector
        Vector2 t = playerObj.transform.parent.GetComponentInChildren<Light2D>().transform.position;
        Vector2 e = transform.position;
        Vector2 normalDir = new Vector2(-(e.y - t.y), e.x - t.x);
        normalDir = normalDir.normalized * (GetComponent<CircleCollider2D>().radius * transform.localScale.x);

        //Get the widest points of enemy collider
        Vector2 p1 = e + normalDir;
        Vector2 p2 = e - normalDir;

        bool p1_Hit = !Physics2D.Linecast(playerPos, p1, layerMask);
        bool p2_Hit = !Physics2D.Linecast(playerPos, p2, layerMask);

        if (p1_Hit)
        {
            Debug.DrawLine(t, p1, Color.red, 10f);
        }
        if (p2_Hit)
        {
            Debug.DrawLine(t, p2, Color.red, 10f);
        }

        return !Physics2D.Linecast(playerPos, p1, layerMask) || !Physics2D.Linecast(playerPos, p2, layerMask);

    }

    //inherited method from interactable, called whenever player leaves area
    public override void LeftArea(GameObject playerObj)
    {
        base.LeftArea(playerObj);

        //stop following the player
        StopFollowing();
    }

    //updating path variables and moving enemy
    void FixedUpdate()
    {

        if (canInteract && !following)
        {
            if (isLit())
            {
                Follow(playerObj.transform);
            }
        }

        if(path == null)
        {
            return;
        }

        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }


        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        Vector2 velocity = direction * speed;
        rb.velocity = velocity;

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDist)
        {
            currentWaypoint++;
        }
        else
        {
            //turn character to face direction of motion
            transform.up = direction;
        }

    }
}
