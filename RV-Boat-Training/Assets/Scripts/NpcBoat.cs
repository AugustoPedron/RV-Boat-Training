using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBoat : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject playerboat;
    public float steeringTorque = 5f;
    public float horsePower = 18f;

    void Start()
    {
        playerboat = GameObject.Find("Boat");
    }

    // Update is called once per frame
    void Update()
    {


        var target = playerboat.transform.position;
        target.x =target.x + 50f;
        target.y = target.z + 50f;
        Vector3 targetDirection = target - transform.position;
        targetDirection.y = 0f;
        if (targetDirection.magnitude > 200)
        { Avanza(targetDirection); }

        else
            Avanza(transform.forward);
        




    }

    public void Avanza(Vector3 follow)
    {
      
        follow.Normalize();
        
        rb.AddForce(horsePower * follow, ForceMode.Acceleration);      //Move object along its forward axis
        
    }
}
