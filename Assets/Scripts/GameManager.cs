using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //�̱��� ������ ����ϱ� ���� ��Ʈ���� ����
    public static GameManager instance = null;

    //�ν��Ͻ��� �����ϱ� ���� ������Ƽ
    public static GameManager Instance
    {
        get
        {
            //�ν��Ͻ��� ���� ��쿡 �����Ϸ� �ϸ� �ν��Ͻ��� �Ҵ����ش�.
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
        //�ν��Ͻ��� �����ϴ� ��� ���λ���� �ν��Ͻ��� �����Ѵ�.
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

