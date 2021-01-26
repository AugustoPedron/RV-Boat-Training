using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToolKit : MonoBehaviour
{
    public Button referenceButton;

    public TMP_FontAsset fontForText;
    public Canvas menu;

    public void ChangeButtonsStyle()
    {
        if (referenceButton != null && menu != null)
        {
            ColorBlock cb = referenceButton.colors;
            Color c = referenceButton.GetComponent<Image>().color;
            TMP_FontAsset f = referenceButton.GetComponentInChildren<TMP_Text>().font;
            Button[] buttons = menu.GetComponentsInChildren<Button>();

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].colors = cb;
                buttons[i].GetComponent<Image>().color = c;
                buttons[i].GetComponentInChildren<TMP_Text>().font = f;
            }
        }
    }

    public void ChangeTextsStyle()
    {
        if (fontForText != null && menu != null)
        {
            TMP_Text[] texts = menu.GetComponentsInChildren<TMP_Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].GetComponentInParent<Button>() == null)
                {
                    texts[i].font = fontForText;
                }
            }
        }
    }

}
