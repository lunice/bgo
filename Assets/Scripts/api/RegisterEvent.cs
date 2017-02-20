using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class RegisterApiRequest {
    public string Did; // device id
}

[System.Serializable]
public class RegisterData {
    public string Aid; // application id
}

[System.Serializable]
public class RegisterApiResponse : ServerData {
    public RegisterData data;
}

public class RegisterEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    public delegate void OnRegisterDone();
    protected OnRegisterDone callback;
    public void subscribe(OnRegisterDone NewCallBackFunction) { callback = NewCallBackFunction; }

    void Start() {
        var register = main.network.apiCmd.GetApiEvent(Api.CmdName.Register);
        register.OnRespond += (sender, e) => Respond(e.Payload);
        register.OnError += (sender, e) => Error(e.Type, e.Message);
    }

    static public void OnRegister() {
        var main = MAIN.getMain;
        var register = new RegisterApiRequest();
        register.Did = SystemInfo.deviceUniqueIdentifier;
        main.network.ApiRequest(Api.CmdName.Register, JsonUtility.ToJson(register));
    }

    void Respond(string payload) {
        RegisterApiResponse response = JsonUtility.FromJson<RegisterApiResponse>(payload);
        if (response.res != 0 ) { 
            Errors.showError(response.res, GameScene.AUTORIZATION);
            return;
        }
        main.applicationID = response.data.Aid;
        PlayerPrefs.SetString("ApplicationID", main.applicationID);

        if (callback!=null) callback();
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.AUTORIZATION);
    }
}
