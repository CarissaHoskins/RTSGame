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

    [SerializeField] int health = 3;


	void Start ()
    {
        initial = GetComponent<MeshRenderer>().material.color;
        colliders = GetComponents<BoxCollider>();
        allRovers = GameObject.FindGameObjectsWithTag("Rover");
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
            GetComponent<MeshRenderer>().enabled = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }
    }
}
