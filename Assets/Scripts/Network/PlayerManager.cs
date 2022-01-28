using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] List<GameObject> gList = new List<GameObject>();
    [SerializeField] UiManager uiManager = null;
    private bool[] survDeadState = new bool[2];
    [SerializeField] private List<Vector3> sPos = new List<Vector3>();
    [SerializeField] private GameObject camNoticeText = null;

    public int survDeadNum = 0;

    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameObject go = PhotonNetwork.Instantiate("Murder", new Vector3(0, 0, 0), Quaternion.identity);
            gList.Add(go);
            go.GetComponent<MurderScr>().MurderSelectAxe(PlayerPrefs.GetFloat("Axe"));
            photonView.RPC("CreateSurvivor", RpcTarget.Others, 0, "MURDER");
            for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
            {
                foreach (var player in PhotonNetwork.PlayerListOthers)
                {
                    int num = Random.Range(0, sPos.Count);
                    if (player.NickName == PlayerPrefs.GetString(i.ToString()))
                    {
                        uiManager.SurvivorNickName(i, player.NickName);
                        GameObject sgo = PhotonNetwork.Instantiate(PlayerPrefs.GetString("Survivor" + player.NickName)
                            , sPos[num], Quaternion.identity);
                        sPos.RemoveAt(num);
                        PhotonView pv = sgo.GetPhotonView();
                        pv.TransferOwnership(player);
                        pv = sgo.transform.GetChild(0).gameObject.GetPhotonView();
                        pv.TransferOwnership(player);

                        SurvivorN survivorN = sgo.GetComponentInChildren<SurvivorN>();
                        survivorN.damageCallback = OndamageSurvivor;
                        survivorN.hangSurvivor = HangInSurvivor;
                        survivorN.hangHook = HangHookSurvivor;
                        survivorN.treatCallback = TreatSurvivor;
                        survivorN.setHangBoolCallback = SetHangBool;
                        survivorN.StateUiCallBack = CurrentSurvivorState;
                        survivorN.setDeadCallback = SetSurvivorDEAD;
                        survivorN.loadSceneWinCallback = LoadSurvWinScene;
                        survivorN.lifeGauageCallBack = DecreaseLifeGauge;
                        survivorN.turnOnFlashCallback = TurnOnLight;
                        survivorN.particleCallBack = SetParticle;
                        survivorN.viewID = sgo.GetPhotonView().ViewID;
                        //survivorN.itemName = "FirstAidKit";

                        photonView.RPC("SetTag", RpcTarget.All, "Player", "Survivor" + i.ToString());
                        gList.Add(sgo);
                        photonView.RPC("CreateSurvivor", RpcTarget.Others,i, "Survivor"+i.ToString());
                        StartCoroutine(SetItem(survivorN, PlayerPrefs.GetString("item" + i.ToString())));
                        break;
                    }
                }
            }
            survDeadState[0] = false;
            survDeadState[1] = false;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("AddGaugeBar", RpcTarget.All);
        }
    }
    private IEnumerator SetItem(SurvivorN _survivorN, string _itemName)
    {
        yield return new WaitForSeconds(1f);

        _survivorN.SetItemRpc(_itemName);
    }
    public void LoadSurvWinScene()
    {
        photonView.RPC("SurvLoadEndingScene", RpcTarget.All);
    }

    public void LoadMurdWinScene()
    {
        photonView.RPC("LoadMurdEndingScene", RpcTarget.All);
    }

    [PunRPC]
    public void SurvLoadEndingScene()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        foreach (var a in gList)
        {
            if (a.tag == "SURVIVOR")
            {
                a.GetComponentInChildren<SurvivorN>().LoadSceneWin();
            }
            else a.GetComponent<MurderScr>().LoadSceneLose();
        }
    }

    [PunRPC]
    public void LoadMurdEndingScene()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        gList.Reverse();
        foreach (var a in gList)
        {
            if (a.tag == "SURVIVOR")
            {
                a.GetComponentInChildren<SurvivorN>().LoadSceneLose();
            }
            else a.GetComponent<MurderScr>().LoadSceneWin();
        }
    }

    [PunRPC]
    public void SetSurvivorDEAD(string _tag)
    {
        int idx = 0;
        foreach (var a in gList)
        {
            if (a.transform.GetChild(1).tag == _tag)
                idx = gList.IndexOf(a) - 1;
        }
        survDeadState[idx] = true;
        survDeadNum++;

        if (survDeadState[0] && survDeadState[1]) LoadMurdWinScene();
    }

    public void SetCameraActive(string _tag)
    {
        //bool setOtherCamTrue = false;
        Camera deadSurvCam = null;
        Camera liveSurvCam = null;
        for (int i=1;i<gList.Count;i++)
        {
            var a = gList[i];
            Debug.LogErrorFormat("{0} {1}", a.transform.GetChild(1).tag, _tag);
            if (a.transform.GetChild(1).tag == _tag)
            {
                deadSurvCam = a.transform.GetChild(2).gameObject.GetComponent<Camera>();
                if (deadSurvCam != null)
                {
                    Debug.LogError(deadSurvCam.transform.parent);
                }
            }
            else
            {
                liveSurvCam = a.transform.GetChild(2).gameObject.GetComponent<Camera>();
                liveSurvCam.gameObject.SetActive(true);
                if (liveSurvCam != null)
                {
                    Debug.LogError(liveSurvCam.transform.parent);
                }
            }
        }
        camNoticeText.SetActive(true);
        GameObject.Find("Camera").GetComponent<Camera>().enabled = false;
        liveSurvCam.enabled = true;
        deadSurvCam.enabled = false;
        deadSurvCam.gameObject.SetActive(false);

        Debug.LogError(deadSurvCam.enabled + " " + liveSurvCam.enabled);
    }

    private GameObject FindGameOBJInList(string _tag)
    {
        GameObject returnOBJ = null;
        foreach (var player in gList)
        {
            if (player.transform.GetChild(1).tag == _tag)
            {
                returnOBJ = player;
                break;
            }
        }
        return returnOBJ;
    }

    public void TreatSurvivor(string _treatedSurvTag)
    {
        GameObject treatedSurv = null;

        treatedSurv = FindGameOBJInList(_treatedSurvTag);

        if (treatedSurv != null && PhotonNetwork.IsMasterClient)
            RPCManager.SurvivorStartRPC(treatedSurv, "STreatRPC");
    }

    public void TurnOnLight(string _survivorTag)
    {
        GameObject surv = null;

        surv = FindGameOBJInList(_survivorTag);

        if (surv != null && PhotonNetwork.IsMasterClient)
            RPCManager.SurvivorStartRPC(surv, "STurnOnFlash");
    }

    public void SetHangBool(string _tag)
    {
        GameObject rescuedSurv = null;
        rescuedSurv = FindGameOBJInList(_tag);
        if (rescuedSurv != null && PhotonNetwork.IsMasterClient)
            RPCManager.SurvivorStartRPC(rescuedSurv, "SSetHangBoolRPC");
    }

    [PunRPC]
    public void AddGaugeBar()
    {
        for (int i = 1; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            gList[i].GetComponentInChildren<SurvivorN>().sGaugeBar = GameObject.Find("Canvas").transform.GetChild(2).gameObject;
            gList[i].GetComponentInChildren<SurvivorN>().sGaugeBar.SetActive(false);
            gList[i].GetComponentInChildren<SurvivorN>().sText = GameObject.Find("Canvas").transform.GetChild(5).gameObject;
        }
    }

    public void SetParticle(string _attackedSurvTag, Vector3 pos1, Vector3 pos2)
    {
        GameObject attacktedSurv = null;

        attacktedSurv = FindGameOBJInList(_attackedSurvTag);

        if (attacktedSurv != null && PhotonNetwork.IsMasterClient)
            RPCManager.SurvivorStartRPC(attacktedSurv, "StartCoroutinePartiRPC", pos1, pos2);
    }

    public void CurrentSurvivorState(string _state, int _myN)
    {
        uiManager.currentSurvivorStateRPC(_state, _myN);
    }

    public void DecreaseLifeGauge(float _lifeTime, int _num)
    {
        uiManager.DecreaseLifeGaugeStateRPC(_lifeTime, _num);
    }

    public void OndamageSurvivor(GameObject _gameObject)
    {
        int id = _gameObject.GetComponent<SurvivorN>().viewID;
        for (int i = 1; i < gList.Count; i++)
        {
            if (gList[i].GetPhotonView().ViewID == id)
            {
                RPCManager.SurvivorStartRPC(gList[i], "OnDamageSurvivorRPC");
            }
        }
    }

    public void HangInSurvivor(GameObject _gameObject)
    {
        int id = _gameObject.GetComponent<SurvivorN>().viewID;
        for (int i = 1; i < gList.Count; i++)
        {
            if (gList[i].GetPhotonView().ViewID == id)
            {
                RPCManager.SurvivorStartRPC(gList[i], "OnHangSurvivorRPC");
            }
        }
    }

    public void HangHookSurvivor(GameObject _gameObject,string _tag)
    {
        int id = _gameObject.GetComponent<SurvivorN>().viewID;
        for (int i = 1; i < gList.Count; i++)
        {
            if (gList[i].GetPhotonView().ViewID == id)
            {
                RPCManager.SurvivorStartRPCString(gList[i], "OnHangHookRPC", _tag);
            }
        }
    }

    [PunRPC]
    public void SetTag(string _tag,string _change)
    {
        GameObject go = GameObject.FindGameObjectWithTag(_tag);
        go.tag = _change;
        go.transform.parent.GetComponentInChildren<SurvivorN>().myNum = gList.Count;
    }

    [PunRPC]
    public void CreateSurvivor(int _num,string _tag)
    {
        if(_tag == "MURDER")
            gList.Add(GameObject.FindGameObjectWithTag(_tag));
        else
        {
            gList.Add(GameObject.FindGameObjectWithTag(_tag).transform.parent.gameObject);
        }
    }


}
