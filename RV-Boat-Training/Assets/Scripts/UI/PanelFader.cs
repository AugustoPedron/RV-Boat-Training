﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFader : MonoBehaviour
{
    public float duration = 0.4f;
    public CanvasGroup cvnGroup;
    [SerializeField]
    private UIManager uiManager;

    public void FadeNext(CanvasGroup nextPanel)
    {
        StartCoroutine(DoFade(nextPanel, cvnGroup.alpha, 0));
        cvnGroup.blocksRaycasts = false;
        nextPanel.blocksRaycasts = true;
    }

    public void FadeNext()
    {
        CanvasGroup activePanel = uiManager.GetActivePanel();
        CanvasGroup nextPanel = uiManager.GetNextPanel();
        StartCoroutine(DoFade(activePanel, nextPanel, activePanel.alpha, 0));
        activePanel.blocksRaycasts = false;
        nextPanel.blocksRaycasts = true;
    }

    public void FadePrevious()
    {
        CanvasGroup activePanel = uiManager.GetActivePanel();
        CanvasGroup nextPanel = uiManager.GetPreviousPanel();
        if (uiManager.GetActivePanelNum() == 0) activePanel = cvnGroup;
        StartCoroutine(DoFade(activePanel, nextPanel, activePanel.alpha, 0));
        activePanel.blocksRaycasts = false;
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

    private IEnumerator DoFade(CanvasGroup activePanel, CanvasGroup nextPanel, float start, float end)
    {
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            activePanel.alpha = Mathf.Lerp(start, end, counter / duration);

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
