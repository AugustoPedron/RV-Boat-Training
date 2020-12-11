using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFader : MonoBehaviour
{
    public float duration = 0.4f;
    public CanvasGroup cvnGroup;

    public void Fade(CanvasGroup nextPanel)
    {
        StartCoroutine(DoFade(nextPanel, cvnGroup.alpha, 0));
        cvnGroup.blocksRaycasts = false;
        nextPanel.blocksRaycasts = true;
    }

    private IEnumerator DoFade(CanvasGroup nextPanel, float start, float end)
    {
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            cvnGroup.alpha = Mathf.Lerp(start, end, counter / duration);

            yield return null;
        }

        counter = 0f;

        float nextPanelStart = nextPanel.alpha;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            nextPanel.alpha = Mathf.Lerp(nextPanelStart, 1, counter / duration);

            yield return null;
        }
    }
}
