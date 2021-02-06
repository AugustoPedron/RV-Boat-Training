using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public CanvasGroup previousCanvas;
    public CanvasGroup nextCanvas;
    public CanvasGroup actualCanvas;
    public List<CanvasGroup> canvasGroups;
    private int activePanel = 0;

    public CanvasGroup GetPreviousPanel()
    {
        if (activePanel > 0)
        {
            return canvasGroups[--activePanel];
        }
        else
        {
            activePanel = 0;
            return previousCanvas;
        }            
    }

    public CanvasGroup GetNextPanel()
    {
        if (activePanel < canvasGroups.Count - 1)
        {    
            return canvasGroups[++activePanel];
        }
        else {
            activePanel = 0;
            return nextCanvas;
        }
    }

    public CanvasGroup GetActivePanel(bool flag)
    {
        if (flag && activePanel == 0)
        {
            return canvasGroups[activePanel];
        }
        else if (flag && activePanel == canvasGroups.Count - 1)
        {
            return actualCanvas;
        }
        else if(!flag && activePanel == 0)
        {
            return actualCanvas;
        }
        else
            return canvasGroups[activePanel];
    }

    public int GetActivePanelNum()
    {
        return activePanel;
    }

    public void ResetPanels()
    {
        canvasGroups[0].alpha = 1;
        for (int i = 1; i < canvasGroups.Count; i++)
            canvasGroups[i].alpha = 0;

        activePanel = 0;
    }

    public void ChangePreviousCanvas(CanvasGroup canvas) { previousCanvas = canvas; }
}
