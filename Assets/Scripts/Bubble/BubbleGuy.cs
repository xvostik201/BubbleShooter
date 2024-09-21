using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BubbleGuy : MonoBehaviour
{
    [SerializeField, TextArea(10, 10)] private string _winText;
    [SerializeField, TextArea(10, 10)] private string _loseText;

    [SerializeField] private float _charSpeed;
    [SerializeField] private TMP_Text _text;

    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _openMounthSprite;

    [SerializeField] private Image _image;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _aboutProgrammBut;

    private bool _isWin;
    private bool _isFirstState = true;

    void Start()
    {
    }

    void Update()
    {
        
    }

    public void SetWinStatus(bool isWin)
    {
        if (!_isFirstState) return;
        _isFirstState = false;
        gameObject.SetActive(true);
        _isWin = isWin;
        if (_isWin)
        {
            StartCoroutine(AnimateText(_charSpeed, _winText));
        }
        else
        {
            StartCoroutine(AnimateText(_charSpeed, _loseText));

        }
    }
    IEnumerator AnimateText(float charSpeed, string text)
    {
        _text.text = "";

        foreach (char ch in text)
        {
            _text.text += ch;
            yield return new WaitForSeconds(charSpeed);

        }
        _image.sprite = _openMounthSprite;
        if (!_isWin) _restartButton.gameObject.SetActive(true);
        else { _aboutProgrammBut.gameObject.SetActive(true);}
    }
}
