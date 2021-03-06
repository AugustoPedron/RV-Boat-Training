﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraControls : MonoBehaviour
{
    public Texture2D crosshair;
    public Canvas canvas;
    public float mouseSpeed = 2f;

    private float pitch = 0f;
    private float yaw = 0f;
    private bool usingThrottle = false;
    private Rect position;
    private int crosshairDimension = 7;
    private bool cameraEnabled = true;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        BoatEventManager.StartListening("endNavigation", DisableCameraMovements);
    }

    private void OnDisable()
    {
        BoatEventManager.StopListening("endNavigation", DisableCameraMovements);
    }

    private void Start()
    {
        position = new Rect((Screen.width - crosshairDimension) / 2, (Screen.height - crosshairDimension) / 2, crosshairDimension, crosshairDimension);
    }

    // Update is called once per frame
    void Update()
    {

        //----------- Movimento della camera con il mouse -----------------
        if (cameraEnabled && !usingThrottle && !PauseMenu.gameIsPaused)
        {
            pitch -= Input.GetAxis("Mouse Y") * mouseSpeed;
            pitch = Mathf.Clamp(pitch, -90f, 90f);
            yaw += Input.GetAxis("Mouse X") * mouseSpeed;
            yaw = yaw % 360f;
            transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (cameraEnabled)
        {
            //---------- Blocco della camera quando si vuole interagire con la leva del motore ---------------
            if (Input.GetKey(KeyCode.Mouse0))
            {
                int layerMask = 1 << 8;
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    //Debug.Log("hit");
                    usingThrottle = true;
                    BoatEventManager.TriggerEvent("enable");
                    //TO DO: finire la gestione una volta creato il modello della barca
                }
            }

            //Debug.DrawRay(transform.position, transform.forward * 30, Color.red);

            if (usingThrottle && !Input.GetKey(KeyCode.Mouse0))
            {
                //Debug.Log("release");
                usingThrottle = false;
                BoatEventManager.TriggerEvent("disable");
            }
        }
    }

    private void OnGUI()
    {
        GUI.DrawTexture(position, crosshair);
    }

    private void LockUnlockCamera()
    {
        usingThrottle = !usingThrottle;
    }

    private void DisableCameraMovements()
    {
        BoatEventManager.StopListening("endNavigation", DisableCameraMovements);
        cameraEnabled = false;
    }
}
