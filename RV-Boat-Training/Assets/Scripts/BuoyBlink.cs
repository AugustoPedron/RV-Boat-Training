using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyBlink : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Material YellowMaterial;
    public Material GreenMaterial;
    private bool blink = true;
    private bool on = true;
    private int id;

    private void OnEnable()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (blink)
        {
            if (on)
            {
                YellowMaterial.DisableKeyword("_EMISSION");
                on = false;
            }
            else
            {
                YellowMaterial.EnableKeyword("_EMISSION");
                on = true;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void ChangeColor()
    {
        blink = false;
        StopCoroutine(Blink());
        meshRenderer.material = GreenMaterial;
    }
}
