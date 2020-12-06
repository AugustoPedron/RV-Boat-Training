// Buoyancy.cs
// by Alex Zhdankin
// Version 2.1
//
// http://forum.unity3d.com/threads/72974-Buoyancy-script
//
// Terms of use: do whatever you like
//
// Further tweaks by Andre McGrail
//
//

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace WaterSystem
{
    public class BuoyantObject : MonoBehaviour
    {
        public BuoyancyType _buoyancyType; // type of buoyancy to calculate
        public float density; // density of the object, this is calculated off it's volume and mass
        public float volume; // volume of the object, this is calculated via it's colliders
        public float voxelResolution = 0.51f; // voxel resolution, represents the half size of a voxel when creating the voxel representation
        private float halfVoxelResolution = 0f;
        private float voxelArea = 0f;
        private float voxelAreaRho = 0f;

        private Bounds _voxelBounds; // bounds of the voxels
        private int voxelXLength = 0;
        private int voxelYLength = 0;
        private int voxelZLength = 0;
        public Vector3 centerOfMass = Vector3.zero; // Center Of Mass offset
        public float waterLevelOffset = 0f;

        private const float Dampner = 0.005f;
        private const float WaterDensity = 1000;
        private const float rhoWater = 513f;
        private const float rhoAir = 0.6275f;

        private float _baseDrag; // reference to original drag
        private float _baseAngularDrag; // reference to original angular drag
        private int _guid; // GUID for the height system
        private float3 _localArchimedesForce;

        private Vector3 totalWaterDrag;
        private Vector3 totalAirDrag;

        private Vector3 transformPosition;
        private List<Voxel> externalVoxels = new List<Voxel>();
        //private List<FaceData> faces = new List<FaceData>();
        //private List<FaceData> submergedFaces = new List<FaceData>();
        private Vector3[] _voxels; // voxel position
        private float[] submergedAmounts;
        private float[] ks;
        private NativeArray<float3> _samplePoints; // sample points for height calc
        [NonSerialized] public float3[] Heights; // water height array(only size of 1 when simple or non-physical)
        private float3[] _normals; // water normal array(only used when non-physical and size of 1 also when simple)
        private float3[] _velocity; // voxel velocity for buoyancy
        [SerializeField] Collider[] colliders; // colliders attatched ot this object
        private Rigidbody _rb;
        private DebugDrawing[] _debugInfo; // For drawing force gizmos
        [NonSerialized] public float PercentSubmerged;

        private float xposForceTot = 0f;
        private float xnegForceTot = 0f;

        [ContextMenu("Initialize")]
        private void Init()
        {
            _voxels = null;
            voxelArea = voxelResolution * voxelResolution;
            voxelAreaRho = voxelArea * rhoWater;
            halfVoxelResolution = voxelResolution * 0.5f;
            //halfVoxelResolution = Truncate(halfVoxelResolution, 1);

            switch (_buoyancyType)
            {
                case BuoyancyType.NonPhysical:
                    SetupVoxels();
                    SetupData();
                    break;
                case BuoyancyType.NonPhysicalVoxel:
                    SetupColliders();
                    SetupVoxels();
                    SetupData();
                    break;
                case BuoyancyType.Physical:
                    SetupVoxels();
                    SetupData();
                    SetupPhysical();
                    break;
                case BuoyancyType.PhysicalVoxel:
                    SetupColliders();
                    SetupVoxels();
                    SetupData();
                    SetupPhysical();
                    //SetupFaces();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetupVoxels()
        {
            if (_buoyancyType == BuoyancyType.NonPhysicalVoxel || _buoyancyType == BuoyancyType.PhysicalVoxel)
            {
                SliceIntoVoxels();
            }
            else
            {
                _voxels = new Vector3[1];
                _voxels[0] = centerOfMass;
            }
        }

        private void SetupData()
        {
            _debugInfo = new DebugDrawing[_voxels.Length];
            Heights = new float3[_voxels.Length];
            _normals = new float3[_voxels.Length];
            _samplePoints = new NativeArray<float3>(_voxels.Length, Allocator.Persistent);
        }

        private void OnEnable()
        {
            _guid = gameObject.GetInstanceID();
            Init();
            LocalToWorldConversion();
        }

        private void SetupColliders()
        {
            // The object must have a Collider
            colliders = GetComponentsInChildren<Collider>();
            if (colliders.Length != 0) return;

            colliders = new Collider[1];
            colliders[0] = gameObject.AddComponent<BoxCollider>();
            Debug.LogError($"Buoyancy:Object \"{name}\" had no coll. BoxCollider has been added.");
        }

        private void Update()
        {
            switch (_buoyancyType)
            {
                case BuoyancyType.NonPhysical:
                    {
                        var t = transform;
                        var vec = t.position;
                        vec.y = Heights[0].y + waterLevelOffset;
                        t.position = vec;
                        t.up = Vector3.Slerp(t.up, _normals[0], Time.deltaTime);
                        break;
                    }
                case BuoyancyType.NonPhysicalVoxel:
                    // do the voxel non-physical
                    break;
                case BuoyancyType.Physical:
                    LocalToWorldJob.CompleteJob(_guid);
                    GetVelocityPoints();
                    break;
                case BuoyancyType.PhysicalVoxel:
                    LocalToWorldJob.CompleteJob(_guid);
                    GetVelocityPoints();
                    //UpdateFacesVelocity();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GerstnerWavesJobs.UpdateSamplePoints(ref _samplePoints, _guid);
            GerstnerWavesJobs.GetData(_guid, ref Heights, ref _normals);
        }

        private void FixedUpdate()
        {
            transformPosition = transform.position;
            var submergedAmount = 0f;
            //float Cf = ResistanceCoefficient(rhoWater, _rb.velocity.magnitude, _rb.transform.localScale.z);
            totalWaterDrag = new Vector3();
            totalAirDrag = new Vector3();

            switch (_buoyancyType)
            {
                case BuoyancyType.PhysicalVoxel:
                    {
                        LocalToWorldJob.CompleteJob(_guid);
                        //Debug.Log("new pass: " + gameObject.name);
                        Physics.autoSyncTransforms = false;

                        for (var i = 0; i < _voxels.Length; i++)
                            submergedAmount += SubmergedAmountOfObject(_samplePoints[i], Heights[i].y, ref ks[i]);
                        
                        for (var i = 0; i < _voxels.Length; i++)
                            BuoyancyForce(_samplePoints[i], _velocity[i], Heights[i].y + waterLevelOffset, submergedAmount, ks[i], ref _debugInfo[i], _voxels[i], ref xposForceTot, ref xnegForceTot);

                        //AddFrictionForces(Cf);

                        Physics.SyncTransforms();
                        Physics.autoSyncTransforms = true;
                        UpdateDrag(submergedAmount);
                        break;
                    }
                case BuoyancyType.Physical:
                    //LocalToWorldJob.CompleteJob(_guid);
                    //BuoyancyForce(Vector3.zero, _velocity[0], Heights[0].y + waterLevelOffset, ref submergedAmount, ref _debugInfo[0]);
                    //UpdateDrag(submergedAmount);
                    break;
                case BuoyancyType.NonPhysical:
                    break;
                case BuoyancyType.NonPhysicalVoxel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LateUpdate() { LocalToWorldConversion(); }

        private void OnDisable() { LocalToWorldJob.Cleanup(_guid); }

        private void OnDestroy() { LocalToWorldJob.Cleanup(_guid); }

        private void LocalToWorldConversion()
        {
            if (_buoyancyType != BuoyancyType.Physical && _buoyancyType != BuoyancyType.PhysicalVoxel) return;

            var transformMatrix = transform.localToWorldMatrix;
            LocalToWorldJob.ScheduleJob(_guid, transformMatrix);
        }

        private float SubmergedAmountOfObject(Vector3 position, float waterHeight, ref float ks)
        {
            if (!(position.y - voxelResolution < waterHeight)) return 0f;

            ks = math.clamp(waterHeight - (position.y - voxelResolution), 0f, 1f);

            return ks / _voxels.Length;
        }

        private void BuoyancyForce(Vector3 position, float3 velocity, float waterHeight, float submergedAmount, float k, ref DebugDrawing debug, Vector3 voxelP, ref float xpos, ref float xneg)
        {
            debug.Position = position;
            debug.WaterHeight = waterHeight;
            debug.Force = Vector3.zero;

            if (!(position.y - voxelResolution < waterHeight)) return;

            //var k = math.clamp(waterHeight - (position.y - voxelResolution), 0f, 1f);

            //submergedAmount += k / _voxels.Length;

            var localDampingForce = Dampner * _rb.mass * -velocity;
            var force = localDampingForce + math.sqrt(k) * _localArchimedesForce * submergedAmount;
            _rb.AddForceAtPosition(force, position);

        }

        private void UpdateDrag(float submergedAmount)
        {
            PercentSubmerged = math.lerp(PercentSubmerged, submergedAmount, 0.25f);
            _rb.drag = _baseDrag + _baseDrag * (PercentSubmerged * 10f);
            _rb.angularDrag = _baseAngularDrag + PercentSubmerged * 0.5f;
        }

        //private void AddFrictionForces(float Cf)
        //{
        //    for (int j = 0; j < externalVoxels.Count; j++)
        //    {
        //        //Check if the face is above or under the water level
        //        if (externalVoxels[j].position.y - voxelResolution < Heights[externalVoxels[j].index].y)
        //        {
        //            //Add under water forces
        //            for (int i = 0; i < externalVoxels[j].faces.Count; i++)
        //            {
        //                ViscousWaterResistanceForce(externalVoxels[j].faces[i], Cf);
        //            }
        //        }
        //        else
        //        {
        //            //Add above water forces
        //            for (int i = 0; i < externalVoxels[j].faces.Count; i++)
        //            {
        //                //AirResistanceForce(rhoAir, externalVoxels[j].faces[i], _rb.drag);
        //            }
        //        }
        //    }
        //}

        //Force 1 - Viscous Water Resistance (Frictional Drag)
        //public void ViscousWaterResistanceForce(FaceData face, float Cf)
        //{
        //    //Viscous resistance occurs when water sticks to the boat's surface and the boat has to drag that water with it

        //    // F = 0.5 * rho * v^2 * S * Cf
        //    // rho - density of the medium you have
        //    // v - speed
        //    // S - surface area
        //    // Cf - Coefficient of frictional resistance

        //    //We need the tangential velocity 
        //    //Projection of the velocity on the plane with the normal normalvec
        //    //http://www.euclideanspace.com/maths/geometry/elements/plane/lineOnPlane/
        //    Vector3 B = face.normal;
        //    Vector3 A = face.velocity;

        //    Vector3 velocityTangent = Vector3.Cross(B, (Vector3.Cross(A, B) / face.normalMagnitude)) / face.normalMagnitude;

        //    //The direction of the tangential velocity (-1 to get the flow which is in the opposite direction)
        //    Vector3 tangentialDirection = velocityTangent.normalized * -1f;

        //    //Debug.DrawRay(triangleCenter, tangentialDirection * 3f, Color.black);
        //    //Debug.DrawRay(triangleCenter, velocityVec.normalized * 3f, Color.blue);
        //    //Debug.DrawRay(triangleCenter, normal * 3f, Color.white);

        //    //The speed of the triangle as if it was in the tangent's direction
        //    //So we end up with the same speed as in the center of the triangle but in the direction of the flow
        //    Vector3 v_f_vec = face.velocityMagnitude * tangentialDirection;

        //    //The final resistance force
        //    Vector3 viscousWaterResistanceForce = voxelAreaRho * v_f_vec.magnitude * v_f_vec * Cf;

        //    viscousWaterResistanceForce = CheckForceIsValid(viscousWaterResistanceForce, "Viscous Water Resistance");

        //    totalWaterDrag += viscousWaterResistanceForce;

        //    Vector3 convertedPosition = transform.localToWorldMatrix * face.position;

        //    _rb.AddForceAtPosition(viscousWaterResistanceForce, convertedPosition);
        //}

        //The Coefficient of frictional resistance - belongs to Viscous Water Resistance but is same for all so calculate once
        //public float ResistanceCoefficient(float rho, float velocity, float length)
        //{
        //    //Reynolds number

        //    // Rn = (V * L) / nu
        //    // V - speed of the body
        //    // L - length of the sumbmerged body
        //    // nu - viscosity of the fluid [m^2 / s]

        //    //Viscocity depends on the temperature, but at 20 degrees celcius:
        //    float nu = 0.000001f;
        //    //At 30 degrees celcius: nu = 0.0000008f; so no big difference

        //    //Reynolds number
        //    float Rn = (velocity * length) / nu;

        //    //The resistance coefficient
        //    float Cf = 0.075f / Mathf.Pow((Mathf.Log10(Rn) - 2f), 2f);

        //    return Cf;
        //}

        //Force 3 - Air resistance on the part of the ship above the water (typically 4 to 8 percent of total resistance)
        //public void AirResistanceForce(float rho, FaceData face, float C_air)
        //{
        //    // R_air = 0.5 * rho * v^2 * A_p * C_air
        //    // rho - air density
        //    // v - speed of ship
        //    // A_p - projected transverse profile area of ship
        //    // C_r - coefficient of air resistance (drag coefficient)

        //    //Only add air resistance if normal is pointing in the same direction as the velocity
        //    if (face.cosTheta < 0f)
        //    {
        //        return;
        //    }

        //    //Find air resistance force
        //    Vector3 airResistanceForce = voxelAreaRho * face.velocityMagnitude * face.velocity * C_air;

        //    //Acting in the opposite side of the velocity
        //    airResistanceForce *= -1f;

        //    airResistanceForce = CheckForceIsValid(airResistanceForce, "Air resistance");

        //    totalAirDrag += airResistanceForce;

        //    _rb.AddForceAtPosition(airResistanceForce, transform.TransformPoint(face.position));
        //}

        //Check that a force is not NaN
        //private Vector3 CheckForceIsValid(Vector3 force, string forceName)
        //{
        //    if (!float.IsNaN(force.x) && !float.IsNaN(force.y) && !float.IsNaN(force.z))
        //    {
        //        //Debug.Log(force + " force");
        //        return force;
        //    }
        //    else
        //    {
        //        //Debug.Log(forceName += " force is NaN");

        //        return Vector3.zero;
        //    }
        //}

        //private void UpdateFacesVelocity()
        //{
        //    for (int i = 0; i < externalVoxels.Count; i++)
        //    {
        //        for (int j = 0; j < externalVoxels[i].faces.Count; j++)
        //        {
        //            externalVoxels[i].faces[j].UpdateFaceVelocity(_rb.GetPointVelocity(transform.TransformPoint(externalVoxels[i].faces[j].position)));
        //        }
        //    }
        //}

        private void GetVelocityPoints()
        {
            for (var i = 0; i < _voxels.Length; i++) { _velocity[i] = _rb.GetPointVelocity(_samplePoints[i]); }
        }

        private void SliceIntoVoxels()
        {
            int count = 0;
            var t = transform;
            var rot = t.rotation;
            var pos = t.position;
            var size = t.localScale;
            t.SetPositionAndRotation(t.position, Quaternion.identity);
            var rawBounds = VoxelBounds();
            rawBounds.center = Vector3.zero;
            rawBounds = UnitaryBounds(rawBounds);
            //t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            t.localScale = Vector3.one;

            _voxels = null;
            var points = new List<Vector3>();

            _voxelBounds = rawBounds;
            _voxelBounds.size = RoundVector(rawBounds.size, voxelResolution);
            voxelXLength = (int)(_voxelBounds.extents.x * 2f / voxelResolution);
            voxelYLength = (int)(_voxelBounds.extents.y * 2f / voxelResolution);
            voxelZLength = (int)(_voxelBounds.extents.z * 2f / voxelResolution);

            for (var iy = -_voxelBounds.extents.y; iy < _voxelBounds.extents.y; iy += voxelResolution)
            {
                for (var ix = -_voxelBounds.extents.x; ix < _voxelBounds.extents.x; ix += voxelResolution)
                {
                    for (var iz = -_voxelBounds.extents.z; iz < _voxelBounds.extents.z; iz += voxelResolution)
                    {
                        FaceFlags flag = FaceFlags.init;

                        var x = halfVoxelResolution + ix;
                        var y = halfVoxelResolution + iy;
                        var z = halfVoxelResolution + iz;

                        var p = new Vector3(x, y, z) + _voxelBounds.center;

                        //var inside = false;
                        //foreach (var t1 in colliders)
                        //{
                        //    if (PointIsInsideCollider(t1, p, pos))
                        //    {
                        //        inside = true;
                        //        break;
                        //    }
                        //}
                        if (CheckInsideCollider(p))
                        {
                            points.Add(p);
                            //if (CheckIfVoxelIsBorder(x, y, z, ref flag))
                            //    externalVoxels.Add(new Voxel(count, new Vector3(x, y, z), flag));
                            //count++;
                        }
                    }
                }
            }

            _voxels = points.ToArray();
            submergedAmounts = new float[_voxels.Length];
            ks = new float[_voxels.Length];
            //t.SetPositionAndRotation(pos, rot);
            t.localScale = size;
            var voxelVolume = Mathf.Pow(voxelResolution, 3f) * _voxels.Length;
            var rawVolume = rawBounds.size.x * rawBounds.size.y * rawBounds.size.z;
            volume = Mathf.Min(rawVolume, voxelVolume);
            density = gameObject.GetComponent<Rigidbody>().mass / volume;
        }

        private Bounds VoxelBounds()
        {
            var bounds = new Bounds(transform.position, new Vector3());
            foreach (var nextCollider in colliders)
            {
                bounds.Encapsulate(nextCollider.bounds);
            }
            return bounds;
        }

        //private bool CheckIfVoxelIsBorder(float vx, float vy, float vz, ref FaceFlags flag)
        //{
        //    if (Mathf.Approximately(vx, -_voxelBounds.extents.x + halfVoxelResolution)) flag = flag | FaceFlags.left;
        //    if (Mathf.Approximately(vy, -_voxelBounds.extents.y + halfVoxelResolution)) flag = flag | FaceFlags.bottom;
        //    if (Mathf.Approximately(vz, -_voxelBounds.extents.z + halfVoxelResolution)) flag = flag | FaceFlags.backward;
        //    if (Mathf.Approximately(vx, _voxelBounds.extents.x - halfVoxelResolution)) flag = flag | FaceFlags.right;
        //    if (Mathf.Approximately(vy, _voxelBounds.extents.y - halfVoxelResolution)) flag = flag | FaceFlags.top;
        //    if (Mathf.Approximately(vz, _voxelBounds.extents.z - halfVoxelResolution)) flag = flag | FaceFlags.forward;

        //    if (flag != FaceFlags.init) return true;
        //    return false;
        //}

        //private void SetupFaces()
        //{
        //    for (int i = 0; i < externalVoxels.Count; i++)
        //    {
        //        FaceFlags flags = externalVoxels[i].flag;

        //        Vector3 position = new Vector3();
        //        Vector3 normal = new Vector3();

        //        if ((flags & FaceFlags.right) != 0)
        //        {
        //            position = new Vector3(_voxelBounds.extents.x, externalVoxels[i].position.y + halfVoxelResolution, externalVoxels[i].position.z + halfVoxelResolution);
        //            normal = transform.right;
        //            externalVoxels[i].faces.Add(new FaceData(normal, position, voxelResolution));
        //        }
        //        else if ((flags & FaceFlags.left) != 0)
        //        {
        //            position = new Vector3(-_voxelBounds.extents.x, externalVoxels[i].position.y + halfVoxelResolution, externalVoxels[i].position.z + halfVoxelResolution);
        //            normal = -transform.right;
        //            externalVoxels[i].faces.Add(new FaceData(normal, position, voxelResolution));
        //        }

        //        if ((flags & FaceFlags.top) != 0)
        //        {
        //            position = new Vector3(externalVoxels[i].position.x + halfVoxelResolution, _voxelBounds.extents.y, externalVoxels[i].position.z + halfVoxelResolution);
        //            normal = transform.up;
        //            externalVoxels[i].faces.Add(new FaceData(normal, position, voxelResolution));
        //        }
        //        else if ((flags & FaceFlags.bottom) != 0)
        //        {
        //            position = new Vector3(externalVoxels[i].position.x + halfVoxelResolution, -_voxelBounds.extents.y, externalVoxels[i].position.z + halfVoxelResolution);
        //            normal = -transform.up;
        //            externalVoxels[i].faces.Add(new FaceData(normal, position, voxelResolution));
        //        }

        //        if ((flags & FaceFlags.forward) != 0)
        //        {
        //            position = new Vector3(externalVoxels[i].position.x + halfVoxelResolution, externalVoxels[i].position.y + halfVoxelResolution, _voxelBounds.extents.z);
        //            normal = transform.forward;
        //            externalVoxels[i].faces.Add(new FaceData(normal, position, voxelResolution));
        //        }
        //        else if ((flags & FaceFlags.backward) != 0)
        //        {
        //            position = new Vector3(externalVoxels[i].position.x + halfVoxelResolution, externalVoxels[i].position.y + halfVoxelResolution, -_voxelBounds.extents.z);
        //            normal = -transform.forward;
        //            externalVoxels[i].faces.Add(new FaceData(normal, position, voxelResolution));
        //        }
        //    }
        //}

        private bool CheckInsideCollider(Vector3 position)
        {
            foreach (var t1 in colliders)
            {
                if (PointIsInsideCollider(t1, position, transformPosition))
                {
                    return true;
                }
            }
            return false;
        }

        private Bounds UnitaryBounds(Bounds bounds)
        {
            Vector3 scaling = new Vector3(1 / (bounds.size.x), 1 / (bounds.size.y), 1 / (bounds.size.z));
            var min = bounds.min;
            min.Scale(scaling);
            var max = bounds.max;
            max.Scale(scaling);
            bounds.SetMinMax(min, max);
            return bounds;
        }

        private static Vector3 RoundVector(Vector3 vec, float rounding)
        {
            return new Vector3(Mathf.Ceil(vec.x / rounding) * rounding, Mathf.Ceil(vec.y / rounding) * rounding, Mathf.Ceil(vec.z / rounding) * rounding);
        }

        private bool PointIsInsideCollider(Collider c, Vector3 p, Vector3 position)
        {
            var cp = Physics.ClosestPoint(p, c, Vector3.zero, Quaternion.identity);
            return Vector3.Distance(cp, p) < 0.01f;
        }

        private void SetupPhysical()
        {
            if (!TryGetComponent(out _rb))
            {
                _rb = gameObject.AddComponent<Rigidbody>();
                Debug.LogError($"Buoyancy:Object \"{name}\" had no Rigidbody. Rigidbody has been added.");
            }
            _rb.centerOfMass = centerOfMass + _voxelBounds.center;
            _baseDrag = _rb.drag;
            _baseAngularDrag = _rb.angularDrag;

            _velocity = new float3[_voxels.Length];
            var archimedesForceMagnitude = WaterDensity * Mathf.Abs(Physics.gravity.y) * volume;
            _localArchimedesForce = new float3(0, archimedesForceMagnitude, 0) / _voxels.Length;
            LocalToWorldJob.SetupJob(_guid, _voxels, ref _samplePoints);
        }

        private void OnDrawGizmosSelected()
        {
            const float gizmoSize = 0.05f;
            var t = transform;
            var matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

            if (_voxels != null)
            {
                Gizmos.color = Color.yellow;

                foreach (var p in _voxels)
                {
                    Gizmos.DrawCube(p, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                }
            }

            Gizmos.matrix = matrix;
            if (voxelResolution >= 0.1f)
            {
                Gizmos.DrawWireCube(_voxelBounds.center, _voxelBounds.size);
                Vector3 center = _voxelBounds.center;
                float y = center.y - _voxelBounds.extents.y;
                for (float x = -_voxelBounds.extents.x; x < _voxelBounds.extents.x; x += voxelResolution)
                {
                    Gizmos.DrawLine(new Vector3(x, y, -_voxelBounds.extents.z + center.z), new Vector3(x, y, _voxelBounds.extents.z + center.z));
                }
                for (float z = -_voxelBounds.extents.z; z < _voxelBounds.extents.z; z += voxelResolution)
                {
                    Gizmos.DrawLine(new Vector3(-_voxelBounds.extents.x, y, z + center.z), new Vector3(_voxelBounds.extents.x, y, z + center.z));
                }
            }
            else
                _voxelBounds = VoxelBounds();

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_voxelBounds.center + centerOfMass, 0.2f);

            Gizmos.matrix = Matrix4x4.identity; Gizmos.matrix = Matrix4x4.identity;

            if (_debugInfo != null)
            {
                foreach (DebugDrawing debug in _debugInfo)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(debug.Position, new Vector3(gizmoSize, gizmoSize, gizmoSize)); // drawCenter
                    var water = debug.Position;
                    water.y = debug.WaterHeight;
                    Gizmos.DrawLine(debug.Position, water); // draw the water line
                    Gizmos.DrawSphere(water, gizmoSize * 4f);
                    if (_buoyancyType == BuoyancyType.Physical || _buoyancyType == BuoyancyType.PhysicalVoxel)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(debug.Position, debug.Force / _rb.mass); // draw force
                    }
                }
            }

        }

        private struct DebugDrawing
        {
            public Vector3 Force;
            public Vector3 Position;
            public float WaterHeight;
        }

        public enum BuoyancyType
        {
            NonPhysical,
            NonPhysicalVoxel,
            Physical,
            PhysicalVoxel
        }

        [Flags]
        public enum FaceFlags
        {
            init = 0,
            right = 1,
            left = 2,
            top = 4,
            bottom = 8,
            forward = 16,
            backward = 32
        }

        public class Voxel
        {
            public int index;
            public Vector3 position;
            public List<FaceData> faces;
            public FaceFlags flag;

            public Voxel(int index, Vector3 position, FaceFlags flag)
            {
                this.index = index;
                this.position = position;
                faces = new List<FaceData>();
                this.flag = flag;
            }
        }

        public class FaceData
        {
            //The center of the triangle
            public Vector3 position;

            //The normal to the triangle
            public Vector3 normal;

            public float normalMagnitude;

            //The area of the triangle
            public float area;

            //The velocity normalized
            public Vector3 velocityDir;

            public Vector3 velocity;

            public float velocityMagnitude;

            //The angle between the normal and the velocity
            //Negative if pointing in the opposite direction
            //Positive if pointing in the same direction
            public float cosTheta;

            public FaceData(Vector3 normal, Vector3 position, float voxelResolution)
            {
                this.position = position;

                //Normal to the triangle
                this.normal = normal.normalized;

                normalMagnitude = normal.magnitude;

                this.velocity = new Vector3(0f, 0f, 0f);

                this.velocityDir = new Vector3(0f, 0f, 0f);

                velocityMagnitude = 0f;

                //Area of the triangle
                this.area = voxelResolution * voxelResolution;

                //Angle between the normal and the velocity
                //Negative if pointing in the opposite direction
                //Positive if pointing in the same direction
                this.cosTheta = 0;
            }

            public void UpdateFaceVelocity(Vector3 velocity)
            {
                this.velocity = velocity;
                velocityMagnitude = velocity.magnitude;
                velocityDir = velocity.normalized;
                cosTheta = Vector3.Dot(velocityDir, normal);
            }
        }
    }
}