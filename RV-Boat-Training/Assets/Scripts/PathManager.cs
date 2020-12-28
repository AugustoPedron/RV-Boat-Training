using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class PathManager : MonoBehaviour
{
    public float directionLenght = 10;
    public CinemachineSmoothPath path;
    public GameObject YellowBuoyPrefab;
    public Transform boatTransform;

    private int lastBuoy = 0;
    [SerializeField]
    private List<GameObject> YellowBuoys = new List<GameObject>();
    [SerializeField]
    private List<BuoyBlink> buoyBlinks = new List<BuoyBlink>();
    [SerializeField]
    private List<Vector3> directions = new List<Vector3>(); //direzione tra coppie di boe
    [SerializeField]
    private List<Segment> segments = new List<Segment>(); //direzione ortogonale alla direzione tra coppie di boe, usata per calcolare se la barca ha superato la boa
    [SerializeField]
    private Transform arrowTransform;

    //disegna la direzione tra coppie di boe a la ortogonale a tale direzione per debug
    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            Gizmos.color = Color.green;
            Vector3 direction = segments[i].ab.normalized * directionLenght;
            Gizmos.DrawRay(YellowBuoys[i].transform.position, direction);
        }

        for (int i = 0; i < directions.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(YellowBuoys[i].transform.position, directions[i] * directionLenght);
        }
    }

    private void Start()
    {
        Vector3 arrowPosition = arrowTransform.position;
        Vector3 direction = boatTransform.position - YellowBuoys[0].transform.position;
        arrowTransform.position = new Vector3();
        arrowTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        arrowTransform.position = arrowPosition;
    }

    private void Update()
    {
        if (lastBuoy < YellowBuoys.Count)
        {
            Vector3 direction = boatTransform.position - YellowBuoys[lastBuoy].transform.position;
            arrowTransform.rotation = arrowTransform.rotation * Quaternion.LookRotation(direction, Vector3.up);
        }

        if (lastBuoy < segments.Count && !segments[lastBuoy].isLeft(boatTransform.position))
        {
            buoyBlinks[lastBuoy].ChangeColor();
            lastBuoy++;
        }
    }

    public void Create()
    {
        Reset();
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);

        GameObject instance;
        Vector3 direction = new Vector3();

        for (int i = 1; i < path.m_Waypoints.Length; i++)
        {
            direction = path.m_Waypoints[i].position - path.m_Waypoints[i - 1].position;
            direction = new Vector3(direction.x, 0, direction.z);
            directions.Add(rotation * direction.normalized);
            direction = rotation * Quaternion.Euler(0, 90, 0) * direction;
            segments.Add(new Segment(direction.normalized, path.m_Waypoints[i - 1].position, rotation));

            if (Application.isEditor)
            {
                instance = Instantiate(YellowBuoyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.position = path.m_Waypoints[i - 1].position;
                YellowBuoys.Add(instance);
                buoyBlinks.Add(instance.GetComponentInChildren<BuoyBlink>());
            }
        }

        segments.Add(new Segment(direction.normalized, path.m_Waypoints[path.m_Waypoints.Length - 1].position, rotation));

        instance = Instantiate(YellowBuoyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        instance.transform.SetParent(transform);
        instance.transform.position = path.m_Waypoints[path.m_Waypoints.Length - 1].position;
        YellowBuoys.Add(instance);
        buoyBlinks.Add(instance.GetComponentInChildren<BuoyBlink>());

        transform.SetPositionAndRotation(position, rotation);
    }

    private void Reset()
    {
        for (int i = 0; i < YellowBuoys.Count; i++)
        {
            DestroyImmediate(YellowBuoys[i]);
        }

        YellowBuoys.Clear();
        directions.Clear();
        segments.Clear();
        buoyBlinks.Clear();
    }

    [Serializable]
    private struct Segment
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 ab;

        public Segment(Vector3 a, Vector3 center, Quaternion rotation)
        {
            this.a = a + rotation * center;
            b = (2 * a) + rotation * center;
            ab = a;
        }

        public bool isLeft(Vector3 point)
        {
            return Vector3.Cross(ab, point - a).y > 0;
        }
    }
}
