using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor_Child : MonoBehaviour
{
    [SerializeField] public float sTreatTime = 30f;

    private bool sIsRuning = false;
    [SerializeField] private bool sIsMoving = false;

    private Rigidbody sRigidbody;

    private GameObject sCamera = null;
    private GameObject sParent = null;

    private CameraCtrlScr sCameraScr = null;

    [SerializeField] public int sLife = 3;

    [SerializeField] public SurvivorState sState;

    protected SurvivorScr otherSurvivor = null;

    protected GameObject sTarget = null;

    protected Vector3 sForward = Vector3.zero;

    protected bool sCanTreat = false;

    public bool sIsHangingAtHook = false;
    public bool sIsHangingOnMurder = false;

    public Transform SHangPos = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SChangeState();
        SSetValueFollowState();
    }

    protected void SChangeState()
    {
        if (sIsHangingAtHook == true
            || sIsHangingOnMurder == true)
        {
            sState = SurvivorState.HANG;
        }
        else if (!sIsHangingOnMurder && !sIsHangingAtHook)
        {
            if (sLife == 3)
            {
                if (sIsRuning)
                    sState = SurvivorState.RUN;
                else
                    sState = SurvivorState.WALK;
            }
            else if (sLife == 2)
            {
                sState = SurvivorState.LIMP;
            }
            else if (sLife == 1)
            {
                sState = SurvivorState.CRAWL;
            }
            else if (sLife == 0)
            {
                sState = SurvivorState.DEAD;
            }
        }
    }
    protected void SSetValueFollowState()
    {
        switch (sState)
        {
            case SurvivorState.HANG:
                if (SHangPos != null)
                {
                    transform.position = SHangPos.position;
                    transform.rotation = SHangPos.rotation;
                }
                break;
        }

        if(sState != SurvivorState.HANG)
        {
            SHangPos = null;
        }
    }
}
