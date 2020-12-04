using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BoatAttack
{
    public class ThrottleControl : MonoBehaviour
    {
        public Boat boat;
        public Transform boatTransform;
        public int steps = 7;
        public float mouseSpeed = 2f;
        public float throttleDeadZone = 2.5f;
        private Vector3 offset = new Vector3(1.5f, 3.4f, -17f);
        private float rotation = -20f;
        private float throttlePosition = 0f;
        private bool enableControl = false;
        private float engineValue = 0f;


        private void OnEnable()
        {
            BoatEventManager.StartListening("enable", Enable);
            BoatEventManager.StartListening("disable", Disable);
        }

        private void OnDisable()
        {
            BoatEventManager.StopListening("enable", Enable);
            BoatEventManager.StopListening("disable", Disable);
        }

        private void Update()
        {
            if (enableControl)
            {
                rotation += Input.GetAxis("Mouse Y") * mouseSpeed;
                rotation = Mathf.Clamp(rotation, -55f, 40f);
                throttlePosition = rotation + 20f;
                if (throttlePosition > throttleDeadZone || throttlePosition < -throttleDeadZone)
                {
                    engineValue = throttlePosition > 0 ? throttlePosition / (60 - throttleDeadZone) : throttlePosition / (35 - throttleDeadZone);
                    boat.UpdateEngingeValue(engineValue);
                }
                else
                {
                    engineValue = 0f;
                    boat.UpdateEngingeValue(engineValue);
                }
            }

            transform.position = Quaternion.Euler(boatTransform.eulerAngles) * offset + boatTransform.position;
            transform.rotation = boatTransform.rotation * Quaternion.Euler(rotation, 0, 0);
        }

        private void Enable()
        {
            enableControl = true;
        }

        private void Disable()
        {
            enableControl = false;
        }
    }
}
