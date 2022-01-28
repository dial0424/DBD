using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using WebSocketSharp;

public enum SurvivorState
{
    WALK,
    RUN,
    LIMP,
    CRAWL,
    HANG,
    DEAD
}

public class SurvivorScr : MonoBehaviourPun
{
    public delegate void DamageDelegate(GameObject gameObject);
    public DamageDelegate damageCallback = null;
    public delegate void HangSurvivor(GameObject gameObject);
    public HangSurvivor hangSurvivor = null;
    public delegate void HangHook(GameObject gameObject,string _tag);
    public HangHook hangHook = null;

    public delegate void StateUiDelegate(string _state, int _myN);
    public StateUiDelegate StateUiCallBack = null;

    public delegate void LifeGauageDelegte(float _lifeTime, int _myN);
    public LifeGauageDelegte lifeGauageCallBack = null;

    public delegate void TreatDelegate(string _treatedSurvTag);
    public TreatDelegate treatCallback = null;
    public delegate void SetHangBoolDelegate(string _RescuedSurvTag);
    public SetHangBoolDelegate setHangBoolCallback = null;
    
    public delegate void SetDeadDelegate(string _deadSurvTag);
    public SetDeadDelegate setDeadCallback = null;

    public delegate void LoadSceneWinDelegate();
    public LoadSceneWinDelegate loadSceneWinCallback = null;

    public delegate void TurnOnFlashDelegate(string _tag);
    public TurnOnFlashDelegate turnOnFlashCallback = null;

    public delegate void ParticleDelegate(string _tag, Vector3 pos1, Vector3 pos2);
    public ParticleDelegate particleCallBack = null;

    [SerializeField] public Camera myCam = null;
    [SerializeField] private float sSpeed = 0f;
    [SerializeField] private float sStruggleTime = 30f;
    [SerializeField] private float sTreatTime = 30f;
    private float sStartTime = 0f;
    private float timeCount = 0.0f;
    [SerializeField] private float journeyTime = 0f;
    private float maxAngleCol = 75f;
    private float maxAngleTri = 90f;

    private bool sIsRuning = false;
    private bool sPressAorD = false;
    private bool isHitNull = false;
    private bool sIsJump = false;
    private bool sJumpOnce = true;
    [SerializeField] private bool sIsOpen = false;
    [SerializeField] private bool sIsMoving = false;
    [SerializeField] public bool sCanDown = false;

    private Transform sJumpStartPos = null;
    private Transform sJumpEndPos = null;

    private Rigidbody sRigidbody;

    public GameObject sGaugeBar;
    public GameObject sText = null;
    //private GameObject sCamera = null;
    private GameObject sParent = null;

    private CameraCtrlScr sCameraScr = null;

    public Generator sGenerator = null;
    [SerializeField] private Lever sLever = null;

    [SerializeField] public int sLife = 3;

    [SerializeField] protected SurvivorState sState;

    protected SurvivorN otherSurvivor = null;

    protected GameObject sTarget = null;
    protected GameObject sSecuredSurvivor = null;
    protected GameObject sHanger = null;

    protected Vector3 sForward = Vector3.zero;

    protected RaycastHit hit;

    protected float treatSpeed = 1f;
    protected float sRepairPower = 0.02f;

    [SerializeField] protected bool sCanTreat = false;

    protected Animator sAnim = null;

    public bool sIsHangingAtHook = false;
    public bool sIsHangingOnMurder = false;
    public bool sIsRepair = false;
    public bool sAttacked = false;

    public float sHangedHookTime = 30f;

    public Transform sHangPos = null;

    public Board sBoard = null;
    private GameObject sJumpObj = null;

    public int viewID = 0;

    public int myNum = 0;

