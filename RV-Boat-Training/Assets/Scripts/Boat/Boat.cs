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
        private bool anchorSet = false;
        private float engineValue = 0f;
        private Rect position;
        private int crosshairDimension = 7;
        public Texture2D crosshair;

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
        }

        private void OnDisable()
        {
            BoatEventManager.StopListening("emptyTank", StopEngine);
            BoatEventManager.StopListening("anchorSet", coroutineWrapper.Invoke);
            BoatEventManager.StopListening("updateThrottle", UpdateEngingeValue);
        }

        private void Start()
        {
            position = new Rect((Screen.width - crosshairDimension) / 2, (Screen.height - crosshairDimension) / 2, crosshairDimension, crosshairDimension);
        }

        private void Update()
        {
            if (engineValue >= 0)
            {
                if (engineValue - acceleration > 0)
                {
                    Acceleration(accelerationSpeed);
                }
                else if (engineValue - acceleration < 0)
                {
                    Deceleration(decelerationSpeed);
                }
            }
            else if (engineValue < 0)
            {
                if (engineValue - acceleration < 0)
                {
                    Acceleration(-accelerationSpeed);
                }
                else if (engineValue - acceleration > 0)
                {
                    Deceleration(-decelerationSpeed);
                }
            }
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

                if (engineValue >= 0)
                {
                    if (engineRunning)
                    {
                        engine.Accelerate(acceleration);
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
                    }
                    else
                    {
                        ResidualForwardForce(-decelerationSpeed);
                    }
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
                    ResidualForwardForce(4f * decelerationSpeed);
                }

                if (acceleration < 0f)
                {
                    ResidualForwardForce(-4f * decelerationSpeed);
                }
            }
        }

        private void OnGUI()
        {
            GUI.DrawTexture(position, crosshair);
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
                    anchorProbability += anchorProbabilityIncrement * Time.deltaTime;
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

        private void Acceleration(float accelerationSpeed)
        {
            acceleration += accelerationSpeed * Time.deltaTime;
            if (accelerationSpeed > 0) acceleration = Mathf.Clamp(acceleration, float.MinValue, engineValue);
            else acceleration = Mathf.Clamp(acceleration, engineValue, float.MaxValue);
        }

        private void Deceleration(float decelerationSpeed)
        {
            acceleration -= decelerationSpeed * Time.deltaTime;
            if (decelerationSpeed > 0) acceleration = Mathf.Clamp(acceleration, engineValue, float.MaxValue);
            else acceleration = Mathf.Clamp(acceleration, float.MinValue, engineValue);
        }
    }
}
