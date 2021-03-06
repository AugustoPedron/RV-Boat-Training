﻿using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using WaterSystem;

namespace BoatAttack
{
    public class Engine : MonoBehaviour
    {
        [NonSerialized] public Rigidbody RB; // The rigid body attatched to the boat
        [NonSerialized] public float VelocityMag; // Boats velocity

        public AudioSource engineSound; // Engine sound clip
        public AudioSource waterSoundMoving; // Water sound clip when the boat is moving
        public AudioSource waterSound; //Water sound when the boat is not moving

        //engine stats
        public float steeringTorque = 5f;
        public float horsePower = 18f;
        public float fuel = 100f;
        public float tankCapacity = 200f;
        public float fuelConsumptionPerHour = 30f;
        public float maxVolumeWater = 0.75f;
        public float maxVolumeWaterMoving = 0.2f;

        private NativeArray<float3> _point; // engine submerged check
        private float3[] _heights = new float3[1]; // engine submerged check
        private float3[] _normals = new float3[1]; // engine submerged check
        private int _guid;
        private float _yHeight;

        public Vector3 enginePosition;
        private Vector3 _engineDir;
        private float _turnVel;
        private float _currentAngle;
        private bool updateFuel = false;
        private float engineSoundValue = 0f;
        [SerializeField]
        private float pitchMulti = 0.01f;
        [SerializeField]
        private float pitchOffset = 0f;

        private void Awake()
        {
            if (engineSound)
                engineSound.time = UnityEngine.Random.Range(0f, engineSound.clip.length); // randomly start the engine sound

            if (waterSoundMoving)
                waterSoundMoving.time = UnityEngine.Random.Range(0f, waterSoundMoving.clip.length); // randomly start the water_highSpeed sound

            if (waterSound)
                waterSound.time = UnityEngine.Random.Range(0f, waterSound.clip.length); // randomly start the water sound

            _guid = GetInstanceID(); // Get the engines GUID for the buoyancy system
            _point = new NativeArray<float3>(1, Allocator.Persistent);
        }

        private void OnEnable()
        {
            fuel = fuel <= tankCapacity ? fuel : tankCapacity;
        }

        private void FixedUpdate()
        {
            //VelocityMag = RB.velocity.sqrMagnitude; // get the sqr mag
            engineSound.pitch = Mathf.Min(pitchOffset + (Mathf.Abs(engineSoundValue) * pitchMulti), 2.5f); // use some magice numbers to control the pitch of the engine sound

            // Get the water level from the engines position and store it
            _point[0] = transform.TransformPoint(enginePosition);
            GerstnerWavesJobs.UpdateSamplePoints(ref _point, _guid);
            GerstnerWavesJobs.GetData(_guid, ref _heights, ref _normals);
            _yHeight = _heights[0].y - _point[0].y;

        }

        private void LateUpdate()
        {
            if (updateFuel)
            {
                fuel = fuel >= (fuelConsumptionPerHour / 3600) * Time.fixedDeltaTime ? fuel - ((fuelConsumptionPerHour / 3600) * Time.fixedDeltaTime) : 0f;
                updateFuel = false;
                if (fuel == 0f)
                {
                    BoatEventManager.TriggerEvent("emptyTank");
                    engineSound.Stop();
                }
            }
        }

        private void OnDisable()
        {
            _point.Dispose();
        }

        /// <summary>
        /// Controls the acceleration of the boat
        /// </summary>
        /// <param name="modifier">Acceleration modifier, adds force in the 0-1 range</param>
        public void Accelerate(float modifier)
        {
            if (_yHeight > -0.1f && fuel > 0f) // if the engine is deeper than 0.1
            {
                //modifier = Mathf.Clamp(modifier, -10f, 10f); // clamp for reasonable values
                var forward = RB.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                RB.AddForce(horsePower * modifier * forward, ForceMode.Acceleration); // add force forward based on input and horsepower
                RB.AddTorque(-Vector3.right * modifier * 0.03f, ForceMode.Acceleration);
                updateFuel = true;

                //blend lineare dei volumi
                //waterSound.volume = maxVolumeWater * (1 - Mathf.Clamp01((Mathf.Abs(modifier) - 0.25f) * 1.81f));
                //waterSoundMoving.volume = maxVolumeWaterMoving * Mathf.Clamp01((Mathf.Abs(modifier) - 0.3f) * 1.428f);

                //blend esponenziale dei volumi
                waterSound.volume = maxVolumeWater * (1 - Mathf.Clamp01((Mathf.Exp(Mathf.Abs(modifier)) - 1f) * 0.5f));
                waterSoundMoving.volume = maxVolumeWaterMoving * Mathf.Clamp01((Mathf.Exp(Mathf.Abs(modifier)) - 1f) * 0.5f);
            }
        }

