using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBoat : MonoBehaviour
{
    public Rigidbody rb;
    public float velocity = 1f;
    public GameObject playerboat;

    // Start is called before the first frame update
    void Start()
    {
        playerboat = GameObject.Find("Boat");
    }

    // Update is called once per frame
    void Update()
    {

        if (playerboat.transform.position.z > 1000) Avanza();

    }
    public void Avanza()
    {
var forward = rb.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        rb.AddForce(velocity * forward, ForceMode.Acceleration);
    }
}
