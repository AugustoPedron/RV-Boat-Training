using System;
using UnityEngine;

namespace BoatAttack
{
    public class Boat : MonoBehaviour
    {
        public Engine engine;
        private float steering = 0f;
        private float acceleration = 0.01f;
        public float steeringSpeed = 5f;
        public float accelerationSpeed = 35f;

        private void Awake()
        {
            TryGetComponent(out engine.RB);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
            {
                //steering -= steeringSpeed * Time.deltaTime;
                engine.Turn(-1);
            }
            if (Input.GetKey(KeyCode.D))
            {
                //steering += steeringSpeed * Time.deltaTime;
                engine.Turn(1);
            }
            if (Input.GetKey(KeyCode.W))
            {
                acceleration += accelerationSpeed * Time.deltaTime;
                engine.Accelerate(acceleration);
            }
            if (Input.GetKey(KeyCode.S))
            {
                acceleration -= accelerationSpeed * Time.deltaTime;
                engine.Accelerate(acceleration);
            }
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            {
                acceleration = 0f;
                engine.Accelerate(0);
            }
        }

        private void LateUpdate()
        {
        }
    }
}
