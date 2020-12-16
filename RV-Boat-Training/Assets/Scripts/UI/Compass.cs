using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform boatTransform;
    public RectTransform transform;
    private Vector2 anchorCenter = new Vector2(0.5f, 0.5f);
    private float rotation = 0f;

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        rotation = -boatTransform.rotation.eulerAngles.y;
        transform.pivot = anchorCenter;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
}
