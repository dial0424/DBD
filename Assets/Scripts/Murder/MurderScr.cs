using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

using UnityEngine.Audio;

public class MurderScr : MonoBehaviourPun
{
    private Rigidbody myRigid;
    private Animator myAni;

    [SerializeField] private GameObject axe;
    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float rotateSpeed = 0;
    [SerializeField] private Transform hangPos;
    [SerializeField] private Camera myCam = null;
    [SerializeField] private AudioClip swingClip = null;
    [SerializeField] private float y = 0;

    private AudioSource murderAudio = null;

    private bool isAttack = false;
    public bool isAttackSuccess = false;
    public bool isHang = false;
    private bool isMove = true;

    GameObject hangPlayer;
    public string hanghookTag;

    private float delayTime = 2f;

    public bool hangOut = false;

    public bool isJump = false;
    public Transform jumpStartPos = null;
    public Transform jumpEndPos = null;
    public float journeyTime = 0;
    public float startTime;
    private bool jumpAni = false;

    private float brokenPower = 0.2f;

    private float maxAngleCol = 75f;
    private float maxAngleTri = 50f;

    private GameObject contactText = null;

    private int hangN = 0;
    private int money = 0;

    private float throwAxeCount = 1f;
    private float throwAxeDelay = 20f;

    [SerializeField] private Transform throwPos;

