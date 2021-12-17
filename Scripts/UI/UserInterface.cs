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

    [SerializeField] private bool _useUI;

    private RawImage _backGround;

    void Awake()
    {
        StartCoroutine("AwaitEndOfFrame");

        _backGround = GetComponentInChildren<RawImage>();

        _backGround.gameObject.SetActive(_useUI);
        if (!_useUI)
        {
            _text_1.text = "";
            _text_2.text = "";
            _text_3.text = "";
            _text_4.text = "";
        }
    }

    IEnumerator AwaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        MainManager.Instance.UI = this;
        Debug.Log($"{Globe.RunOrder} - UserInterface is set");
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (_backGround != null) _backGround.gameObject.SetActive(_useUI);

        if (!_useUI)
        {
            _text_1.text = "";
            _text_2.text = "";
            _text_3.text = "";
            _text_4.text = "";
        }
    }
    public void Text(int displayID, string text)
    {
        if (!_useUI) return;

        switch (displayID)
        {
            case 0: _text_1.text = text; break;
            case 1: _text_2.text = text; break;
            case 2: _text_3.text = text; break;
            case 3: _text_4.text = text; break;
        }
    }
}

