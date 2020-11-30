using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BoatAttack
{
    public class Boat : MonoBehaviour
    {
        public Engine engine;
        private Action coroutineWrapper;
        public Slider slider;
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
        private float engineValue = 0f;
        private float maxThrottle = 0f;
        private float maxReverseThrottle = 0f;

        private void Awake()
        {
            TryGetComponent(out engine.RB);
        }

        private void OnEnable()
        {
            coroutineWrapper = () => StartCoroutine(UpdateAnchorProbability());
            BoatEventManager.StartListening("emptyTank", StopEngine);
            BoatEventManager.StartListening("anchorSet", coroutineWrapper.Invoke);
            BoatEventManager.StartListening("updateThrottle", UpdateEngingeValue);
            maxThrottle = 1 / slider.maxValue;
            maxReverseThrottle = -1 / slider.minValue;
        }

        private void OnDisable()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            BoatEventManager.StopListening("anchorSet", coroutineWrapper.Invoke);
            BoatEventManager.StopListening("updateThrottle", UpdateEngingeValue);
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

                if (engineValue > 0)
                {
                    if (engineRunning)
                    {
                        engine.Accelerate(engineValue);
                    }
                    else
                    {
                        acceleration = ResidualForwardForce(acceleration, decelerationSpeed);
                    }
                }

                if (acceleration > 0f && engineValue == 0)
                {
                    acceleration = ResidualForwardForce(acceleration, decelerationSpeed);
                }

                if (engineValue < 0)
                {
                    if (engineRunning)
                    {
                        engine.Accelerate(engineValue);
                    }
                    else
                    {
                        acceleration = ResidualForwardForce(acceleration, -decelerationSpeed);
                    }
                }

                if (acceleration < 0f && engineValue == 0)
                {
                    acceleration = ResidualForwardForce(acceleration, -decelerationSpeed);
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    GameObject anchor = GameObject.FindGameObjectWithTag("Anchor");
                    anchor.transform.parent = null;
                    anchor.GetComponent<Collider>().enabled = true;
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
                    anchor.GetComponent<Collider>().enabled = false;
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

        private void StopEngine()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            engineRunning = false;
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
                if (engineValue != 0)
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

        public void UpdateEngingeValue(float value)
        {
            engineValue = value;
        }
    }
}
