using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviourPun
{
    static string nextScene = null;

    private float sceneLoad = 0;

    [SerializeField] private Image loadingImage = null;

    public static void LoadScene(string _name)
    {
        nextScene = _name;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        Debug.LogError("로딩씬 코루틴 시작");
        StartCoroutine(LoadSceneProgress());
    }

    IEnumerator LoadSceneProgress()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("GameScene");

        asyncOperation.allowSceneActivation = false;

        float timer = 0f;
        while (!asyncOperation.isDone)
        {
            yield return null;

            if (asyncOperation.progress < 0.9f)
            {
                loadingImage.fillAmount = asyncOperation.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                loadingImage.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                if (loadingImage.fillAmount >= 1f)
                {

                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (sceneLoad >= 2)
                        {
                            Debug.LogError("마스터 씬 로드");
                            yield return new WaitForSeconds(4f);
                            asyncOperation.allowSceneActivation = true;
                            yield break;
                        }
                    }

                    else
                    {
                        asyncOperation.allowSceneActivation = true;
                        photonView.RPC("LoadingSuccess", RpcTarget.MasterClient);
                        Debug.LogError("마스터 아님 씬 로드");
                        yield break;
                    }
                }
            }
        }
    }

    [PunRPC]
    private void LoadingSuccess()
    {
        sceneLoad++;
    }
}
