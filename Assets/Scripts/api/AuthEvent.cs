using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AuthTypes {
    public static readonly string Vk = "vk";
}

[System.Serializable]
public class SidVkAuthApiRequest{
    public AuthUserInfo Ui;
    public string Sid; // session vk id
}
[System.Serializable]
public class SidGuestAuthApiRequest {
    public string Sid; // session id
}

[System.Serializable]
public class GuestAuthApiRequest {
    public string Aid; // application id
}

[System.Serializable]
public class AuthApiRequest : GuestAuthApiRequest {
    public AuthUserInfo Ui; // user info
    public string Type; // auth type
}

[System.Serializable]
public struct VkObject{
    public int id;
    public string title;
}

[System.Serializable]
public class AuthUserInfo {
    public string last_name;
    public string first_name;
    public Int64 id; // user id
    public short sex;
    public string bdate; // birthday
    public VkObject city; //city
    public VkObject country; //country
}

[System.Serializable]
public class AuthData {
    public string Sid; // session id
}

[System.Serializable]
public class AuthApiResponse : ServerData {
    public AuthData data;
}


public class AuthEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;
    
    public delegate void OnAuthenticationDone(string sessionID);
    protected OnAuthenticationDone callback;
    public void subscribe(OnAuthenticationDone NewCallBackFunction) { callback = NewCallBackFunction; }

    void Start() {
        var auth = main.network.apiCmd.GetApiEvent(Api.CmdName.Auth);

        auth.OnRespond += (sender, e) => Respond(e.Payload);
        auth.OnError += (sender, e) => Error(e.Type, e.Message);
    }

    void Update() {

    }

    public static void OnAuthVk(AuthUserInfo userInfo, string type) {
        var main = MAIN.getMain;

        var auth = new AuthApiRequest();
        auth.Aid = main.applicationID;
        auth.Ui = userInfo;
        auth.Type = type;
        main.network.ApiRequest(Api.CmdName.Auth, JsonUtility.ToJson(auth));
    }
    public static void onQuickAuthVk(AuthUserInfo userInfo, string type) {
        //print();
        var main = MAIN.getMain;

        var auth = new SidVkAuthApiRequest();
        auth.Sid = main.sessionID;
        auth.Ui = userInfo;

        main.network.ApiRequest(Api.CmdName.Auth, JsonUtility.ToJson(auth));
    }

    public static void OnSidAuth(string guestSessionID) {
        var auth = new SidGuestAuthApiRequest();
        auth.Sid = guestSessionID;
        MAIN.getMain.network.ApiRequest(Api.CmdName.Auth, JsonUtility.ToJson(auth));
    }

    public static void OnGuestAuth() {
       MAIN main = MAIN.getMain;
       var auth = new GuestAuthApiRequest();
       auth.Aid = main.applicationID; // "88a82d19-08a0a282-40552701-44500410";
       main.network.ApiRequest(Api.CmdName.Auth, JsonUtility.ToJson(auth));
    }

    void Respond(string payload) {
        AuthApiResponse response = JsonUtility.FromJson<AuthApiResponse>(payload);
        main.sessionID = response.data.Sid;
        
        if (response.res == Api.ServerErrors.E_APP_NOT_FOUND) {
            RegisterEvent.OnRegister();
            return;
        }

        if (main.tryRestoreSessionID && response.res == Api.ServerErrors.E_SESSION_EXPIRED) {
            print("Error:E_SESSION_EXPIRED! type auth:" + main.authType);
            main.tryRestoreSessionID = false;
            switch (main.authType) {
                case AuthType.GUEST: OnGuestAuth(); break;
                case AuthType.VK: OnAuthVk(VKontakte.user, AuthTypes.Vk); break;
                default: Debug.Log("undefine AuthType:" + main.authType); break;
            }
            return;
        }

        if (response.res == Api.ServerErrors.E_OK){
            if (callback != null) callback(response.data.Sid);
            else Errors.showTest("[AuthEvent,] callback == null");
        } else Errors.showError(response.res, GameScene.AUTORIZATION);
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        Errors.showError(Errors.TypeError.ES_CONNECT_ERROR,GameScene.AUTORIZATION);
    }
}
