using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlockBehavior : MonoBehaviour
{
    private Color initial;
    private Color highlighted;
    private BoxCollider[] colliders;
    private GameObject[] allRovers;

    public bool isSelceted = false;
    public bool isWalkable = false;
    public bool isAccessible = false;

    [SerializeField] int health = 3;


	void Start ()
    {
        initial = GetComponent<MeshRenderer>().material.color;
        colliders = GetComponents<BoxCollider>();
        allRovers = GameObject.FindGameObjectsWithTag("Rover");

        if (GetComponent<MeshRenderer>().enabled)
            isWalkable = false;
        else
            isWalkable = true;
	}

    private void OnMouseDown()
    {
        if (!isSelceted)
        {
            for (int i = 0; i < allRovers.Length; ++i)
                allRovers[i].SendMessage("AddWork", this.gameObject);

            highlighted = GetComponent<MeshRenderer>().material.color;
            highlighted.g = 255;
            GetComponent<MeshRenderer>().material.color = highlighted;
            isSelceted = true;
        }
        else
        {
            for (int i = 0; i < allRovers.Length; ++i)
                allRovers[i].SendMessage("RemoveWork", this.gameObject);

            GetComponent<MeshRenderer>().material.color = initial;
            isSelceted = false;
        }
    }

    void Hit()
    {
        health -= 1;
        if (health <= 0)
        {
            for (int i = 0; i < allRovers.Length; ++i)
                allRovers[i].SendMessage("RemoveWork", this.gameObject);

            GetComponent<MeshRenderer>().enabled = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
            isWalkable = true;

            ActivateBlock(transform.position, Vector3.forward);
            ActivateBlock(transform.position, Vector3.back);
            ActivateBlock(transform.position, Vector3.right);
            ActivateBlock(transform.position, Vector3.left);
        }
    }

    void ActivateBlock(Vector3 _origin, Vector3 _dirrection)
    {
        RaycastHit lilCheck;
        Physics.Raycast(_origin, _dirrection, out lilCheck, 1.5f);
        if (lilCheck.transform.tag == "Block")
            lilCheck.transform.GetComponent<BreakableBlockBehavior>().isAccessible = true;
    }
}
