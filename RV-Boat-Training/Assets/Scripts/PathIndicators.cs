using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
public class PathIndicators : MonoBehaviour
{
    public CinemachineSmoothPath path;
    public List<Vector3> directions = new List<Vector3>();
    public List<Vector3> ortogonalDirections = new List<Vector3>();

    void Start()
    {
        for (int i = 1; i < path.m_Waypoints.Length; i++)
        {
            Vector3 direction = path.m_Waypoints[i].position - path.m_Waypoints[i - 1].position;
            direction = new Vector3(direction.x, 0, direction.z);
            directions.Add(direction.normalized);
            direction = Quaternion.Euler(0, 90, 0) * direction;
            ortogonalDirections.Add(direction.normalized);
        }
    }

    void Update()
    {
        
    }
}
