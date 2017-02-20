using UnityEngine;
using System.Collections;

[System.Serializable]
public struct BuyApiRequest {
    public string Sid; // session id
    public string Rct; // receipt
    public string Sig; // signature
    public string Cur; // currency code
    public int    Prc; // price
}

[System.Serializable]
public struct Receipt {
    public string Payload;
}

[System.Serializable]
public struct PayloadReceipt {
    public string json;
    public string signature;
}

[System.Serializable]
public class BuyApiResponse : ServerData {
    public BuyData data;
}

[System.Serializable]
public class BuyData {
    public AccountData Acc; // account info
    public string Pid; // product name
}

public class BuyEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    void Start() {
        var buy = main.network.apiCmd.GetApiEvent(Api.CmdName.Buy);

        buy.OnRespond += (sender, e) => Respond(e.Payload);
        buy.OnError += (sender, e) => Error(e.Type, e.Message);
    }

    void Update() {}

    public void ByCrystall() {
        main.purchase.BuyProduct(PurchaseableProductNames.Crystal_1);
    }

    public static void OnBuy(string receipt, string code, decimal price) {
        var main = MAIN.getMain;

        var rct = JsonUtility.FromJson<Receipt>(receipt);
        var payload = JsonUtility.FromJson<PayloadReceipt>(rct.Payload);
        Debug.Log("JSPayload: " + payload.json);

        var buy = new BuyApiRequest();
        buy.Rct = payload.json;
        buy.Sig = payload.signature;
        buy.Sid = main.sessionID;
        buy.Cur = code;
        buy.Prc = (int)(price * 100);

        main.network.ApiRequest(Api.CmdName.Buy, JsonUtility.ToJson(buy));
    }

    void Respond(string payload) {
        BuyApiResponse response = JsonUtility.FromJson<BuyApiResponse>(payload);
        if (response.res == Api.ServerErrors.E_OK) { 
            main.purchase.ConfirmPurchase(response.data.Pid);
            main.addToMyRubins(response.data.Acc.Crystal - main.rubins.getValue());
        } else {
            main.purchase.ClearPurchase();
            Errors.showError(response.res, GameScene.MAIN_MENU);
        }
    }

    void Error(Api.ErrorType type, string message) {
        //Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.MAIN_MENU);
        /*var errWnd = */Errors.show(Errors.connectErrorText);
        Debug.Log("Error: " + type + ", msg: " + message);
    }
}
