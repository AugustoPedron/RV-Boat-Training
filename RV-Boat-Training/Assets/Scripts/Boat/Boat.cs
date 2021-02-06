using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BoatAttack
{
    public class Boat : MonoBehaviour
    {
        public Engine engine;
        public float steeringSpeed = 5f;
        public float accelerationSpeed = 5f;
        public float decelerationSpeed = 0.1f;
        public float anchorProbability = 2f;
        public float anchorProbabilityIncrement = 0.05f;

        private Action coroutineWrapper;
        private float steering = 0f;
        private float acceleration = 0f;
        private bool engineRunning = true;
        private bool anchorSet = false;
        private bool anchorDropped = false;
        private float engineValue = 0f;
        private float engineSound = 0f;
        private bool controlsEnbabled = true;

        private void Awake()
        {
            TryGetComponent(out engine.RB);
        }

        private void OnEnable()
        {
            coroutineWrapper = () => StartCoroutine(UpdateAnchorProbability());
            BoatEventManager.StartListening("emptyTank", StopEngine);
            BoatEventManager.StartListening("anchorSet", coroutineWrapper.Invoke);
            BoatEventManager.StartListening("anchorUnset", AnchorUnset);
            BoatEventManager.StartListening("endNavigation", DisableControls);
        }

        private void OnDisable()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            BoatEventManager.StopListening("anchorSet", coroutineWrapper.Invoke);
            BoatEventManager.StopListening("anchorUnset", AnchorUnset);
            BoatEventManager.StopListening("endNavigation", DisableControls);
        }

        private void Update()
        {
            if (controlsEnbabled && !PauseMenu.gameIsPaused)
            {
                UpdateValue(ref acceleration, 1);
                UpdateValue(ref engineSound, 3);
            }
        }

        private void FixedUpdate()
        {
            if (controlsEnbabled)
            {
                if (!anchorSet)
                {
                    if (engineValue >= 0)
                    {
                        if (engineRunning)
                        {
                            engine.Accelerate(acceleration);
                            engine.UpdateEngineSoundValue(engineSound);
                        }
                        else
                        {
                            ResidualForwardForce(decelerationSpeed);
                        }
                    }

                    if (engineValue < 0)
                    {
                        if (engineRunning)
                        {
                            engine.Accelerate(acceleration);
                            engine.UpdateEngineSoundValue(engineSound);
                        }
                        else
                        {
                            ResidualForwardForce(-decelerationSpeed);
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.F) && !anchorDropped)
                    {
                        GameObject anchor = GameObject.FindGameObjectWithTag("Anchor");
                        anchor.transform.parent = null;
                        anchor.GetComponent<Collider>().enabled = true;
                        BoatEventManager.TriggerEvent("dropAnchor");
                        anchorDropped = true;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        BoatEventManager.TriggerEvent("resetAnchor");
                    }

                    if (acceleration > 0f)
                    {
                        ResidualForwardForce(2 * decelerationSpeed);
                    }

                    if (acceleration < 0f)
                    {
                        ResidualForwardForce(2 * decelerationSpeed);
                    }

                    engine.UpdateEngineSoundValue(engineSound);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    if (engineRunning)
                    {
                        UpdateSteering(-1f);
                    }
                    else
                    {
                        ResidualTurningForce(-decelerationSpeed);
                    }
                }
                else
                {
                    if (steering < 0f) ResidualTurningForce(-decelerationSpeed);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    if (engineRunning)
                    {
                        UpdateSteering(1f);
                    }
                    else
                    {
                        ResidualTurningForce(decelerationSpeed);
                    }
                }
                else
                {
                    if (steering > 0f) ResidualTurningForce(decelerationSpeed);
                }
            }
        }

        private void AnchorUnset()
        {
            anchorSet = false;
            anchorDropped = false;
            anchorProbability = 2f;
            GameObject anchor = GameObject.FindGameObjectWithTag("Anchor");
            anchor.transform.parent = transform;
            anchor.GetComponent<Collider>().enabled = false;
        }

        private void StopEngine()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            engineRunning = false;
        }

        private void ResidualForwardForce(float decelerationSpeed)
        {
            if (acceleration >= decelerationSpeed * Time.fixedDeltaTime)
                acceleration -= decelerationSpeed * Time.fixedDeltaTime;
            else
                acceleration = 0f;
            engine.AccelerateNoFuel(acceleration);
        }

        private void UpdateSteering(float right)
        {
            steering += steeringSpeed * Time.fixedDeltaTime * right;
            steering = Mathf.Clamp(steering, -1f, 1f);
            engine.Turn(steering);
        }

        private void ResidualTurningForce(float decelerationSpeed)
        {
            if (steering >= decelerationSpeed * 4 * Time.fixedDeltaTime)
                steering -= decelerationSpeed * 4 * Time.fixedDeltaTime;
            else
                steering = 0f;
            engine.TurnNoFuel(steering);
        }

        private IEnumerator UpdateAnchorProbability()
        {
            bool run = true;
            while (run)
            {
                if (engineValue != 0)
                {
                    anchorProbability += anchorProbability * anchorProbabilityIncrement * Time.deltaTime;
                    if (UnityEngine.Random.Range(0f, 100f) <= anchorProbability)
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

        private void Acceleration(ref float value, float accelerationSpeed)
        {
            value += accelerationSpeed * Time.deltaTime;
            if (accelerationSpeed > 0) value = Mathf.Clamp(value, float.MinValue, engineValue);
            else value = Mathf.Clamp(value, engineValue, float.MaxValue);
        }

        private void Deceleration(ref float value, float decelerationSpeed)
        {
            value -= decelerationSpeed * Time.deltaTime;
            if (decelerationSpeed > 0) value = Mathf.Clamp(value, engineValue, float.MaxValue);
            else value = Mathf.Clamp(value, float.MinValue, engineValue);
        }

        private void UpdateValue(ref float value, float modifier)
        {
            if (engineValue >= 0)
            {
                if (engineValue - value > 0)
                {
                    Acceleration(ref value, modifier * accelerationSpeed);
                }
                else if (engineValue - value < 0)
                {
                    Deceleration(ref value, modifier * decelerationSpeed);
                }
            }
            else if (engineValue < 0)
            {
                if (engineValue - value < 0)
                {
                    Acceleration(ref value, modifier * -accelerationSpeed);
                }
                else if (engineValue - value > 0)
                {
                    Deceleration(ref value, modifier * -decelerationSpeed);
                }
            }
        }

        private void DisableControls()
        {
            BoatEventManager.StopListening("endNavigation", DisableControls);
            controlsEnbabled = false;
            acceleration = 0;
            steering = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            BoatEventManager.TriggerEvent("buoyReached");
        }

    }
}