    [SerializeField] private AudioSource survivorAudio=null;
    [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();

    [SerializeField] private Transform itemPos = null;
    private float itemTime = 0f;
    [SerializeField] private string itemName = null;

    private GameObject survItem = null;

    [SerializeField] private FirstAidKit sFirstAidKit = null;
    private bool sIsKit = false;

    private int sRescueN = 0;
    private int sTreatN = 0;
    private float sRepairingN = 0f;
    private float sRepairingTime = 0f;
    private int sMoney = 0;

    [SerializeField] private GameObject bloodParticle = null;
    private Vector3 instancPos;
    private Vector3 calPos;

    private Image itemUI = null;

    protected virtual void Start()
    {
        sParent = transform.parent.gameObject;
        if (photonView.IsMine)
        {
            sRigidbody = GetComponent<Rigidbody>();
            sCameraScr = myCam.GetComponent<CameraCtrlScr>();
            sAnim = GetComponent<Animator>();
            sState = SurvivorState.WALK;
            myCam.gameObject.SetActive(true);
        }

        //if (!string.IsNullOrEmpty(itemName)) SetItemInstans(itemName);

    }

    public void SetItemRpc(string _itemName)
    {
        if (string.IsNullOrEmpty(_itemName)) return;

        photonView.RPC("SetItemInstans", RpcTarget.All, _itemName);
    }

    [PunRPC]
    public void SetItemInstans(string _itemName)
    {
        itemName = _itemName;
        survItem = (GameObject)Instantiate(Resources.Load(itemName), itemPos);
        if (itemName == "FirstAidKit")
        {
            sFirstAidKit = survItem.GetComponent<FirstAidKit>();
        }

        //0728 란 추가
        if (!PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            itemUI = GameObject.Find("ItemUI").GetComponent<Image>();

            itemUI.sprite = Resources.Load("UI/UITexture/" + itemName, typeof(Sprite)) as Sprite;
        }
    }


    public void StartRPC(string _rpcName)
    {
        photonView.RPC(_rpcName, RpcTarget.All);
    }

    public void StartRPC(string _rpcName, Vector3 _pos1, Vector3 _pos2)
    {
        photonView.RPC(_rpcName, RpcTarget.All, _pos1, _pos2);
    }

    public void StartRPCString(string _rpcName,string _tag)
    {
        photonView.RPC(_rpcName, RpcTarget.All, _tag);
    }

    public void CallMaster(string _name)
    {
        photonView.RPC(_name, RpcTarget.MasterClient);
    }

    public void CallMasterString(string _name,string _tag)
    {
        photonView.RPC(_name, RpcTarget.MasterClient, _tag);
    }

    //private void PlaySurvivorSound(string _audioName)
    //{
    //    if (survivorAudio.isPlaying == true) survivorAudio.Stop();

    //    switch (_audioName)
    //    {
    //        case "Hurt":
    //            survivorAudio.clip = audioClips[0];
    //            break;
    //        case "Die":
    //            survivorAudio.clip = audioClips[1];
    //            break;
    //        case "Hang":
    //            survivorAudio.clip = audioClips[2];
    //            break;
    //        case "Walk":
    //            survivorAudio.clip = audioClips[3];
    //            break;
    //        case "Run":
    //            survivorAudio.clip = audioClips[4];
    //            break;
    //    }

    //    survivorAudio.Play();
    //}


    [PunRPC]
    public void OnDamageSurvivorRPC()
    {
        if (sLife > 1)
        {
            if (photonView.IsMine)
            {
                sAnim.SetTrigger("tAttacked");
            }
            sLife--;

            /*if (survivorAudio.isPlaying == false)
            {
                //PlaySurvivorSound("Hurt");
            }*/

            if (sIsRepair)
            {
                FromAttack();
            }

            SChangeState();
            if (photonView.IsMine)
            {
                photonView.RPC("UiCallBack", RpcTarget.MasterClient, myNum);
            }
        }
    }

    [PunRPC]
    public void OnHangSurvivorRPC()
    {
        sIsHangingOnMurder = true;
        sHangPos = GameObject.FindGameObjectWithTag("HANGPOS").transform;
        sParent.GetComponent<Rigidbody>().isKinematic = true;
        GetComponentInParent<CapsuleCollider>().enabled = false;

        SChangeState();
        if (photonView.IsMine)
        {
            photonView.RPC("UiCallBack", RpcTarget.MasterClient, myNum);
        }
    }
    
    //갈고리에 걸었을때 상태 바뀌는 부분
    [PunRPC]
    public void OnHangHookRPC(string _tag)
    {
        sGaugeBar.SetActive(false);
        sIsHangingOnMurder = false;
        sIsHangingAtHook = true;
        sHangPos = GameObject.FindGameObjectWithTag(_tag).transform;
        GameObject.FindGameObjectWithTag(_tag).transform.parent.GetComponent<HangerScr_>().survivor = this.transform.parent.gameObject;
        GameObject.FindGameObjectWithTag(_tag).transform.parent.GetComponent<HangerScr_>().SetMaterialHanger(_tag);
    }

    [PunRPC]
    public virtual void DamageCall()
    {
    }

    [PunRPC]
    public virtual void HangInCall()
    {
    }

    [PunRPC]
    public virtual void HangHookCall(string _tag)
    {
    }


    [PunRPC]
    public void UiCallBack(int _myNum)
    {
        string mystateString = sState.ToString();
        
        if(sLife==3) StateUiCallBack?.Invoke("WALK", _myNum);
        else if (sLife == 2) StateUiCallBack?.Invoke("LIMP", _myNum);
        else if (sLife == 1) StateUiCallBack?.Invoke(mystateString, _myNum);
        else if (sLife == 0) StateUiCallBack?.Invoke("DEAD", _myNum);
    }

    [PunRPC]
    public void LifeGaugeCallBack(float _lifeTime, int _myNum)
    {
        lifeGauageCallBack?.Invoke(_lifeTime, _myNum);
    }

    protected virtual void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            SMove();
            SSetValueFollowState();
            //StartCoroutine(tttset());

        }
        SurvivorDown();

