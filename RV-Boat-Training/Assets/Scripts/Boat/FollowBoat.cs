using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBoat : MonoBehaviour
{
    public Transform boatTransform;
    public Vector3 offset = new Vector3(1.5f, 3.4f, -17f);
    public bool rotation = true;
    public bool yRotation = true;

    void Update()
    {
        if (!PauseMenu.gameIsPaused) {
            transform.position = Quaternion.Euler(boatTransform.eulerAngles) * offset + boatTransform.position;
            if (rotation)
            {
                if (yRotation) transform.rotation = boatTransform.rotation;
                else transform.rotation = Quaternion.Euler(boatTransform.rotation.eulerAngles.x, 0, boatTransform.rotation.eulerAngles.z);
            }
        }
    }
}
