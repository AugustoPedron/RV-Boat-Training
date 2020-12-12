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
}
