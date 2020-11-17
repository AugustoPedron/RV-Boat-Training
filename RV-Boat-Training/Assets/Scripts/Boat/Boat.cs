using System;
using UnityEngine;
using System.Collections;

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
        private bool engineRunning = true;
        public float anchorProbability = 30f;
        public float anchorProbabilityIncrement = 5f;
        public bool anchorSet = false;
        public bool anchorStart = false;

        private void Awake()
        {
            TryGetComponent(out engine.RB);
        }

        private void OnEnable()
        {
            BoatEventManager.StartListening("emptyTank", StopEngine);
        }

        private void OnDisable()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
        }

        private void Update()
        {
            if (!anchorSet)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    if (engineRunning)
                    {
                        UpdateSteering(-1f);
                    }
                    else
                    {
                        steering = ResidualTurningForce(steering, -decelerationSpeed);
                    }
                }

                if (steering < 0f && !Input.GetKey(KeyCode.A))
                {
                    steering = ResidualTurningForce(steering, -decelerationSpeed);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    if (engineRunning)
                    {
                        UpdateSteering(1f);
                    }
                    else
                    {
                        steering = ResidualTurningForce(steering, decelerationSpeed);
                    }
                }

                if (steering > 0f && !Input.GetKey(KeyCode.D))
                {
                    steering = ResidualTurningForce(steering, decelerationSpeed);
                }

                if (Input.GetKey(KeyCode.W))
                {
                    if (engineRunning)
                    {
                        UpdateAcceleration(1f);
                    }
                    else
                    {
                        acceleration = ResidualForwardForce(acceleration, decelerationSpeed);
                    }
                }

                if (acceleration > 0f && !Input.GetKey(KeyCode.W))
                {
                    acceleration = ResidualForwardForce(acceleration, decelerationSpeed);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    if (engineRunning)
                    {
                        UpdateAcceleration(-1f);
                    }
                    else
                    {
                        acceleration = ResidualForwardForce(acceleration, -decelerationSpeed);
                    }
                }

                if (acceleration < 0f && !Input.GetKey(KeyCode.S))
                {
                    acceleration = ResidualForwardForce(acceleration, -decelerationSpeed);
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    anchorStart = true;
                }
            }

            if (anchorStart)
            {
                float prob = UnityEngine.Random.Range(0f, 100f);
                if (prob <= anchorProbability)
                {
                    anchorSet = true;
                    anchorStart = false;
                }

                else
                    anchorProbability += anchorProbabilityIncrement * Time.deltaTime;
            }

            if (anchorSet)
            {
                if (Input.GetKeyDown(KeyCode.F)) anchorSet = false;

                if(acceleration > 0f)
                {
                    acceleration = ResidualForwardForce(acceleration, 4f * decelerationSpeed);
                }

                if(acceleration < 0f)
                {
                    acceleration = ResidualForwardForce(acceleration, -4f * decelerationSpeed);
                }
            }
        }

        private void LateUpdate()
        {
        }

        private void StopEngine()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            engineRunning = false;
        }

        private void UpdateAcceleration(float forward)
        {
            acceleration += accelerationSpeed * Input.mouseScrollDelta.y * Time.deltaTime * forward;
            acceleration = Mathf.Clamp(acceleration, -1f, 1f);
            engine.Accelerate(acceleration);
        }

        private float ResidualForwardForce(float acceleration, float decelerationSpeed)
        {
            if (acceleration >= decelerationSpeed * Time.deltaTime)
                acceleration -= decelerationSpeed * Time.deltaTime;
            else
                acceleration = 0f;
            engine.AccelerateNoFuel(acceleration);
            return acceleration;
        }

        private void UpdateSteering(float right)
        {
            steering += steeringSpeed * Time.deltaTime * right;
            steering = Mathf.Clamp(steering, -1f, 1f);
            engine.Turn(steering);
        }

        private float ResidualTurningForce(float steering, float decelerationSpeed)
        {
            if (steering >= decelerationSpeed * 4 * Time.deltaTime)
                steering -= decelerationSpeed * 4 * Time.deltaTime;
            else
                steering = 0f;
            engine.TurnNoFuel(steering);
            return steering;
        }
    }
}
