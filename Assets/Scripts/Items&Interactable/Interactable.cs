using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float radius = 3f;   //how close the player needs to be to interact

    bool hasInteracted = false;
    public bool canInteract = false;

    private void Awake()
    {
        CircleCollider2D col = gameObject.AddComponent(typeof(CircleCollider2D)) as CircleCollider2D;
        col.isTrigger = true;

        //multipy by local scale so the radius (of circle collider) is not effected by the scale of the game object
        col.radius = radius * (1/col.transform.localScale.x);
    }

    public virtual void Interact()
    {
        Debug.Log("Interact");
        hasInteracted = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" &&  other.GetComponent<PlayerController>() == null)
        {
            EnteredArea(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && other.GetComponent<PlayerController>() == null)
        {
                LeftArea(other.gameObject);
        }
    }

    //Player has entered the area
    public virtual void EnteredArea(GameObject playerObj)
    {
        Debug.Log("Entered Area");
        canInteract = true;
    }

    //Player has left the area
    public virtual void LeftArea(GameObject playerObj)
    {
        Debug.Log("left area");
        canInteract = false;
    }

    private void Update()
    {
        //add !hasInteracted && to the if statement to only allow one interaction;
        if (canInteract &&  Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    //reset interactable if needed
    public void Reset()
    {
        hasInteracted = false;
    }

}
