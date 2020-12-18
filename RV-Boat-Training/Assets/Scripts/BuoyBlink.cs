using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyBlink : MonoBehaviour
{
    public Material material;
    private bool on = true;

    private void OnEnable()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            if (on)
            {
                material.DisableKeyword("_EMISSION");
                on = false;
            }
            else
            {
                material.EnableKeyword("_EMISSION");
                on = true;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
