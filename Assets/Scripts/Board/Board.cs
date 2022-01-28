using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Board : MonoBehaviourPun
{
    public delegate void BreakBoard(GameObject _gameObject);
    public BreakBoard breakBoard = null;

    [SerializeField]
    private Transform rotateTr = null;
    private float rotateSpeed = 8.0f;

    private float destroyTime = 3.0f;

    public void StartBoardRPC(string _name)
    {
        photonView.RPC(_name, RpcTarget.All);
    }

    [PunRPC]
    public void BreakBoardRPC()
    {
        Destroy(transform.parent.parent.gameObject,destroyTime);
    }

    public float t = 0;

    public bool isPush = false;
    public bool isDown = false;

    // Update is called once per frame
    void Update()
    {
        if (isPush)
        {
            t += Time.deltaTime * rotateSpeed;
            if (t >= 1)
            {
                isDown = true;
                
                isPush = false;
                
            }
            float angle = Mathf.Lerp(-2f, 45f, t);
            rotateTr.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
