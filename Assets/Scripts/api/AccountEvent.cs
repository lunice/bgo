using UnityEngine;
using System.Collections;

[System.Serializable]           // данные аккаунта
public class AccountData {
    public int Gold;
    public int Crystal;
    public int Xp;
    public short Level;
    public int GamesPlayed;
}
[System.Serializable]
class ServerAccountData : ServerData{
    public AccountData data;
}

public class AccountEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;
    [System.Serializable]
    public struct AccountApiRequest {
        public string Sid; // session id
    }
    public delegate void OnAccountReceive(AccountData account);
    protected OnAccountReceive callback;
    public void subscribe(OnAccountReceive NewCallBackFunction) { callback = NewCallBackFunction; }

    void Start () {
        if (main.gameMode != GameMode.SERVER) return;
        var account = main.network.apiCmd.GetApiEvent(Api.CmdName.Account);

        account.OnRespond += (sender, e) => Respond(e.Payload);
        account.OnError += (sender, e) => Error(e.Type, e.Message);
    }

    public static void requestAccountInformation(){
        var main = MAIN.getMain;
        if (main.isWaitingReplyAboutAccountFromServer) {
            Errors.showTest("Error! [requestAccountInformation] request already sended!");
            return;
        }
        var account = new AccountApiRequest();
        account.Sid = main.sessionID;
        main.network.ApiRequest(Api.CmdName.Account, JsonUtility.ToJson(account));
        main.isWaitingReplyAboutAccountFromServer = true;
        
    }

    void Respond(string accountString) {
        main.handlerServerData.loadAccount(accountString);
        main.isWaitingReplyAboutAccountFromServer = false;
        //main.accountData = JsonUtility.FromJson<AccountData>(accountString);
        if (callback != null) callback(main.accountData);
        
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        Errors.showTest(errorMessage);
        main.isWaitingReplyAboutAccountFromServer = false;
    }
}
