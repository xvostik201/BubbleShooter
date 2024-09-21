using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScript : MonoBehaviour
{
    AsyncOperation _asyncOperation;

    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private Slider _progBar;
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private float _sliderTowardsSpeed = 0.2f;

    public void ChooseScene(int sceneIndex)
    {
        _loadingScreen.SetActive(true);
        _progBar.gameObject.SetActive(true);
        _loadingText.gameObject.SetActive(true);
        _loadingText.text = "Загрузка...";
        StartCoroutine(LoadScenelWithRealProgress(sceneIndex));
        AudioManager.instance.PlayButtonClick();
    }

    IEnumerator LoadScenelWithRealProgress(int sceneIndex)
    {
        yield return new WaitForSeconds(1);
        _asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        _asyncOperation.allowSceneActivation = false;

        while (!_asyncOperation.isDone)
        {
            if (_asyncOperation.progress >= 0.9f)
            {
                _progBar.value = Mathf.MoveTowards(_progBar.value, 1f, _sliderTowardsSpeed * Time.deltaTime);

                if (_progBar.value >= 0.95f)
                {
                    _loadingText.text = "Нажмите 'Пробел', чтобы начать игру";

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        _asyncOperation.allowSceneActivation = true;
                    }
                }
            }
            else
            {
                _progBar.value = _asyncOperation.progress;
            }

            Debug.Log($"Текущий прогресс: {_asyncOperation.progress * 100}%");
            yield return null;
        }
    }
}
