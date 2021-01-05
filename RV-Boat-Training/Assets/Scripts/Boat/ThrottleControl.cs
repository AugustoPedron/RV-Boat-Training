using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BoatAttack
{
    public class ThrottleControl : MonoBehaviour
    {
        public Boat boat;
        public Engine engine;
        public Transform boatTransform;
        public float startingPosition = -20f;
        public float maxThrottleRotation = 40f;
        public float minThrottleRotation = -55f;
        public float throttleDeadZone = 2.5f;  //zona nella quale l'acceleratore seppur ruotato non attiva il motore
        public float mouseSpeed = 2f;

        private float rotation = -20f;
        private float throttlePosition = 0f;
        private float maxThrottlePosition = 0;
        private float minThrottlePosition = 0;
        private bool enableControl = false;
        private float engineValue = 0f;


        private void OnEnable()
        {
            BoatEventManager.StartListening("enable", Enable);
            BoatEventManager.StartListening("disable", Disable);
            maxThrottlePosition = maxThrottleRotation - startingPosition - throttleDeadZone;
            minThrottlePosition = -(minThrottleRotation - startingPosition - throttleDeadZone);
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
                rotation = Mathf.Clamp(rotation, minThrottleRotation, maxThrottleRotation);
                throttlePosition = rotation + 20f;
                if (throttlePosition > throttleDeadZone || throttlePosition < -throttleDeadZone)
                {
                    engineValue = throttlePosition > 0 ? (throttlePosition - throttleDeadZone) / maxThrottlePosition : (throttlePosition - throttleDeadZone) / minThrottlePosition;
                }
                else
                {
                    engineValue = 0f;
                }

                boat.UpdateEngingeValue(engineValue);
            }

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
