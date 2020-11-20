using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    public Rigidbody RB;
    public bool start = false;
    private Vector3 initialPosition = new Vector3(0f, -1.1f, 0f);

    private void OnEnable()
    {
        BoatEventManager.StartListening("dropAnchor", DropAnchor);
        BoatEventManager.StartListening("resetAnchor", ResetAnchor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            BoatEventManager.TriggerEvent("anchorSet");
            start = true;
            RB.isKinematic = true;
        }
    }

    private void OnDisable()
    {
        BoatEventManager.StopListening("dropAnchor", DropAnchor);
        BoatEventManager.StopListening("resetAnchor", ResetAnchor);
    }

    private void DropAnchor()
    {
        RB.isKinematic = false;
    }

    private void ResetAnchor()
    {
        transform.localPosition = initialPosition;
    }
}
