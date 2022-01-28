using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;

public class LogoutManager : Singleton<LogoutManager>
{
    private DatabaseReference reference = null;

    public string loginID = null;
    public float money = 0f;
    public float healItem = 0f;
    public float lightItem = 0f;
    public float axeItem = 0f;

    //public static LogoutManager instance = null;

    public FirebaseApp firebaseApp = null;
    public FirebaseDatabase database = null;

    private void Awake()
    {
        ////if (instance == null)
        ////{
        ////    instance = this;
        ////}
        ////else if (instance != this)
        ////{
        ////    Destroy(this.gameObject);
        ////}
        ////DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        //reference = FirebaseDatabase.DefaultInstance.RootReference;

        firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            DatabaseUrl = new System.Uri("https://dbd-project-a8b3b-default-rtdb.firebaseio.com/")
        });
        this.database = FirebaseDatabase.GetInstance(firebaseApp);
        database.SetPersistenceEnabled(false);
        database.RootReference.KeepSynced(false);
    }

    public void Logout(string _userID, string _login)
    {
        database.RootReference.Child("users").Child(_userID).Child("login").SetValueAsync(_login);
        loginID = null;
        money = 0f;
        healItem = 0f;
        lightItem = 0f;
        axeItem = 0f;
    }

    private void OnApplicationQuit()
    {
        CheckLogin();
    }

    public void CheckLogin()
    {
        if (!string.IsNullOrEmpty(loginID))
        {
            Logout(loginID, "false");
        }
    }

    public void BuyItemFuc()
    {
        database.RootReference.Child("users").Child(loginID).Child("money").SetValueAsync(money);
        database.RootReference.Child("users").Child(loginID).Child("heal").SetValueAsync(healItem);
        database.RootReference.Child("users").Child(loginID).Child("light").SetValueAsync(lightItem);
        database.RootReference.Child("users").Child(loginID).Child("axe").SetValueAsync(axeItem);
    }

    public void UseItemFuc()
    {
        database.RootReference.Child("users").Child(loginID).Child("heal").SetValueAsync(healItem);
        database.RootReference.Child("users").Child(loginID).Child("light").SetValueAsync(lightItem);
        database.RootReference.Child("users").Child(loginID).Child("axe").SetValueAsync(axeItem);
    }


    public void AddMoney(int _money)
    {
        float sumMoney = _money + money;
        database.RootReference.Child("users").Child(loginID).Child("money").SetValueAsync(sumMoney);
    }
}
