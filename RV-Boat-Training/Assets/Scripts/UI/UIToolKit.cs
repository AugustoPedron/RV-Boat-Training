using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToolKit : MonoBehaviour
{
    public Button referenceButton;
    public Color color;
    public Color normalColor;
    public Color highlightenedColor;
    public Color pressedColor;
    public TMP_FontAsset font;
    public Canvas menu;

    public void ChangeButtonsStyle()
    {
        if (referenceButton != null && menu != null)
        {
            //ColorBlock cb = referenceButton.colors;
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = normalColor;
            cb.highlightedColor = highlightenedColor;
            cb.pressedColor = pressedColor;

            //Color c = referenceButton.GetComponent<Image>().color;
            //TMP_FontAsset f = referenceButton.GetComponentInChildren<TMP_Text>().font;
            Button[] buttons = menu.GetComponentsInChildren<Button>();

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].colors = cb;
                buttons[i].GetComponent<Image>().color = color;
                buttons[i].GetComponentInChildren<TMP_Text>().font = font;
            }
        }
    }

    public void ChangeTextsStyle()
    {
        if (font != null && menu != null)
        {
            TMP_Text[] texts = menu.GetComponentsInChildren<TMP_Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].GetComponentInParent<Button>() == null)
                {
                    texts[i].font = font;
                }
            }
        }
    }

}
