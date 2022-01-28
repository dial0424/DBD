using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //싱글톤 패턴을 사용하기 위한 인트선스 변수
    public static GameManager instance = null;

    //인스턴스에 접근하기 위한 프로퍼티
    public static GameManager Instance
    {
        get
        {
            //인스턴스가 없는 경우에 접근하려 하면 인스턴스를 할당해준다.
            if (!instance)
            {
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        //인스턴스가 존재하는 경우 새로생기는 인스턴스를 삭제한다.
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void ClickRestartBtn()
    {
        SceneManager.LoadScene(0);
    }

    public void ClickExitBtn()
    {
        Application.Quit();
    }
}

