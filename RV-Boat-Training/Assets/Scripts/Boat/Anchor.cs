using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    public Rigidbody RB;
    public bool start = false;
    public Vector3 initialPosition = new Vector3(0f, -1f, 0f);

    private void OnEnable()
    {
        BoatEventManager.StartListening("dropAnchor", DropAnchor);
        BoatEventManager.StartListening("resetAnchor", ResetAnchor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Terrain")
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