        public void AccelerateNoFuel(float modifier)
        {
            if (_yHeight > -0.1f) // if the engine is deeper than 0.1
            {
                //modifier = Mathf.Clamp(modifier, -10f, 10f); // clamp for reasonable values
                var forward = RB.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                RB.AddForce(horsePower * modifier * forward, ForceMode.Acceleration); // add force forward based on input and horsepower
                RB.AddRelativeTorque(-Vector3.right * modifier * 0.01f, ForceMode.Acceleration);

                //blend lineare dei volumi
                //waterSound.volume = maxVolumeWater * (1 - Mathf.Clamp01((Mathf.Abs(modifier) - 0.25f) * 1.81f));
                //waterSoundMoving.volume = maxVolumeWaterMoving * Mathf.Clamp01((Mathf.Abs(modifier) - 0.3f) * 1.428f);

                //blend esponenziale dei volumi
                waterSound.volume = maxVolumeWater * (1 - Mathf.Clamp01((Mathf.Exp(Mathf.Abs(modifier)) - 1f) * 0.5f));
                waterSoundMoving.volume = maxVolumeWaterMoving * Mathf.Clamp01((Mathf.Exp(Mathf.Abs(modifier)) - 1f) * 0.5f);
            }
        }

        /// <summary>
        /// Controls the turning of the boat
        /// </summary>
        /// <param name="modifier">Steering modifier, positive for right, negative for negative</param>
        public void Turn(float modifier)
        {
            if (fuel > 0f)
            {
                if (_yHeight > -0.1f) // if the engine is deeper than 0.1
                {
                    //modifier = Mathf.Clamp(modifier, -1f, 1f); // clamp for reasonable values
                    RB.AddRelativeTorque(new Vector3(0f, steeringTorque, -steeringTorque * 0.5f) * modifier, ForceMode.Acceleration); // add torque based on input and torque amount
                    updateFuel = true;
                }

                _currentAngle = Mathf.SmoothDampAngle(_currentAngle,
                    60f * -modifier,
                    ref _turnVel,
                    0.5f,
                    10f,
                    Time.fixedTime);
                transform.localEulerAngles = new Vector3(0f, _currentAngle, 0f);
            }
        }

        public void TurnNoFuel(float modifier)
        {
            if (_yHeight > -0.1f) // if the engine is deeper than 0.1
            {
                //modifier = Mathf.Clamp(modifier, -1f, 1f); // clamp for reasonable values
                RB.AddRelativeTorque(new Vector3(0f, steeringTorque, -steeringTorque * 0.5f) * modifier, ForceMode.Acceleration); // add torque based on input and torque amount
            }

            _currentAngle = Mathf.SmoothDampAngle(_currentAngle,
                60f * -modifier,
                ref _turnVel,
                0.5f,
                10f,
                Time.fixedTime);
            transform.localEulerAngles = new Vector3(0f, _currentAngle, 0f);

        }

        public void SetFuel(float fuel) { this.fuel = fuel <= tankCapacity ? fuel : tankCapacity; }

        // Draw some helper gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(enginePosition, new Vector3(0.1f, 0.2f, 0.3f)); // Draw teh engine position with sphere
        }

        public void UpdateEngineSoundValue(float value)
        {
            engineSoundValue = value;
        }

        public void StopEngineSound()
        {
            waterSoundMoving.Stop();
        }
    }
}
