using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Misc.SceneManagment
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _loading;

        public void ChangeScene(string sceneName)
        {
            StartCoroutine(LoadSceneWithDelay(sceneName));
        }

        private IEnumerator LoadSceneWithDelay(string sceneName)
        {
            _loading.SetActive(true);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            _loading.SetActive(false);
        }
    }
}