        //if (sGaugeBar.activeSelf) Debug.LogError("true");
        //else Debug.LogError("false");
    }

    IEnumerator tttset()
    {
        yield return new WaitForSeconds(1f);
        if (sGaugeBar.activeSelf == false) Debug.LogError("게이지바 꺼짐");
        else if (sGaugeBar.activeSelf == true) Debug.LogError("게이지바 켜짐");
    }

    protected virtual void Update()
    {
        SChangeState();
        if (sIsHangingAtHook)
        {
            if (sHangedHookTime >= 0)
            {
                sHangedHookTime -= Time.deltaTime;
                if (photonView.IsMine)
                {
                    photonView.RPC("LifeGaugeCallBack", RpcTarget.MasterClient, sHangedHookTime, myNum);
                }
            }
            else
            {
                sIsHangingAtHook = false;
                sLife = 0;

                //if (survivorAudio.isPlaying == false)
                //{
                //    //PlaySurvivorSound("Die");
                //}

                SChangeState();
                if (photonView.IsMine)
                {
                    photonView.RPC("UiCallBack", RpcTarget.MasterClient, myNum);
                    photonView.RPC("ImDead", RpcTarget.MasterClient, transform.parent.GetChild(1).tag);
                    PlayerManager playerMng = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
                    if (playerMng != null)
                    {
                        playerMng.SetCameraActive(transform.parent.GetChild(1).tag);
                    }
                }
            }
        }

        if (photonView.IsMine)
        {
            //SSetValueFollowState();

            if (sIsJump) jump();
            if (!myCam.gameObject.activeSelf)
                myCam.gameObject.SetActive(true);
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            if (myCam.gameObject.activeSelf)
                myCam.gameObject.SetActive(false);
        }

        if (itemTime > 0f)
        {
            itemTime -= Time.deltaTime;
        }

        if (sIsRepair) sRepairingTime += Time.deltaTime;
    }

    private void CalculateRewardMoney(bool result)
    {
        PlayerPrefs.DeleteAll();

        int sumScore = 0;

        sMoney = 300;

        if (sState == SurvivorState.DEAD)
        {
            sumScore = -50;
        }
        else
        {
            sumScore = 100;
        }

        sMoney += sumScore;
        PlayerPrefs.SetInt("Dead", sumScore);

        if (sRescueN <= 5)
        {
            sumScore = sRescueN * 50;
        }

        else
        {
            sumScore = 250;
        }

        sMoney += sumScore;
        PlayerPrefs.SetInt("Rescue", sumScore);

        if (transform.parent.name.ToString().Equals("Survivor1(Clone)")) sRepairingN = sRepairingTime / 50f;
        if (transform.parent.name.ToString().Equals("Survivor2(Clone)")) sRepairingN = sRepairingTime / 12.5f;

        if (sRepairingN > 0 && sRepairingN < 1) sumScore = 70;
        else if (sRepairingN >= 1 && sRepairingN < 2) sumScore = 150;
        else if (sRepairingN >= 2 && sRepairingN < 3) sumScore = 230;
        else if (sRepairingN >= 3) sumScore = 350;

        sMoney += sumScore;
        PlayerPrefs.SetInt("Repair", sumScore);

        if (sTreatN <= 5)
            sumScore = sTreatN * 70;
        else sumScore = 5 * 70;

        sMoney += sumScore;
        PlayerPrefs.SetInt("Treat", sumScore);

        /*if (GameManager.instance.playTime <= 100f)
        {
            if (result) sMoney += 200;
            else sMoney += -150;
        }*/

        if (result)
        {
            sMoney *= 2;
            PlayerPrefs.SetInt("Result", 1);
        }
        else PlayerPrefs.SetInt("Result", 0);

        if (photonView.IsMine)
        {
            LogoutManager.Instance.AddMoney(sMoney);
        }
    }

    private void SurvivorDown()
    {
        if (transform.position.y > 0)
        {
            sParent.GetComponent<Rigidbody>().AddForce(new Vector3(0, -75f, 0));
        }
    }

    [PunRPC]
    public void ImDead(string _tag)
    {
        setDeadCallback?.Invoke(_tag);
    }

    //키입력 움직임
    protected void SMove()
    {
        if (!sIsHangingAtHook && !sIsHangingOnMurder && !sIsJump && !sAttacked)
        {
            if (Input.GetKey(KeyCode.W))
            {
                sForward += myCam.transform.forward;
                sForward = sForward.normalized;
                sForward.y = 0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                sForward += -myCam.transform.forward;
                sForward = sForward.normalized;
                sForward.y = 0f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                sForward += myCam.transform.right;
                sForward = sForward.normalized;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                sForward += -myCam.transform.right;
                sForward = sForward.normalized;
            }

            if (Input.GetKey(KeyCode.LeftShift) && sIsMoving)
            {
                //PlaySurvivorSound("Run");
                sIsRuning = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) || !sIsMoving)
            {
                //survivorAudio.Stop();
                sIsRuning = false;
            }
        }

        if (sAttacked || sCameraScr.speed == 0
            || (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)
            && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)))
        {
            sIsMoving = false;
            sForward = Vector3.zero;
            sParent.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        }
        else if(!sIsJump)
        {
            sIsMoving = true;
            transform.forward = sForward;       //sForward방향으로 회전
            
            sParent.GetComponent<Rigidbody>().velocity = transform.forward * sCameraScr.speed * 2f * Time.deltaTime;
        }
    }

    //상태 바꾸는 함수
    protected void SChangeState()
    {
        if(sIsHangingAtHook == true 
            || sIsHangingOnMurder == true)
        {
            sState = SurvivorState.HANG;
        }
        else if(!sIsHangingOnMurder && !sIsHangingAtHook)
        {
            if (sLife == 3)
            {
                if (sIsRuning && sIsMoving)
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
            else if(sLife == 0)
            {
                sState = SurvivorState.DEAD;
            }
        }
    }

    //생존자 상태에 따라 값 바뀌게 하는 함수
    protected void SSetValueFollowState()
    {
        switch (sState)
        {
            case SurvivorState.WALK:
                STreatSurvivor();
                SRescueSurvivor();
                SRepairGenerator();
                sLeverDown();
                BoardAttack();
                SJump();
                SUseItem();
                sHangPos = null;
                sCameraScr.speed = 110f;

                sAnim.SetBool("isRun", false);
                sAnim.SetBool("isLimp", false);

                if (sIsMoving)
                {
                    sAnim.SetBool("isWalk", true);
                    sAnim.SetBool("isFix", false);
                }
                else
                {
                    sAnim.SetBool("isWalk", false);
                }
                break;
            case SurvivorState.RUN:
                sCameraScr.speed = 150f;

                if (sIsRuning && sIsMoving)
                {
                    sAnim.SetBool("isRun", true);
                    sAnim.SetBool("isFix", false);
                }
                else
                {
                    sAnim.SetBool("isRun", false);
                }
                break;
            case SurvivorState.LIMP:
                SRescueSurvivor();
                STreatSurvivor();
                SRepairGenerator();
                sLeverDown();
                BoardAttack();
                SJump();
                SUseItem();
                sHangPos = null;
                sCameraScr.speed = 85f;

                sAnim.SetBool("isCrawl", false);
                sAnim.SetBool("isLimp", true);
                sAnim.SetBool("isHangH", false);

                if (sIsMoving)
                {
                    sAnim.SetBool("isWalk", true);
                    sAnim.SetBool("isFix", false);
                    if (sIsRuning)
                    {
                        sAnim.SetBool("isRun", true);
                        sCameraScr.speed = 100f;
                    }
                    else
                    {
                        sAnim.SetBool("isRun", false);
                        sCameraScr.speed = 85f;
                    }
                }
                else
                {
                    sAnim.SetBool("isRun", false);
                    sAnim.SetBool("isWalk", false);
                }

                break;
            case SurvivorState.CRAWL:
                sHangPos = null;
                sCameraScr.speed = 30f;

                sAnim.SetBool("isLimp", false);
                sAnim.SetBool("isCrawl", true);
                sAnim.SetBool("isFix", false);

                break;
            case SurvivorState.HANG:
                if (sHangPos != null)
                {
                    sParent.transform.position = sHangPos.position;
                    transform.rotation = sHangPos.rotation;
                }

                sAnim.SetBool("isCrawl", false);

                if (sIsHangingOnMurder)
                {
                    sAnim.SetBool("isHangM", true);
                    SStruggle();
                    transform.parent.GetComponent<CapsuleCollider>().isTrigger = true;
                }
                else if (sIsHangingAtHook)
                {
                    sAnim.SetBool("isHangH", true);
                    sAnim.SetBool("isHangM", false);
                }

                sAnim.SetBool("isCrawl", false);
                
                sCameraScr.speed = 0f;
                break;
            case SurvivorState.DEAD:

                sCameraScr.speed = 0f;
                break;
        }
    }

    //동료 치료
    protected void STreatSurvivor()
    { 
        if (otherSurvivor != null)
        {
            if (!sIsMoving)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (otherSurvivor.sState == SurvivorState.LIMP || otherSurvivor.sState == SurvivorState.CRAWL)
                    {
                        if (sText.activeSelf)
                            sText.SetActive(false);
                        if (!sGaugeBar.activeSelf)
                            sGaugeBar.SetActive(true);

                        sCanTreat = true;
                    }
                }
            }
            else
            {
                sCanTreat = false;
                return;
            }

            if (sCanTreat && !sIsMoving)
            {

                if (otherSurvivor.sTreatTime > 0)
                {

                    otherSurvivor.sTreatTime -= Time.deltaTime * treatSpeed;
                    sGaugeBar.transform.GetComponent<GeneratorGaugeBar>().setGauge(otherSurvivor.sTreatTime / 30f);

                    if (otherSurvivor.sTreatTime <= 0)
                    {
                        if (sGaugeBar.activeSelf)
                        {
                            sGaugeBar.SetActive(false);
                        }

                        sTreatN++;
                        photonView.RPC("STreateDelegate", RpcTarget.MasterClient, otherSurvivor.transform.parent.GetChild(1).tag);
                    }
                }

            }


            if (Input.GetMouseButtonUp(0))
            {
                sCanTreat = false;
                if (sGaugeBar.activeSelf)
                    sGaugeBar.SetActive(false);

                sText.SetActive(true);
            }

        }
        else
        {
            if (sGaugeBar.activeSelf)
                if (sGaugeBar.activeSelf && !sIsRepair && !sIsOpen && !sIsKit)
                    sGaugeBar.SetActive(false);
        }
    }


    //PlayerManager에서 마스터 클라이언트에 있는 치료된 오브젝트 찾아서
    //함수 호출시켜주는 함수 연결된 델리게이트
    [PunRPC]
    public void STreateDelegate(string _treatedSurvTag)
    {
        treatCallback?.Invoke(_treatedSurvTag);
    }

    //목숨 +1해주는 RPC함수
    [PunRPC]
    public void STreatRPC()
    {
        Debug.LogError("life++");
        sLife++;
        
        SChangeState();
        if (photonView.IsMine)
        {
            photonView.RPC("UiCallBack", RpcTarget.MasterClient, myNum);
        }
    }

    //발버둥
    //HANG 상태중에 hangAtMurder가 true일때 호출
    protected void SStruggle()
    {
        if ((Input.GetKeyDown(KeyCode.A) && sPressAorD) ||
            (Input.GetKeyDown(KeyCode.D) && !sPressAorD))
        {
            sPressAorD = !sPressAorD;
            if (sStruggleTime >= 0)
            {
                sStruggleTime -= Time.deltaTime * 5f;
                if(!sGaugeBar.activeSelf)
                    sGaugeBar.SetActive(true);

                sGaugeBar.GetComponent<GeneratorGaugeBar>().setGauge(sStruggleTime / 30f);
            }
        }

        if (sStruggleTime < 0 && sIsHangingOnMurder)
        {
            sGaugeBar.SetActive(false);
            sIsHangingOnMurder = false;
            sAnim.SetBool("isHangM", false);
            photonView.RPC("SSetHangBoolDelegate", RpcTarget.MasterClient, transform.parent.GetChild(1).tag);
            photonView.RPC("ExecuteMurderHangOut", RpcTarget.MasterClient);
            photonView.RPC("STreateDelegate", RpcTarget.MasterClient, transform.parent.GetChild(1).tag);
            sStruggleTime = 30f;
        }
    }

    [PunRPC]
    public void ExecuteMurderHangOut()
    {
        GameObject.FindGameObjectWithTag("MURDER").GetComponent<MurderScr>().HangOut();
    }

    //고리에 걸린 동료 구하는 함수
    protected void SRescueSurvivor()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (sHanger == null)
            {
                return;
            }

            sSecuredSurvivor = sHanger.GetComponent<HangerScr_>().survivor;
            
            if (sSecuredSurvivor != null)
            {
                if (sSecuredSurvivor.GetComponentInChildren<SurvivorN>().sState == SurvivorState.HANG && sSecuredSurvivor.GetComponentInChildren<SurvivorN>().sState != SurvivorState.DEAD)
                {
                    string securedSurvTag = sSecuredSurvivor.transform.GetChild(1).tag;

                    photonView.RPC("SSetHangBoolDelegate", RpcTarget.MasterClient, securedSurvTag);
                    photonView.RPC("STreateDelegate", RpcTarget.MasterClient, securedSurvTag);

                    sSecuredSurvivor = null;
                    sHanger.GetComponent<HangerScr_>().StartRPCMaster("SetNullDelegate", sHanger.transform.GetChild(7).tag.ToString());
                    sRescueN++;
                }
            }
        }
    }

    [PunRPC]
    public void SSetHangBoolDelegate(string _tag)
    {
        setHangBoolCallback?.Invoke(_tag);
    }

    [PunRPC]
    public void SSetHangBoolRPC()
    {
        sHangPos = null;
        sHanger = null;
        sIsHangingAtHook = false;
        sIsHangingOnMurder = false;
        sParent.GetComponent<Rigidbody>().isKinematic = false;
        GetComponentInParent<CapsuleCollider>().enabled = true;
        GetComponentInParent<CapsuleCollider>().isTrigger = false;
    }

    protected void SUseItem()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            //Debug.LogError("return");
            return;
        }

        if (itemTime <= 0f)
        {
            if(itemName== "Flashlight")
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    photonView.RPC("STurnOnFlashDelegate", RpcTarget.MasterClient);

                    itemUI.transform.GetChild(0).gameObject.SetActive(true);
                    itemUI.transform.GetChild(0).GetComponent<ItemUI>().fullTime = 100f;
                }
            }
            else if(itemName== "FirstAidKit")
            {
                if (!sIsMoving)
                {
                    if(this.sState == SurvivorState.LIMP)
                    {
                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                            if (!sGaugeBar.activeSelf)
                                sGaugeBar.SetActive(true);

                            sIsKit = true;
                        }

                        if (sIsKit)
                        {
                            if (sFirstAidKit.kitTime > 0f)
                            {
                                sFirstAidKit.kitTime -= Time.deltaTime * sFirstAidKit.kitPower;
                                sGaugeBar.transform.GetComponent<GeneratorGaugeBar>().setGauge(sFirstAidKit.kitTime / 30f);

                                if (sFirstAidKit.kitTime <= 0f)
                                {
                                    if (sGaugeBar.activeSelf)
                                    {
                                        sGaugeBar.SetActive(false);
                                    }
                                    photonView.RPC("STreateDelegate", RpcTarget.MasterClient, this.transform.parent.GetChild(1).tag);

                                    itemUI.transform.GetChild(0).gameObject.SetActive(true);
                                    itemUI.transform.GetChild(0).GetComponent<ItemUI>().fullTime = 180f;

                                    itemTime = 180f;
                                }
                            }
                        }

                        if (Input.GetKeyUp(KeyCode.Q))
                        {
                            if (sGaugeBar.activeSelf)
                            {
                                sGaugeBar.SetActive(false);
                            }
                            sIsKit = false;
                        }
                    }
                }
                else
                {
                    if (sGaugeBar != null)
                        if (sGaugeBar.activeSelf && !sIsRepair && !sIsOpen && !sCanTreat)
                            sGaugeBar.SetActive(false);
                    sIsKit = false;
                    return;
                }
            }
        }
    }

    [PunRPC]
    public void STurnOnFlashDelegate()
    {
        turnOnFlashCallback?.Invoke(transform.parent.GetChild(1).tag);
    }

    //20210714_김란
    [PunRPC]
    public void STurnOnFlash()
    {
        Debug.LogError("활성화함" + survItem.transform.GetChild(0).gameObject);
        survItem.transform.GetChild(0).gameObject.SetActive(true);
        itemTime = 100f;
    }


    //정채은이 함
    //발전기 함수
    protected void SRepairGenerator()
    {
        if (sGenerator != null)
        {
            if (!sIsMoving)
            {
                if (Input.GetMouseButtonDown(0) && !sGenerator.isStun && !sGenerator.isClear)
                {
                    sIsRepair = true;
                    if (photonView.IsMine)
                    {
                        sAnim.SetBool("isFix", true);
                        sGenerator.MasterCanDestroy(true);

                        //sGenerator.PlaySoundAll();
                    }

                    sText.gameObject.SetActive(false);
                    sGaugeBar.SetActive(true);

                }

                if (sIsRepair)
                {
                    if (photonView.IsMine)
                    {
                        sGenerator.GaugeUp(sRepairPower);
                    }
                    sGaugeBar.GetComponent<GeneratorGaugeBar>().setGauge(sGenerator.gauge);

                    FromGenerator();

                    if (sGenerator.isClear == true)
                    {
                        if (photonView.IsMine)
                            sAnim.SetBool("isFix", false);
                        sText.gameObject.SetActive(false);
                        //sGaugeBar.SetActive(false);
                        
                        sIsRepair = false;

                        sGenerator = null;
                    }

                }

                if (Input.GetMouseButtonUp(0) && sIsRepair == true)
                {
                    if (photonView.IsMine)
                    {
                        sAnim.SetBool("isFix", false);

                        //sGenerator.StopSoundAll();

                        //if (sGenerator.genAudioSource != null)
                        //{
                        //    sGenerator.StopSoundAll();
                        //}
                        //else
                        //{
                        //    Debug.LogError("null");
                        //}
                    }
                        
                    sIsRepair = false;
                    
                    sText.gameObject.SetActive(true);

                    sGenerator = null;
                }
            }
            else
            {
                sAnim.SetBool("isFix", false);
                sIsRepair = false;
                if (!sGenerator.isStun && !sGenerator.isClear) sText.gameObject.SetActive(true);
            }

            if (sGenerator != null)
            {
                if (photonView.IsMine)
                {
                    if (sIsRepair == true) sGenerator.generatorIsRepair = true;
                    else sGenerator.generatorIsRepair = false;
                }
            }
        }
    }

    //[PunRPC]
    //public void GenCanDestroyPlzMaster()
    //{
    //    photonView.RPC("GenCanDestroyAll", RpcTarget.All);
    //}

    //[PunRPC]
    //public void GenCanDestroyAll()
    //{
    //    sGenerator.canDestroy = true;
    //}

    //정채은 코드
    //발전기 스턴 상태인지 확인
    private void FromGenerator()
    {
        if (sGenerator.isStun == true)
        {
            if (photonView.IsMine)
                sAnim.SetBool("isFix", false);
            sGaugeBar.SetActive(false);
            sText.gameObject.SetActive(false);

            sIsRepair = false;
        }
    }

    private void FromAttack()
    {
        sGaugeBar.SetActive(false);
        if(sGenerator!=null&&sState!=SurvivorState.CRAWL) sText.gameObject.SetActive(false);
        sIsRepair = false;
    }

    //정채은 코드
    private void BoardAttack()
    {
        if (sBoard != null)
        {
            if (Input.GetKeyDown(KeyCode.Space) && sBoard.isDown == false)
            {
                sBoard.transform.parent.transform.parent.GetComponentInChildren<BoardRange>().pushBoard(sBoard.transform.parent.parent.gameObject);
            }
        }
    }
    
    //황채민님 코드
    private void SJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (sJumpObj == null) return;
            if(sIsJump == false)
            {
                sStartTime = Time.time;
                sJumpStartPos = sParent.transform;
                sIsJump = true;
            }
            else return;
        }
    }

    private void jump()
    {
        if (sJumpOnce)
        {
            sJumpOnce = false;
            sAnim.SetTrigger("tJump");
        }
        if (Vector3.Distance(sJumpStartPos.position, sJumpEndPos.position) <= 0.05f)
        {
            sIsJump = false;
        }

        Vector3 center = (sJumpStartPos.position + sJumpEndPos.position) * 0.5f;
        center -= new Vector3(0, 1, 0);
        Vector3 riseCenter = sJumpStartPos.position - center;
        Vector3 setCenter = sJumpEndPos.position - center;
        float complete = (Time.time - sStartTime) / journeyTime;
        sParent.transform.position = Vector3.Slerp(riseCenter, setCenter, complete);
        sParent.transform.position += center;
        transform.localPosition = Vector3.zero;
    }

    protected void sLeverDown()
    {
        if (sLever != null)
        {
            if (!sIsMoving)
            {
                //상호작용 했을때 발전기가 모두 고쳐져 있으면 실행
                if (Input.GetMouseButtonDown(0) && sLever.canUse == true)
                {
                    sLever.leverAnim.Play();
                    sIsOpen = true;
                    sGaugeBar.SetActive(true);
                    sText.SetActive(false);
                }

                if (sIsOpen)
                {
                    sLever.LeverGaugeUp();
                    sGaugeBar.GetComponent<GeneratorGaugeBar>().setGauge(1f - sLever.leverGauge);

                    if (sLever.isExitOpen)
                    {
                        sGaugeBar.SetActive(false);
                        sLever = null;
                        sIsOpen = false;
                    }
                }

                if (Input.GetMouseButtonUp(0) && sIsOpen == true)
                {
                    sIsOpen = false;
                    if (sLever.canUse == true && !sText.activeSelf)
                        sText.SetActive(true);
                    sGaugeBar.SetActive(false);
                }
            }
            else
            {
                sIsOpen = false;
                sGaugeBar.SetActive(false);
                if(sLever.canUse == true && !sText.activeSelf)
                    sText.SetActive(true);
            }
        }
    }

    public void LoadSceneWin()
    {
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            CalculateRewardMoney(true);
            SceneManager.LoadScene("Win");
        }
    }

    public void LoadSceneLose()
    {
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            CalculateRewardMoney(false);
            SceneManager.LoadScene("Lose");
        }
    }

    //살인마가 생존자 상태 확인 할 수 있게하는 함수
    public SurvivorState survivorState
    {     get { return sState; } }

    private IEnumerator Setparticle(Vector3 instancPos, Vector3 calPos)
    {
        bloodParticle.transform.position = instancPos;
        bloodParticle.transform.forward = (instancPos - calPos).normalized;
        bloodParticle.transform.GetComponentInChildren<ParticleSystem>().Play();
        yield return new WaitForSeconds(1f);
        bloodParticle.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        bloodParticle.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
    }

    [PunRPC]
    public void StartCoroutinePartiRPC(Vector3 pos1, Vector3 pos2)
    {
        StartCoroutine(Setparticle(pos1, pos2));
    }

    [PunRPC]
    public void ParticleCallBack(Vector3 pos1, Vector3 pos2)
    {
        particleCallBack?.Invoke(transform.parent.GetChild(1).tag, pos1, pos2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sState != SurvivorState.HANG && sState != SurvivorState.CRAWL)
        {
            if (other.tag == "ESCAPE")
            {
                loadSceneWinCallback?.Invoke();
            }
            else if (other.tag == "CANBOARDATTACK" && other.GetComponent<BoardRange>()._board.isDown == false)
            {
                if (photonView.IsMine)
                {
                    if (!sText.activeSelf)
                        sText.SetActive(true);
                    sText.GetComponent<Text>().text = "Press 'SPACE' to BoardAttack";
                }
                sBoard = other.GetComponent<BoardRange>()._board;
            }
            else if (other.tag == "AXE")
            {
                instancPos = sParent.GetComponent<Collider>().ClosestPoint(other.transform.position);
                calPos = sParent.transform.position;
                calPos.y = instancPos.y;

                //StartCoroutine(Setparticle(instancPos, calPos));
                photonView.RPC("ParticleCallBack", RpcTarget.MasterClient, instancPos, calPos);
            }

            Vector3 v3Direction = other.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, v3Direction) <= maxAngleTri)
            {
                if (other.tag == "TREATOBJ")
                {
                    if (otherSurvivor == null)
                    {
                        otherSurvivor = other.GetComponent<SurvivorN>();
                        if (otherSurvivor.sState == SurvivorState.CRAWL || otherSurvivor.sState == SurvivorState.LIMP)
                        {
                            if (photonView.IsMine)
                            {
                                if (!sText.activeSelf)
                                    sText.SetActive(true);
                                sText.GetComponent<Text>().text = "Mouse 'Left Click' to Treat";
                            }
                        }
                    }
                }
                else if (other.tag == "HANGER")
                {
                    sHanger = other.gameObject;
                    sSecuredSurvivor = other.GetComponent<HangerScr_>().survivor;
                    if (sSecuredSurvivor != null)
                    {
                        if (photonView.IsMine)
                        {
                            if (!sText.activeSelf)
                                sText.SetActive(true);
                            sText.GetComponent<Text>().text = "Press 'R' to Rescue";
                        }
                    }
                }
                else if (other.tag == "GENERATOR")
                {
                    sGenerator = other.GetComponent<Generator>();
                    if (sGenerator.isClear) sGenerator = null;
                    else if (sGenerator != null)
                    {
                        if (photonView.IsMine)
                        {
                            if (!sText.activeSelf)
                                sText.SetActive(true);
                            sText.GetComponent<Text>().text = "Mouse 'Left Click' to Repair";
                        }
                    }
                }
                else if (other.tag == "BOARDDOWN" || other.tag == "JUMP")
                {
                    sJumpObj = other.gameObject;
                    if (sJumpObj != null)
                    {
                        if (photonView.IsMine)
                        {
                            if (!sText.activeSelf)
                                sText.SetActive(true);
                            sText.GetComponent<Text>().text = "Press 'SPACE' to Jump";
                        }
                    }
                    if (Vector3.Angle(other.transform.forward, transform.forward) < 90)
                    {
                        sJumpEndPos = other.transform.GetChild(0).transform;
                    }
                    else
                    {
                        sJumpEndPos = other.transform.GetChild(1).transform;
                    }
                }
                else if (other.tag == "LEVER")
                {
                    sLever = other.GetComponent<Lever>();
                    if (sLever != null)
                    {
                        if (sLever.canUse)
                        {
                            if (photonView.IsMine)
                            {
                                if (!sText.activeSelf)
                                    sText.SetActive(true);
                                sText.GetComponent<Text>().text = "Mouse 'Left Click' to Escape";
                            }
                        }
                        else sLever = null;
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Vector3 v3Direction = other.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, v3Direction) > maxAngleTri)
        {
            if (other.tag == "TREATOBJ")
            {
                if(photonView.IsMine)
                    if (sText.activeSelf)
                        sText.SetActive(false);
                otherSurvivor = null;
            }
            else if (other.tag == "HANGER")
            {
                if (photonView.IsMine)
                    if (sText.activeSelf)
                        sText.SetActive(false);
                sHanger = null;
                sSecuredSurvivor = null;
            }
            else if (other.tag == "GENERATOR")
            {
                if (photonView.IsMine)
                    if (sText.activeSelf)
                        sText.SetActive(false);
                sGenerator = null;
            }
            else if (other.tag == "BOARDDOWN" || other.tag == "JUMP")
            {
                if (photonView.IsMine)
                    if (sText.activeSelf)
                        sText.SetActive(false);
                if (!sIsJump)
                {
                    sJumpObj = null;
                    sJumpEndPos = null;
                }
            }
            else if (other.tag == "LEVER")
            {
                if (photonView.IsMine)
                    if (sText.activeSelf)
                        sText.SetActive(false);
                sLever = null;
            }
        }
        else
        {
            if (sState != SurvivorState.HANG && sState != SurvivorState.CRAWL)
            {
                if (other.tag == "TREATOBJ")
                {
                    if (otherSurvivor == null)
                    {
                        otherSurvivor = other.GetComponent<SurvivorN>();
                        if (otherSurvivor.sState == SurvivorState.CRAWL || otherSurvivor.sState == SurvivorState.LIMP)
                        {
                            if (photonView.IsMine && !sText.activeSelf && !sCanTreat)
                                sText.SetActive(true);
                        }
                    }
                }
                else if (other.tag == "HANGER")
                {
                    if (sHanger == null || sSecuredSurvivor == null)
                    {
                        if (sSecuredSurvivor != null && photonView.IsMine)
                        {
                            if (!sText.activeSelf)
                                sText.SetActive(true);
                            sText.GetComponent<Text>().text = "Press 'R' to Rescue";
                        }
                        sHanger = other.gameObject;
                        sSecuredSurvivor = other.GetComponent<HangerScr_>().survivor;
                    }
                }
                else if (other.tag == "GENERATOR" && !other.GetComponent<Generator>().isClear)
                {
                    if (sGenerator == null && !sIsMoving)
                    {
                        sGenerator = other.GetComponent<Generator>();
                        if (sGenerator != null)
                        {
                            if (photonView.IsMine && !sText.activeSelf)
                                sText.SetActive(true);
                        }
                    }
                }
                else if (other.tag == "BOARDDOWN" || other.tag == "JUMP")
                {
                    if (!sIsJump)
                    {
                        if (photonView.IsMine && !sText.activeSelf)
                            sText.SetActive(true);
                        sJumpObj = other.gameObject;
                        if (Vector3.Angle(other.transform.forward, transform.forward) < 90)
                        {
                            if (sJumpEndPos == null)
                            {
                                sJumpEndPos = other.transform.GetChild(0).transform;
                            }
                        }
                        else
                        {
                            if (sJumpEndPos == null)
                            {
                                sJumpEndPos = other.transform.GetChild(1).transform;
                            }
                        }
                    }
                }
                else if (other.tag == "LEVER")
                {
                    if (sLever == null)
                        sLever = other.GetComponent<Lever>();
                    else if (!sText.activeSelf && !sIsOpen && sLever.canUse && photonView.IsMine)
                    {
                        sText.SetActive(true);
                        sText.GetComponent<Text>().text = "Mouse 'Left Click' to Escape";
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "TREATOBJ")
        {
            if (photonView.IsMine)
                if (sText.activeSelf)
                    sText.SetActive(false);
            otherSurvivor = null;
        }
        else if (other.tag == "LEVER")
        {
            if (photonView.IsMine)
                if (sText.activeSelf)
                    sText.SetActive(false);
            sLever = null;
        }
        else if (other.tag == "CANBOARDATTACK")
        {
            if (photonView.IsMine)
                if (sText.activeSelf)
                    sText.SetActive(false);
            sBoard = null;
        }
        else if(other.tag == "HANGER")
        {
            if (photonView.IsMine)
                if (sText.activeSelf)
                    sText.SetActive(false);
            sHanger = null;
        }
    }
}