    private Image itemUI = null;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        myAni = GetComponent<Animator>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            myCam.gameObject.SetActive(true);
            contactText = GameObject.FindGameObjectWithTag("ContactText");
            contactText.SetActive(false);
            murderAudio = GetComponent<AudioSource>();
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (!isAttack && !isJump && isMove)
            {
                MurderMove();
            }
        }

        MurderDown();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (isMove)
            {
                MurderRotation();
            }

            if (Input.GetMouseButtonDown(0) && !isAttack && !isHang && isMove)
            {
                isAttack = true;
                PlaySound("Attack");
                StartCoroutine(MurderAttack());
            }

            if (Input.GetMouseButtonDown(1) && throwAxeCount > 0 && throwAxeDelay > 5f && !isAttack && !isHang && isMove)
            {
                isAttack = true;
                throwAxeDelay = 0;
                StartCoroutine(MurderThrowAxe());
            }

            if (isJump == true)
            {
                MurderJump();
            }

            if (isHang == true)
            {
                if (hangPlayer != null)
                {
                }
            }

            if (throwAxeDelay <= 5f)
            {
                throwAxeDelay += Time.deltaTime;
            }
        }
    }
    private void MurderMove()
    {

        float front = Input.GetAxis("Vertical");
        float side = Input.GetAxis("Horizontal");


            if (front != 0 || side != 0)
            {
                myAni.SetBool("IsWork", true);
            }
            else if (front == 0 && side == 0)
            {
                myAni.SetBool("IsWork", false);
            }

        //if (front != 0 || side != 0)
        //{
        //    myAni.SetBool("IsHangWork", true);
        //}
        //else if (front == 0 && side == 0)
        //{
        //    myAni.SetBool("IsHangWork", false);
        //}

        Vector3 pos = transform.forward * front * Time.deltaTime * moveSpeed +
                transform.right * side * Time.deltaTime * moveSpeed;

        myRigid.velocity = pos;

    }

    private void MurderDown()
    {
        if(transform.position.y > 0)
        {
            myRigid.AddForce(new Vector3(0, -y, 0));
        }
    }

    private void MurderRotation()
    {
        float side = Input.GetAxis("Mouse X");

        this.transform.Rotate(this.transform.up * side * rotateSpeed * Time.deltaTime);
    }
    
    public void AttackSuccessCheck()
    {
        isAttackSuccess = true;
        myAni.SetBool("AttackSuccess", true);
    }

    private IEnumerator MurderThrowAxe()
    {
        myAni.SetBool("ThrowAxe", true);
        GameObject sgo = PhotonNetwork.Instantiate("ThrowAxe", throwPos.position, transform.rotation);
        itemUI.transform.GetChild(0).gameObject.SetActive(true);
        itemUI.GetComponentInChildren<ItemUI>().fullTime = 5f;
        yield return new WaitForSeconds(1.5f);
        myAni.SetBool("ThrowAxe", false);
        isAttack = false;
    }

    private IEnumerator MurderAttack()
    {
        myAni.SetBool("IsAttack",true);
        axe.GetComponent<BoxCollider>().enabled = true;
        yield return new WaitForSeconds(delayTime);
        myAni.SetBool("IsAttack",false);
        axe.GetComponent<BoxCollider>().enabled = false;
        isAttack = false;
        if (isAttackSuccess == true)
        {
            StartCoroutine(AttackSuccess());
            isAttackSuccess = false;
        }
    }

    public IEnumerator AttackSuccess()
    {
        isMove = false;
        yield return new WaitForSeconds(delayTime * 2);
        isMove = true;
        myAni.SetBool("AttackSuccess", false);
    }

    private void MurderJump()
    {
        if (jumpAni == false)
        {
            myAni.SetBool("IsJump",true);
            jumpAni = true;
        }
        if (Vector3.Distance(jumpStartPos.position, jumpEndPos.position) <= 0.3f)
        {
            myAni.SetBool("IsJump", false);
            isJump = false;
            jumpAni = false;
        }

        Vector3 center = (jumpStartPos.position + jumpEndPos.position) * 0.5f;
        center -= new Vector3(0, 1, 0);
        Vector3 riseCenter = jumpStartPos.position - center;
        Vector3 setCenter = jumpEndPos.position - center;
        float complete = (Time.time - startTime) / journeyTime;
        transform.position = Vector3.Slerp(riseCenter, setCenter, complete);
        transform.position += center;
    }

    public void HangOut()
    {
        myAni.SetBool("HangOut",true);
        isHang = false;
        hangPlayer = null;
    }

    public void HangIn()
    {
        myAni.SetBool("IsHangIn",true);
        hangPlayer.GetComponentInChildren<SurvivorN>().CallMasterString("HangHookCall", hanghookTag);

        hangPlayer = null;
        hanghookTag = null;
        isHang = false;
        hangN++;
    }

    private IEnumerator MurderDontMove(string _ani)
    {
        isMove = false;
        myAni.SetBool(_ani, true);
        yield return new WaitForSeconds(delayTime * 2f);
        myAni.SetBool(_ani, false);
        isMove = true;
    }

    private void ContactTextOnOff(string _text,bool _set)
    {
        contactText.SetActive(_set);
        contactText.GetComponent<Text>().text = _text;
    }

    public void LoadSceneWin()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            CalculateRewardMoney(true);
            SceneManager.LoadScene("Win");
        }
    }

    public void LoadSceneLose()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            CalculateRewardMoney(false);
            SceneManager.LoadScene("Lose");
        }
    }

    private void PlaySound(string _clip)
    {
        switch(_clip)
        {
            case "Attack":
                murderAudio.clip = swingClip;
                break;
        }

        murderAudio.Play();
    }

    //20210718_±è¶õ
    private void CalculateRewardMoney(bool result)
    {
        PlayerPrefs.DeleteAll();

        int sumScore = 0;
        int deadSurvNum = GameObject.Find("PlayerManager").GetComponent<PlayerManager>().survDeadNum;

        money = 300;

        if (hangN < 7 && hangN > 1)
        {
            if (hangN == 6) sumScore = 50;
            else sumScore = (6 - hangN) * 150;
        }

        money += sumScore;
        PlayerPrefs.SetInt("Hang", sumScore);

        if (deadSurvNum == 0) sumScore = -50;
        else if (deadSurvNum == 1) sumScore = 300;
        else if (deadSurvNum == 2) sumScore = 550;

        money += sumScore;
        PlayerPrefs.SetInt("DeadSurv", sumScore);

        /*if (GameManager.instance.playTime <= 100f)
        {
            if (result) money += 200;
            else money += -150;
        }*/

        if (result)
        {
            money *= 2;
            PlayerPrefs.SetInt("Result", 1);
        }
        else PlayerPrefs.SetInt("Result", 0);

        if (photonView.IsMine)
        {
            LogoutManager.Instance.AddMoney(money);
        }
    }

    public void MurderSelectAxe(float _count)
    {
        throwAxeCount = _count;
        if(_count > 0)
        {
            SetItemInstans();
        }
    }

    public void SetItemInstans()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (itemUI == null)
            {
                itemUI = GameObject.Find("ItemUI").GetComponent<Image>();
                itemUI.sprite = Resources.Load("UI/UITexture/Axe", typeof(Sprite)) as Sprite;
            }
            else
                itemUI.sprite = Resources.Load("UI/UITexture/Axe", typeof(Sprite)) as Sprite;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine)
        {
            if ((other.CompareTag("BOARD") && other.GetComponent<Board>().isPush == true) || other.CompareTag("FLASHLIGHT")
)
            {
                StartCoroutine(MurderDontMove("IsStun"));
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (photonView.IsMine)
        {
            Vector3 _Pos = (collision.contacts[0].point - transform.position).normalized;

            if (Vector3.Angle(transform.forward, _Pos) <= maxAngleCol)
            {
                if (collision.transform.CompareTag("SURVIVOR") && collision.transform.GetComponentInChildren<SurvivorN>().survivorState == SurvivorState.CRAWL && isHang == false)
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        myAni.SetBool("HangOut", false);
                        myAni.SetBool("IsHangIn", false);
                        StartCoroutine(MurderDontMove("IsLift"));
                        isHang = true;
                        hangPlayer = collision.gameObject;
                        hangPlayer.GetComponentInChildren<SurvivorN>().CallMaster("HangInCall");
                    }
                }

                else if (collision.transform.CompareTag("JUMP") && isJump == false)
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        startTime = Time.time;
                        jumpStartPos = transform;
                        if (Vector3.Angle(collision.transform.forward, collision.contacts[0].point - transform.position) < 90)
                            jumpEndPos = collision.transform.GetChild(0).transform;
                        else
                            jumpEndPos = collision.transform.GetChild(1).transform;
                        isJump = true;
                    }
                }
                else if (collision.transform.CompareTag("GENERATOR") && collision.transform.GetComponent<Generator>().canDestroy)
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        StartCoroutine(MurderDontMove("IsKick"));

                        collision.transform.GetComponent<Generator>().CanDestroyAll(false);
                        collision.transform.GetComponent<Generator>().BrokenGeneratorCall(brokenPower);
                    }
                }
                else if (collision.transform.CompareTag("BOARDDOWN") && collision.transform.GetComponent<BoardRange>()._board.isDown == true)
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        collision.transform.GetComponent<BoardRange>()._board.isDown = false;
                        Destroy(collision.transform.parent.gameObject, 2f);
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (photonView.IsMine)
        {
            Vector3 v3Direction = other.transform.position - transform.position;
            if (Vector3.Angle(transform.forward, v3Direction) <= maxAngleTri)
            {
                if (other.CompareTag("BOARDDOWN"))
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        StartCoroutine(MurderDontMove("IsBreak"));
                        other.transform.parent.GetComponentInChildren<Board>().breakBoard(other.transform.parent.gameObject);
                    }

                }

                else if (other.CompareTag("HANGER") && isHang == true && other.GetComponent<HangerScr_>().survivor == null)
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        hanghookTag = other.transform.GetChild(7).transform.tag;
                        HangIn();
                    }
                }

                else if (other.CompareTag("JUMP") && isJump == false)
                {
                    ContactTextOnOff("Press On Space", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ContactTextOnOff("", false);
                        startTime = Time.time;
                        jumpStartPos = transform;
                        if (Vector3.Angle(other.transform.forward, v3Direction) <= 90)
                            jumpEndPos = other.transform.GetChild(0).transform;
                        else
                            jumpEndPos = other.transform.GetChild(1).transform;
                        isJump = true;
                    }
                }
            }

            
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (photonView.IsMine)
        {
            if (contactText.activeSelf)
            {
                ContactTextOnOff("", false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (photonView.IsMine)
        {
            if (contactText.activeSelf)
            {
                ContactTextOnOff("", false);
            }
        }
    }
}
