using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;

using UnityEngine.UI;
using System.Threading.Tasks;

using Photon.Pun;
using Photon.Realtime;

public class LoginManager : MonoBehaviourPun
{
    [SerializeField] private InputField idIF = null;
    [SerializeField] private InputField passIF = null;
    [SerializeField] private Text checkText = null;
    [SerializeField] private Button selectMurder = null;
    [SerializeField] private Button selectSurvivor = null;
    [SerializeField] private Button loginBt = null;
    [SerializeField] private Button createBt = null;
    [SerializeField] private Button logoutBt = null;
    [SerializeField] private Button shopBt = null;

    private LogoutManager lm = null;

    [SerializeField] private GameObject noticeWindow = null;

    public class UserInfo
    {
        public string id = "";
        public string password = "";
        public string login = "";
        public float money = 0f;
        public float heal = 0f;
        public float light = 0f;
        public float axe = 0f;

        public UserInfo(string _id, string _password,string _login,float _money,float _heal, float _light, float _axe)
        {
            this.id = _id;
            this.password = _password;
            this.login = _login;
            this.money = _money;
            this.heal = _heal;
            this.light = _light;
            this.axe = _axe;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["id"] = this.id;
            dic["password"] = this.password;
            dic["login"] = this.login;
            dic["money"] = this.money;
            dic["heal"] = this.heal;
            dic["light"] = this.light;
            dic["axe"] = this.axe;
            return dic;
        }
    }

    FirebaseApp firebaseApp = null;
    private FirebaseDatabase database = null;
    //private DatabaseReference reference = null;

    private void Start()
    {
        firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            DatabaseUrl = new System.Uri("https://dbd-project-a8b3b-default-rtdb.firebaseio.com/")
        });
        this.database = FirebaseDatabase.GetInstance(firebaseApp);

        database.SetPersistenceEnabled(false);
        database.RootReference.KeepSynced(false);

        lm = LogoutManager.Instance;

        if (lm != null)
        {
            Debug.Log("정상");
            if (string.IsNullOrEmpty(lm.loginID))
            {
                Debug.Log("로그인");
                DisplayOnOff(true);
            }
            else
            {
                Debug.Log("로그아웃");
                checkText.text = lm.loginID;
                DisplayOnOff(false);
            }
        }

    }

    public void DisplayOnOff(bool _check)
    {
        idIF.gameObject.SetActive(_check);
        passIF.gameObject.SetActive(_check);
        createBt.gameObject.SetActive(_check);
        logoutBt.gameObject.SetActive(!_check);
        selectMurder.gameObject.SetActive(!_check);
        selectSurvivor.gameObject.SetActive(!_check);
        loginBt.gameObject.SetActive(_check);
        shopBt.gameObject.SetActive(!_check);
    }
    public async void CreateButton()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (idIF.text != null && passIF.text != null)
        {
            UserInfo user = await ReadUserInfos("users", idIF.text);

            if (user.id == null)
            {
                CreateUserWithJson(idIF.text, new UserInfo(idIF.text, passIF.text, "false", 0f, 0f, 0f, 0f));
                Debug.Log("Success Create");
                checkText.text = "CreateID";
                OpenNoticeWindow("Creat ID");
            }
            else
            {
                Debug.Log("Fail Create");
                checkText.text = "Fail Creat";
                OpenNoticeWindow("Creat Fail");
            }
        }
        idIF.text = null;
        passIF.text = null;
    }

    public async void LoginButton()
    {
        AudioManager.audioInstance._audioSource.Play();

        if (!string.IsNullOrEmpty(idIF.text) && !string.IsNullOrEmpty(passIF.text))
        {
            UserInfo user = await ReadUserInfos("users", idIF.text);

            if (user.id == null)
            {
                checkText.text = "Login Fail";
                OpenNoticeWindow("Login Fail");
            }
            else if (user.login == "true")
            {
                checkText.text = "Login Fail";
                OpenNoticeWindow("Login Fail");
            }
            else if (user.password != passIF.text)
            {
                checkText.text = "Login Fail";
                OpenNoticeWindow("Login Fail");
            }
            else if (user.password == passIF.text)
            {
                OpenNoticeWindow("Login Success");
                checkText.text = user.id;
                UpdateLoginInfo(user.id, "true");
                PhotonNetwork.NickName = user.id;
                lm.loginID = user.id;
                lm.money = user.money;
                lm.healItem = user.heal;
                lm.lightItem = user.light;
                lm.axeItem = user.axe;
                DisplayOnOff(false);
            }
        }
    }

    public void LogoutButton()
    {
        AudioManager.audioInstance._audioSource.Play();

        UpdateLoginInfo(lm.loginID, "false");
        DisplayOnOff(true);
        checkText.text = "로그인 하세요";
        OpenNoticeWindow("Logout");
        idIF.text = null;
        passIF.text = null;
        lm.loginID = null;
    }

    public void CreateUserWithJson(string _userID, UserInfo _userInfo)
    {
        string json = JsonUtility.ToJson(_userInfo);
        database.RootReference.Child("users").Child(_userID).SetRawJsonValueAsync(json);
    }

    public void UpdateLoginInfo(string _userID, string _login)
    {
        database.RootReference.Child("users").Child(_userID).Child("login").SetValueAsync(_login);
    }

    public async Task<UserInfo> ReadUserInfos(string _dataSet, string _userID)
    {
        DatabaseReference uiReference = database.GetReference(_dataSet);
        UserInfo CoincideUser = new UserInfo("","","",0f,0f,0f,0f);

        await uiReference.GetValueAsync().ContinueWith(
             task =>
             {
                 if (task.IsFaulted)
                 {
                     return;
                 }

                 else if (task.IsCompleted)
                 {
                     DataSnapshot snapshot = task.Result;

                     if (snapshot.Child(_userID).Exists)
                     {

                         Debug.Log("Join");
                         IDictionary userInfo = (IDictionary)snapshot.Child(_userID).Value;
                         CoincideUser.id = userInfo["id"].ToString();
                         CoincideUser.password = userInfo["password"].ToString();
                         CoincideUser.login = userInfo["login"].ToString();
                         CoincideUser.money = float.Parse(userInfo["money"].ToString());
                         CoincideUser.heal = float.Parse(userInfo["heal"].ToString());
                         CoincideUser.light = float.Parse(userInfo["light"].ToString());
                         CoincideUser.axe = float.Parse(userInfo["axe"].ToString());
                     }
                     else
                     {
                         Debug.Log("Fail");
                         CoincideUser.id = null;
                         CoincideUser.password = null;
                         CoincideUser.login = null;
                         CoincideUser.money =0f;
                         CoincideUser.heal = 0f;
                         CoincideUser.light = 0f;
                         CoincideUser.axe = 0f;
                     }
                 }
             });
        return CoincideUser;
    }

    public void RemoveUserInfo(string _userID)
    {
        database.RootReference.Child("users").Child(_userID).RemoveValueAsync();
    }

    private void OpenNoticeWindow(string _text)
    {
        noticeWindow.GetComponentInChildren<Text>().text = _text;
        noticeWindow.SetActive(true);
    }

    public void CloseNoticeWindow()
    {
        noticeWindow.SetActive(false);
    }
}
