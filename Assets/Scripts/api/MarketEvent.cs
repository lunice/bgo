using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct MarketApiRequest {
}

[System.Serializable]
public class MarketApiResponse : ServerData {
    public MarketData data;
}

[System.Serializable]
public class MarketData {
    public MarketPurchase[] Purchase; // purchasable items
    public MarketExchange[] Exchange; // exchangeable items
}

[System.Serializable]
public class MarketPurchase {
	public short    Id; 
    public string   Name;
    public int      Free;
}

[System.Serializable]
public class ExchangedItemCount : ExchangeItem {
    public int Count;  // number of items for exchange
}

[System.Serializable]
public class MarketExchange {
    public ExchangedItemCount From; // item which is exchanged
    public ExchangedItemCount To;  // element to which the exchange
    public int                Free; // free product count
}

public class PurchaseableItem {
    public string   Name;  // product name
    public string   Currency; // product currency code
    public int      Free; // free product count
    public decimal  Price; // product price
}

public class MarketEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    public event EventHandler<EventArgs> OnReady;
    public MarketApiResponse response;

    public void Start() {
        var market = main.network.apiCmd.GetApiEvent(Api.CmdName.Market);

        market.OnRespond += (sender, e) => Respond(e.Payload);
        market.OnError += (sender, e) => Error(e.Type, e.Message);

        OnReady += (sender, e) => Ready();
    }
    
    public static void requestMarketItems(){
        MAIN main = MAIN.getMain;
        if (main.isWaitingReplyAboutMarket){
            Errors.showTest("Error! [Ball Respond] request already sended!");
            return;
        }
        var market = new MarketApiRequest();
        main.network.ApiRequest(Api.CmdName.Market, JsonUtility.ToJson(market));
        //Debug.Log("OnMarket");
    }

    void Update() {

    }

    /*public void OnMarket() {
        var market = new MarketApiRequest();
        main.network.ApiRequest(Api.CmdName.Market, JsonUtility.ToJson(market));
        Debug.Log("OnMarket");
    }*/

    public void Ready() {
        //Debug.Log("Ready");
    }

    void Respond(string payload) {
        response = JsonUtility.FromJson<MarketApiResponse>(payload);
        if ( response.res !=0 ) {
            Errors.showError(response.res);
            return;
        }
        main.purchase = new Purchaser();
        main.purchase.InitializePurchasing(response.data.Purchase, OnReady);
        main.marketPurchaser = response.data.Purchase;
    }

    public void testRespound(MarketPurchase[] mp) {
        print("test respoudn MarketEvent call!");
        MAIN main = MAIN.getMain;
        main.purchase = new Purchaser();
        main.purchase.InitializePurchasing(mp,OnReady);
        main.marketPurchaser = mp;
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        Errors.showError(Errors.TypeError.ES_CONNECT_ERROR,GameScene.MARKET);
    }
}
