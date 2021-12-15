using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [SerializeField] private Text _text_1;
    [SerializeField] private Text _text_2;
    [SerializeField] private Text _text_3;
    [SerializeField] private Text _text_4;

    void Awake()
    {
        StartCoroutine("AwaitEndOfFrame");
    }

    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.UI = this;
        Debug.Log($"{Globe.RunOrder} - UserInterface is set");
    }

    public void Text(int displayID, string text)
    {
        switch (displayID)
        {
            case 0: _text_1.text = text; break;
            case 1: _text_2.text = text; break;
            case 2: _text_3.text = text; break;
            case 3: _text_4.text = text; break;
        }
    }
}

