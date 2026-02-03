using Unity.VisualScripting;
using UnityEngine;

public class CollisionTester : MonoBehaviour
{

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Somethign happened");
    }
}
