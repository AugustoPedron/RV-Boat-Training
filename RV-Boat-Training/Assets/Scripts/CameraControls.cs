using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraControls : MonoBehaviour
{
    public Canvas canvas;
    public float mouseSpeed = 2f;
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private float pitch = 0f;
    private float yaw = 0f;


    // Start is called before the first frame update
    void Start()
    {
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = canvas.GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        pitch -= Input.GetAxis("Mouse Y") * mouseSpeed;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        yaw += Input.GetAxis("Mouse X") * mouseSpeed;
        yaw = yaw % 360f;
        transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
