using System.Collections;
using TMPro;
using UnityEngine;

public class AboutProgramm : MonoBehaviour
{
    [SerializeField, TextArea(10, 10)]
    private string _aboutProgrammText;

    [SerializeField] private TMP_Text _aboutProgramm;
    [SerializeField] private TMP_Text _myProfille;
    [SerializeField] private float _timeToSetChar;

    private void Start()
    {
        _myProfille.gameObject.SetActive(false);

        StartCoroutine(AnimateText(_timeToSetChar));
    }

    IEnumerator AnimateText(float charSpeed)
    {
        _aboutProgramm.text = "";

        foreach (char ch in _aboutProgrammText)
        {
            _aboutProgramm.text += ch;
            yield return new WaitForSeconds(charSpeed);
        }

        _myProfille.gameObject.SetActive(true);
        _myProfille.text = "Захар Костюткин в <link=\"https://t.me/huildo\"><color=#0000FF>телеграмм</color></link>.";

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_myProfille.gameObject.activeSelf)  
            {
                CheckLinkClick(_myProfille);
            }
        }
    }

    private void CheckLinkClick(TMP_Text textObject)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textObject, Input.mousePosition, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textObject.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();
            Application.OpenURL(url);
        }
    }
}
