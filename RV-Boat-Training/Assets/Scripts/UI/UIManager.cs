using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public List<CanvasGroup> canvasGroups;
    private int activePanel = 1;

    public CanvasGroup GetPreviousPanel()
    {
        if (activePanel > 0)
            return canvasGroups[--activePanel];
        else
            return null;
    }

    public CanvasGroup GetNextPanel()
    {
        if (activePanel < canvasGroups.Count - 1)
            return canvasGroups[++activePanel];
        else
            return null;
    }

    public CanvasGroup GetActivePanel()
    {
        return canvasGroups[activePanel];
    }

    public int GetActivePanelNum()
    {
        return activePanel;
    }

    public void ResetPanels(CanvasGroup cvnGroup)
    {
        int panelNum = GetPanelIndex(cvnGroup);

        canvasGroups[1].alpha = 1;
        for (int i = 2; i < canvasGroups.Count; i++)
            canvasGroups[i].alpha = 0;

        activePanel = panelNum;

        if (panelNum == 7)
        {
            activePanel = 1;
        }
    }

    private int GetPanelIndex(CanvasGroup cvnGroup)
    {
        for (int i = 0; i < canvasGroups.Count; i++)
            if (canvasGroups[i] == cvnGroup) return i;

        return -1;
    }
}
