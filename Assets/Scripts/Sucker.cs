using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sucker : MonoBehaviour
{
    [Header("Set in inspector")]
    [SerializeField] BoxCollider suckerCollider;

    void FixedUpdate()
    {
        // Check for overlap with leaves
        Collider[] hitColliders = Physics.OverlapBox(suckerCollider.bounds.center, suckerCollider.bounds.extents, suckerCollider.transform.rotation, LayerMask.GetMask("Leaf"));

        foreach (Collider collider in hitColliders)
        {
            Leaf leaf = collider.gameObject.GetComponent<Leaf>();
            if (leaf != null)
            {
                leaf.Suck();
            }
        }
    }

}
