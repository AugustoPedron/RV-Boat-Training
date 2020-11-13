using System;
using UnityEngine;

namespace BoatAttack
{
    public class Boat : MonoBehaviour
    {
        public Engine engine;
        private float steering = 0f;
        private float acceleration = 0f;
        public float steeringSpeed = 5f;
        public float accelerationSpeed = 5f;
        public float decelerationSpeed = 0.1f;

        private void Awake()
        {
            TryGetComponent(out engine.RB);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
            {
                steering -= steeringSpeed * Time.deltaTime;
                steering = Mathf.Clamp(steering, -1f, 0f);
                engine.Turn(steering);
            }
            if (steering < 0f && !Input.GetKey(KeyCode.A))
            {
                if (steering <= decelerationSpeed * 4 * Time.deltaTime)
                    steering += decelerationSpeed * 4 * Time.deltaTime;
                else
                    steering = 0f;
                engine.Turn(steering);
            }
            if (Input.GetKey(KeyCode.D))
            {
                steering += steeringSpeed * Time.deltaTime;
                steering = Mathf.Clamp01(steering);
                engine.Turn(steering);
            }
            if (steering > 0f && !Input.GetKey(KeyCode.D)){
                if (steering >= decelerationSpeed * 4 * Time.deltaTime)
                    steering -= decelerationSpeed * 4 * Time.deltaTime;
                else
                    steering = 0f;
                engine.Turn(steering);
            }
            if (Input.GetKey(KeyCode.W))
            {
                acceleration += accelerationSpeed * Input.mouseScrollDelta.y * Time.deltaTime;
                acceleration = Mathf.Clamp01(acceleration);
                engine.Accelerate(acceleration);
            }
            if (acceleration > 0f && !Input.GetKey(KeyCode.W))
            {
                if (acceleration >= decelerationSpeed * Time.deltaTime)
                    acceleration -= decelerationSpeed * Time.deltaTime;
                else
                    acceleration = 0f;
                engine.Accelerate(acceleration);
            }
            if (Input.GetKey(KeyCode.S))
            {
                acceleration -= accelerationSpeed * Input.mouseScrollDelta.y * Time.deltaTime;
                acceleration = Mathf.Clamp(acceleration, -1f, 0f);
                engine.Accelerate(acceleration);
            }
            if (acceleration < 0f && !Input.GetKey(KeyCode.S))
            {
                if (acceleration <= decelerationSpeed * Time.deltaTime)
                    acceleration += decelerationSpeed * Time.deltaTime;
                else
                    acceleration = 0f;
                engine.Accelerate(acceleration);
            }
        }

        private void LateUpdate()
        {
        }
    }
}
