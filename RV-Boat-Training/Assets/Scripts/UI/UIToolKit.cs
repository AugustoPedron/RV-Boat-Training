using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToolKit : MonoBehaviour
{
    public Color color;
    public Color normalColor;
    public Color highlightenedColor;
    public Color pressedColor;
    public Color textColor;
    public TMP_FontAsset font;
    public Canvas menu;

    public void ChangeButtonsStyle()
    {
        if (CheckNullButton())
        {
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = normalColor;
            cb.highlightedColor = highlightenedColor;
            cb.pressedColor = pressedColor;

            Button[] buttons = menu.GetComponentsInChildren<Button>();

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].colors = cb;
                buttons[i].GetComponent<Image>().color = color;
                buttons[i].GetComponentInChildren<TMP_Text>().font = font;
                buttons[i].GetComponentInChildren<TMP_Text>().color = textColor;
            }
        }
    }

    public void ChangeTextsStyle()
    {
        if (CheckNullText())
        {
            TMP_Text[] texts = menu.GetComponentsInChildren<TMP_Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].GetComponentInParent<Button>() == null)
                {
                    texts[i].font = font;
                    texts[i].color = textColor;
                }
            }
        }
    }

    private bool CheckNullButton()
    {
        return menu != null && color != null && normalColor != null && highlightenedColor != null && pressedColor != null && font != null && textColor != null ? true : false;
    }

    private bool CheckNullText()
    {
        return menu != null && font != null && textColor != null ? true : false;
    }
}
