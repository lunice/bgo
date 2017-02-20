using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
// Класс отвечающий за аунтификацию клиента через ВК
public class VKontakte : MonoBehaviour {

    public static AndroidJavaClass activityClass;
    public static AndroidJavaClass unityActivityClass;
    public static AndroidJavaObject activity;
    private IntPtr JavaClass;

    public static AuthUserInfo user;
    MAIN main = MAIN.getMain;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static AuthUserInfo getVkUserInfo() {
        return user;
    }
    // когда получили сессионный ключь ВК
    public void onLoginComplete(string msg) {
        //isReady = false;
        if (msg != null && msg == "true") {
            //print("onLoginComplete: success: " + msg);
            OnClickProfile();
        } else {
            main.setMessage("onLoginComplete: error: " + msg);
        }
    }
    // когда получили данные об игроке от ВК
    public void onProfileComplete(string msg) {
        if (msg != null && (msg != "" || msg != "false")) {
            //MAIN main = 
            //main.setMessage("onProfileComplete: success: " + msg);
            user = JsonUtility.FromJson<AuthUserInfo>(msg);
            //main.sessionID = PlayerPrefs.GetString(AuthType.VK.ToString(), "");
            if (main.sessionID == "") AuthEvent.OnAuthVk(user, AuthTypes.Vk);
            else AuthEvent.onQuickAuthVk(user, AuthTypes.Vk);
        } else {
            main.setMessage("onProfileComplete: error: " + msg);
        }
    }

    public void onError(string msg) {
        main.setMessage("onError: " + msg); // пока просто выводить перечень ошибок
        Autorization.authenticationAsGuest();
    }

    public void OnClickLogin() {
        //MAIN.getMain.setMessage("[OnClickLogin]");
#if UNITY_ANDROID && !UNITY_EDITOR
        activityClass = new AndroidJavaClass("com.ilot.bingogo.VkLoginActivity");
        unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
        activityClass.CallStatic("Login", activity, this.name, "onLoginComplete", "onError");
#else
        onError("NOT (UNITY_ANDROID && !UNITY_EDITOR)");
#endif
    }

    public void OnClickLogout() {
        //MAIN.getMain.setMessage("[OnClickLogout]");
#if UNITY_ANDROID && !UNITY_EDITOR
        activityClass = new AndroidJavaClass("com.ilot.bingogo.VkLoginActivity");
        unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
        activityClass.CallStatic("Logout", activity);
#else
        onError("NOT (UNITY_ANDROID && !UNITY_EDITOR)");
#endif
    }

    public void OnClickProfile() {
        //MAIN.getMain.setMessage("[OnClickProfile]");
#if UNITY_ANDROID && !UNITY_EDITOR
        activityClass = new AndroidJavaClass("com.ilot.bingogo.VkLoginActivity");
        unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
        activityClass.CallStatic("Profile", activity, this.name, "onProfileComplete", "onError");
#else
        onError("NOT (UNITY_ANDROID && !UNITY_EDITOR)");
#endif
    }

    public static bool OnClickStatus() {
        //MAIN.getMain.setMessage("[OnClickStatus]");
#if UNITY_ANDROID && !UNITY_EDITOR
        activityClass = new AndroidJavaClass("com.ilot.bingogo.VkLoginActivity");
        unityActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = unityActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
        bool loggedIn = activityClass.CallStatic<bool>("Status", activity);
        Debug.Log("LoggedIn is: " + loggedIn);
        return loggedIn;
#endif
        return false;
    }
    

    //bool isPaused = false;
    //bool isReady = false;

    //void OnGUI() {
    //    if (isPaused)
    //        GUI.Label(new Rect(100, 100, 50, 50), "Game paused");
    //}

    //void OnApplicationFocus(bool hasFocus) {
    //    isPaused = !hasFocus;
    //    Debug.Log("OnApplicationFocus: paused: " + isPaused + ", hasFocus: " + hasFocus);
    //}

    //void OnApplicationPause(bool pauseStatus) {
    //    isPaused = pauseStatus;
    //    Debug.Log("OnApplicationPause: paused: " + isPaused);
    //    if (!isPaused && !isReady) {
    //        isReady = true;
    //        OnClickLogin();
    //    }
    //}


}
