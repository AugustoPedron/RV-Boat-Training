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
    public GameObject EndNavigationPanel;

    private int lastBuoy = 0;
    [SerializeField]
    private float maxDistanceFromPath = 80f;
    private float maxDistanceFromPathSqr = 0f;
    [SerializeField]
    private float maxAngle = 70f;
    [SerializeField]
    private List<GameObject> YellowBuoys = new List<GameObject>();
    [SerializeField]
    private List<Buoy> buoyBlinks = new List<Buoy>();
    [SerializeField]
    private Transform arrowTransform;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < YellowBuoys.Count; i++)
        {
            Gizmos.color = Color.green;
            Vector3 forward = YellowBuoys[i].transform.forward.normalized * directionLenght;
            Gizmos.DrawRay(YellowBuoys[i].transform.position, forward);

            Gizmos.DrawRay(YellowBuoys[i].transform.position, Quaternion.Euler(0, 70, 0) * forward);
            Gizmos.DrawRay(YellowBuoys[i].transform.position, Quaternion.Euler(0, -70, 0) * forward);
        }
    }

    private void OnEnable()
    {
        BoatEventManager.StartListening("buoyReached", BuoyReached);
    }

    private void OnDisable()
    {
        BoatEventManager.StopListening("buoyReached", BuoyReached);
    }

    private void Start()
    {
        Init();

        Vector3 arrowPosition = arrowTransform.position;
        Vector3 direction = boatTransform.position - YellowBuoys[0].transform.position;
        arrowTransform.position = new Vector3();
        arrowTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        arrowTransform.position = arrowPosition;

        StartCoroutine(EnableRemainingAudio());
    }

    private void Update()
    {
        if (!PauseMenu.gameIsPaused)
        {
            if (lastBuoy < YellowBuoys.Count)
            {
                Vector3 direction = boatTransform.position - YellowBuoys[lastBuoy].transform.position;
                arrowTransform.rotation = arrowTransform.rotation * Quaternion.LookRotation(direction, Vector3.up);
                if (CheckDistanceFromPath())
                {
                    //Debug.Log("wrongDirection");
                    //riprodurre audio di avviso per l'allontanamento dal percorso
                }
            }

            if (lastBuoy == YellowBuoys.Count)
            {
                EndNavigation();
                lastBuoy++;
            }
        }
    }

    public void Create()
    {
        Reset();
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Vector3 boxColliderSize = new Vector3(maxDistanceFromPath * 2, 25, 0.1f);

        transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);

        GameObject instance;

        for (int i = 1; i < path.m_Waypoints.Length; i++)
        {

            if (Application.isEditor)
            {
                instance = Instantiate(YellowBuoyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.position = path.m_Waypoints[i - 1].position;
                instance.GetComponent<BoxCollider>().size = boxColliderSize;
                if (i == 1)
                {
                    instance.GetComponent<AudioSource>().enabled = true;
                    instance.GetComponent<BoxCollider>().enabled = true;
                }
                YellowBuoys.Add(instance);
                buoyBlinks.Add(instance.GetComponentInChildren<Buoy>());
            }
        }

        instance = Instantiate(YellowBuoyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        instance.transform.SetParent(transform);
        instance.transform.position = path.m_Waypoints[path.m_Waypoints.Length - 1].position;
        instance.GetComponent<BoxCollider>().size = boxColliderSize;
        YellowBuoys.Add(instance);
        buoyBlinks.Add(instance.GetComponentInChildren<Buoy>());

        Init();

        transform.SetPositionAndRotation(position, rotation);
    }

    private void Reset()
    {
        for (int i = 0; i < YellowBuoys.Count; i++)
        {
            DestroyImmediate(YellowBuoys[i]);
        }

        YellowBuoys.Clear();
        buoyBlinks.Clear();
    }

    IEnumerator EnableRemainingAudio()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 1; i < YellowBuoys.Count; i++)
        {
            YellowBuoys[i].GetComponent<AudioSource>().enabled = true;
        }

        yield break;
    }

    private void EndNavigation()
    {
        EndNavigationPanel.SetActive(true);
        BoatEventManager.TriggerEvent("endNavigation");
    }

    private void Init()
    {
        Vector3 direction = new Vector3();

        for (int i = 0; i < YellowBuoys.Count - 1; i++)
        {
            direction = -YellowBuoys[i].transform.position + YellowBuoys[i + 1].transform.position;
            YellowBuoys[i].transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        YellowBuoys[YellowBuoys.Count - 1].transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        maxDistanceFromPathSqr = maxDistanceFromPath * maxDistanceFromPath;
    }

    private void BuoyReached()
    {
        buoyBlinks[lastBuoy].ChangeColor();
        YellowBuoys[lastBuoy].GetComponent<BoxCollider>().enabled = false;
        lastBuoy++;
        if (lastBuoy < YellowBuoys.Count) YellowBuoys[lastBuoy].GetComponent<BoxCollider>().enabled = true;

        if (lastBuoy == YellowBuoys.Count) BoatEventManager.StopListening("buoyReached", BuoyReached);
    }

    //controllo sull'allontanamento dal percorso indicato
    private bool CheckDistanceFromPath()
    {
        Vector3 direction = (YellowBuoys[lastBuoy].transform.position + gameObject.transform.position) - boatTransform.position;
        Vector3 boatForward = boatTransform.forward;

        float angle = Vector3.Angle(direction, boatForward);

        if (angle > 70 && direction.sqrMagnitude >= maxDistanceFromPathSqr)
            return true;

        return false;
    }
}
