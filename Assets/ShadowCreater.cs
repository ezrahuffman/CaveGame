using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class ShadowCreater : MonoBehaviour
{
    HashSet<Collider2D> shapes;
    HashSet<Collider2D> newList;

    public UnityEngine.Rendering.Universal.Light2D torchLight;

    public LayerMask layerMask;
    public LayerMask testLayer;

    float boxWidth;

    public GameObject debugDot;
    HashSet<Collider2D> remainingShadows;

    List<GameObject> parents;
    List<GameObject> usedParents;
    List<GameObject> shadows;
    List<GameObject> usedShadows;

    public float cornerDelta = .1f;   //how much the corner check for isVisable should be moved


    Dictionary<UnityEngine.Rendering.Universal.ShadowCaster2D, Transform> parentDict;
    

    private void Awake()
    {
        parentDict = new Dictionary<UnityEngine.Rendering.Universal.ShadowCaster2D, Transform>();
        shadows = new List<GameObject>();
        usedShadows = new List<GameObject>();
        usedParents = new List<GameObject>();
        parents = new List<GameObject>();
        shapes = new HashSet<Collider2D>();
        remainingShadows = new HashSet<Collider2D>();
    }

    private void Start()
    {
        boxWidth = GameController.instance.WallWidth;
    }

    // Update is called once per frame
    void Update()
    {
        Light2D torch = FindObjectOfType<PlayerController>().torch;
        Vector3 torchPos = torch.transform.position;
        float r = torch.pointLightOuterRadius;
        newList = new HashSet<Collider2D>(Physics2D.OverlapCircleAll(torchPos, r, layerMask));  //get all colliders within radius of torch light

        //if shapes list has not changed, don't do anything
        if (newList == shapes)
        {
            return;
        }



        remainingShadows.Clear();

        


        //TODO: eventually there will be no need for testing if blocks are internal, because they will be removed when making the graph. 
        //HashSet<Vector2> verts = new HashSet<Vector2>();
        foreach (Collider2D collider in newList)
        {

            if (IsVisable(collider))
            {
                remainingShadows.Add(collider);
            }
        }


        while (remainingShadows.Count > 0)
        {
            //get next collider
            Collider2D shadow = remainingShadows.First();

            //Determine Shadow Parent
            GameObject groupParent;

            if (shadow.transform.parent.GetComponent<UnityEngine.Rendering.Universal.CompositeShadowCaster2D>() != null)
            {
                groupParent = shadow.transform.parent.gameObject;
            }
            else if (parents.Count <= 0)
            {
                //create parent object for shadows
                groupParent = new GameObject("ShadowParent");
                groupParent.AddComponent<UnityEngine.Rendering.Universal.CompositeShadowCaster2D>();

                usedParents.Add(groupParent);
            }
            else
            {
                groupParent = parents[0];
                parents.RemoveAt(0);
                usedParents.Add(groupParent);
            }


            //search for neighbors fo shadow
            FindNeighbors(shadow, groupParent);
        }

        //Clear old shapes
        Reset();

        shapes = newList;

    }

    void FindNeighbors(Collider2D shadow, GameObject parent)
    {
        remainingShadows.Remove(shadow);
        CreateShadow(shadow, parent);

        //check to see if this object has neighbors
        Collider2D[] colls = Physics2D.OverlapCircleAll(shadow.transform.position, (5 / 3) * boxWidth, layerMask);


        foreach(Collider2D coll in colls)
        {
            if (remainingShadows.Contains(coll))
            {
                FindNeighbors(coll, parent);
            }
        }
    }


    //Test if a block is internal or not (Not needed atm)

    bool IsVisable(Collider2D collider)
    {
        Vector2[] corners = BoxPoints(collider as BoxCollider2D);
        float radius = torchLight.pointLightOuterRadius + (boxWidth * 4);

        foreach (Vector2 corner in corners)
        {
            Vector2 torchPos = torchLight.transform.position;
            
            if (Physics2D.Raycast(torchPos, corner - torchPos, radius, layerMask).collider == collider)
            {
                //Check if behind wall by altering the direction fo the raycast slightly

                Vector2 target = corner + (Vector2.up * cornerDelta);
                bool upHit = Physics2D.Raycast(torchPos, target - torchPos, radius, layerMask).collider == collider;

                target = corner - (Vector2.up * cornerDelta);
                bool downHit = Physics2D.Raycast(torchPos, target - torchPos, radius, layerMask).collider == collider;
                    
                if (upHit || downHit)
                {
                    return true;
                }
            }
        }

       return false;
    }


    void CreateShadow(Collider2D coll, GameObject parent)
    {

        //Vector2[] points;

        //if (coll.GetType() == typeof(BoxCollider2D))
        //{
        //    Debug.Log("is box");
        //    points = BoxPoints((BoxCollider2D)coll);
        //}
        //else
        //{
        //    points = ((PolygonCollider2D)coll).points;
        //}
        UnityEngine.Rendering.Universal.ShadowCaster2D caster;
        if (coll.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>() == null)
        {

            caster = coll.gameObject.AddComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();

            //create new object to hold shadow and group using parent
            if (!parentDict.ContainsKey(caster))
            {
                parentDict.Add(caster, coll.transform.parent);
            }
            coll.transform.parent = parent.transform;
            caster.enabled = true;
            caster.selfShadows = false;
        } else if(shapes.Contains(coll))
        {
            caster = coll.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
            //create new object to hold shadow and group using parent
            if (!parentDict.ContainsKey(caster))
            {
                parentDict.Add(caster, coll.transform.parent);
            }
            coll.transform.parent = parent.transform;

            newList.Add(coll);
            shapes.Remove(coll);
            caster.enabled = true;
        }
        else
        {

            caster = coll.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
            //create new object to hold shadow and group using parent
            if (!parentDict.ContainsKey(caster))
            {
                parentDict.Add(caster, coll.transform.parent);
            }
            coll.transform.parent = parent.transform;

            newList.Add(coll);
            caster.enabled = true;
        }
    }


    //Reset the values of variables
    //TODO: good optimization would be to only update what has changed if possible
    private void Reset()
    {
        foreach(var shape in shapes)
        {
            
            UnityEngine.Rendering.Universal.ShadowCaster2D caster = shape.gameObject.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
            if (caster != null)
            {
                caster.enabled = false;
            }
        }

        //mark all used parents as not used
        parents.AddRange(usedParents);
        usedParents.Clear();

        List<GameObject> temp = new List<GameObject>();

        //uparent children and disable shadows
        foreach (var parent in parents)
        {
            int childCountBefore = parent.transform.childCount;

            //reparent children that are no longer visable
            foreach (var child in parent.GetComponentsInChildren<UnityEngine.Rendering.Universal.ShadowCaster2D>())
            {
                if (!IsVisable(child.GetComponent<Collider2D>()))
                {
                    child.transform.parent = parentDict[child];
                    parentDict.Remove(child);
                }
            }


            //if all children were removed detatch all children and mark 
            if (parent.transform.childCount == 0)
            {
                parent.transform.DetachChildren();
                temp.Add(parent);
            }
            else
            {
                usedParents.Add(parent);
            }
        }

        //clear the used parents list
        parents = temp; //make sure this is copying value not reference
    }

    //Get points from box collider (world space)
    Vector2[] BoxPoints(BoxCollider2D collider)
    {
        Vector2 pos = collider.transform.position;
        float insetDelta = .01f;    //amount to inset the corners by (there is a strange bug if on the exact corner of collider)


        Vector2 right = (collider.bounds.extents.x - insetDelta) * Vector2.right;
        Vector2 up = (collider.bounds.extents.y - insetDelta) * Vector2.up;
        Vector2[] points = new Vector2[]{
            (up + right) + pos,     //top right
            (-up + right) + pos,    //bottom right
            (-up - right) + pos,   //bottom left
            (up - right) + pos,     //top left
            };
        return points;
    }
    bool IsInternal(Collider2D collider)
    {
        Vector2 pos = collider.transform.position;
        //cast ray from collider to the right
        Collider2D collider_right = Physics2D.OverlapPoint( pos + (Vector2.right * (boxWidth * (3/ 2))));
        bool right = (collider_right != null && collider_right.CompareTag("Wall"));

        if (!right) { return false; }


        Collider2D collider_left = Physics2D.OverlapPoint(pos + (Vector2.left * (boxWidth * (3 / 2))));
        bool left = (collider_left != null && collider_left.CompareTag("Wall"));
        
        if (!left) { return false; }

        Collider2D collider_up = Physics2D.OverlapPoint(pos + (Vector2.up * (boxWidth * (3 / 2))));
        bool up = (collider_up != null && collider_up.CompareTag("Wall"));

        if (!up) { return false; }

        Collider2D collider_down = Physics2D.OverlapPoint(pos + (Vector2.down * (boxWidth * (3 / 2))));
        bool down = (collider_down != null && collider_down.CompareTag("Wall"));

        if (!down) { return false; }

        return true;
    }











































}
