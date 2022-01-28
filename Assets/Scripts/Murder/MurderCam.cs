using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class MurderCam : MonoBehaviourPun
{
    [SerializeField] private float turnSpeed = 0f;
    private float updownRotate = 0f;
    private float camRange = 45f;
    void Start()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(photonView.IsMine)
            UpdownCamera();
    }

    void UpdownCamera()
    {
        float updown = -Input.GetAxis("Mouse Y") * turnSpeed;
        updownRotate = Mathf.Clamp(updown + updownRotate, -camRange, camRange);

        Vector3 angle = transform.eulerAngles;
        angle.x = updownRotate;
        transform.eulerAngles = angle;
    }
}
