using System;
using UnityEngine;
using System.Collections;

namespace BoatAttack
{
    public class Boat : MonoBehaviour
    {
        public Engine engine;
        private Action coroutineWrapper;
        private float steering = 0f;
        private float acceleration = 0f;
        public float steeringSpeed = 5f;
        public float accelerationSpeed = 5f;
        public float decelerationSpeed = 0.1f;
        private bool engineRunning = true;
        public float anchorProbability = 2f;
        public float anchorProbabilityIncrement = 0.05f;
        public bool anchorSet = false;
        public bool anchorStart = false;
        private Vector3 anchorStartingPosition = new Vector3( 0f, -1f, 0f );

        private void Awake()
        {
            TryGetComponent(out engine.RB);
        }

        private void OnEnable()
        {
            coroutineWrapper = () => StartCoroutine(UpdateAnchorProbability());
            BoatEventManager.StartListening("emptyTank", StopEngine);
            BoatEventManager.StartListening("anchorSet", coroutineWrapper.Invoke);
        }

        private void OnDisable()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            BoatEventManager.StopListening("anchorSet", coroutineWrapper.Invoke);
        }

        private void FixedUpdate()
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
                    GameObject anchor = GameObject.FindGameObjectWithTag("Anchor");
                    anchor.transform.parent = null;
                    BoatEventManager.TriggerEvent("dropAnchor");
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    anchorSet = false;
                    anchorProbability = 2f;
                    GameObject anchor = GameObject.FindGameObjectWithTag("Anchor");
                    anchor.transform.parent = transform;
                    BoatEventManager.TriggerEvent("resetAnchor");
                }

                if (acceleration > 0f)
                {
                    acceleration = ResidualForwardForce(acceleration, 4f * decelerationSpeed);
                }

                if (acceleration < 0f)
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
            acceleration += accelerationSpeed * Input.mouseScrollDelta.y * Time.fixedDeltaTime * forward;
            acceleration = Mathf.Clamp(acceleration, -1f, 1f);
            engine.Accelerate(acceleration);
        }

        private float ResidualForwardForce(float acceleration, float decelerationSpeed)
        {
            if (acceleration >= decelerationSpeed * Time.fixedDeltaTime)
                acceleration -= decelerationSpeed * Time.fixedDeltaTime;
            else
                acceleration = 0f;
            engine.AccelerateNoFuel(acceleration);
            return acceleration;
        }

        private void UpdateSteering(float right)
        {
            steering += steeringSpeed * Time.fixedDeltaTime * right;
            steering = Mathf.Clamp(steering, -1f, 1f);
            engine.Turn(steering);
        }

        private float ResidualTurningForce(float steering, float decelerationSpeed)
        {
            if (steering >= decelerationSpeed * 4 * Time.fixedDeltaTime)
                steering -= decelerationSpeed * 4 * Time.fixedDeltaTime;
            else
                steering = 0f;
            engine.TurnNoFuel(steering);
            return steering;
        }

        private IEnumerator UpdateAnchorProbability()
        {
            bool run = true;
            while (run)
            {
                if (acceleration > 0f)
                {
                    anchorProbability += anchorProbabilityIncrement * Time.deltaTime;
                    if(UnityEngine.Random.Range(0f, 100f) <= anchorProbability)
                    {
                        anchorSet = true;
                        yield break;
                    }
                }
                yield return new WaitForSecondsRealtime(0.4f);
            }
        }
    }
}
