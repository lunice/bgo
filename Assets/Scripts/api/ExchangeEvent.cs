using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
// Ниже структура JSON для данного класса
[System.Serializable]
public struct ExchangeApiRequest {
    public string Sid; // session id
    public int Cnt;   // multiplier
    public ExchangeItem From; // item which is exchanged
    public ExchangeItem To; // element to which the exchange
}

[System.Serializable]
public class ExchangeApiResponse : ServerData {
    public AccountData data; // account info
}

[System.Serializable]
public class ExchangeItem {
    public short Id;
    public string Name;
}

// Класс отвечающий за взаимодействие с сервером предназначенный для обмена золота
public class ExchangeEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    void Start() {
        var exchange = main.network.apiCmd.GetApiEvent(Api.CmdName.Exchange);
        exchange.OnRespond += (sender, e) => Respond(e.Payload);
        exchange.OnError += (sender, e) => Error(e.Type, e.Message);
        //print(exchange.OnRespond);
        //if (exchange.OnRespond == null) { }
    }

    void Update() {

    }

    public void Exchange() {
        ExchangeItem from = new ExchangeItem { Id = 3, Name = "crystal_1" };
        ExchangeItem to = new ExchangeItem { Id = 2, Name = "gold_10" };
        int count = 1;
        OnExchange(from, to, count);
    }

    public static void OnExchange(ExchangeItem from, ExchangeItem to, int count) {
        var main = MAIN.getMain;

        var exchange = new ExchangeApiRequest();
        exchange.Sid = main.sessionID;
        exchange.Cnt = count;
        exchange.From = from;
        exchange.To = to;
        main.network.ApiRequest(Api.CmdName.Exchange, JsonUtility.ToJson(exchange));
    }

    void Respond(string resString) {
        ExchangeApiResponse response = JsonUtility.FromJson<ExchangeApiResponse>(resString);
        if (response.res == 0) {
            //main.addToMyMoney(response.data.Gold - main.money.getValue());
            if (response == null) { Debug.Log("response == null"); }
            if (response.data == null) { Debug.Log("response.data == null"); }
            if (main.money == null) { Debug.Log("main.money == null"); }
            if (main.raffle == null) { Debug.Log("main.raffle.flyExchengeMoneyToPocket == null"); }


            HUD.getHUD.flyExchengeMoneyToPocket = response.data.Gold - main.money.getValue();
            main.addToMyRubins(response.data.Crystal - main.rubins.getValue());
            Flickering.set(main.rubins.transform.parent.gameObject,1.0f,1);
        } else Errors.showError(response.res, GameScene.MAIN_MENU);
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.MAIN_MENU);
    }
}
