using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public struct PlayApiRequest {
    public ushort Cid; // combination id (1-1250), 0 - random
    public ushort Cnt; // ticket count
    public ushort Rid; // room id 
    public string Sid; // session id
}

public class PlayEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    void Start () {
        if (main.gameMode != GameMode.SERVER)
            return;

        var play = main.network.apiCmd.GetApiEvent(Api.CmdName.Play);
        
        play.OnRespond += (sender, e) => Respond(e.Payload);
        play.OnError += (sender, e) => Error(e.Type, e.Message);
    }
	
	void Update () {

	}

   public static void OnPlay() {
        var main = MAIN.getMain;
        if (main.isWaitingReplyAboutRaffleFromServer) {
            Errors.showTest("Error! [Ball Respond] request already sended!");
            return;
        }
        var play = new PlayApiRequest();
        play.Cid = 0;//617;//0; //102;
        play.Cnt = (ushort)Rooms.countTickets;
        play.Rid = Rooms.currentRoom.Id;
        play.Sid = main.sessionID;
        main.network.ApiRequest(Api.CmdName.Play, JsonUtility.ToJson(play));
        main.isWaitingReplyAboutRaffleFromServer = true;
    }


    void Respond(string payload) {
        //print("█ payload:"+payload);
        main.isWaitingReplyAboutRaffleFromServer = false;
        main.handlerServerData.receiveNewRaffleData(payload);
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        //Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.RAFFLE);
        var errWnd = Errors.show(Errors.connectErrorText2);
        errWnd.setAction(0, () => { ScenesController.loadScene(GameScene.BUY_TICKETS); });
        /*main.setMessage(errorMessage);
        Debug.Log(errorMessage);
        main.jsonHandler.loadServerRaffleFromFile(1); // start from 1 (not from 0)*/
        main.isWaitingReplyAboutRaffleFromServer = false;
    }
}
