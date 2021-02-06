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
    private List<GameObject> YellowBuoys = new List<GameObject>();
    [SerializeField]
    private List<Buoy> buoyBlinks = new List<Buoy>();
    [SerializeField]
    private Transform arrowTransform;

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
        transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);

        GameObject instance;

        for (int i = 1; i < path.m_Waypoints.Length; i++)
        {

            if (Application.isEditor)
            {
                instance = Instantiate(YellowBuoyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.transform.position = path.m_Waypoints[i - 1].position;
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
            direction = YellowBuoys[i].transform.position - YellowBuoys[i + 1].transform.position;
            YellowBuoys[i].transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        YellowBuoys[YellowBuoys.Count-1].transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void BuoyReached()
    {
        buoyBlinks[lastBuoy].ChangeColor();
        YellowBuoys[lastBuoy].GetComponent<BoxCollider>().enabled = false;
        lastBuoy++;
        if(lastBuoy < YellowBuoys.Count) YellowBuoys[lastBuoy].GetComponent<BoxCollider>().enabled = true;

        if (lastBuoy == YellowBuoys.Count) BoatEventManager.StopListening("buoyReached", BuoyReached);
    }
}
