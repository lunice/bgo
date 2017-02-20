using UnityEngine;
using System.Collections;

[System.Serializable]
public struct BallApiRequest {
    public ushort Bal;
    public string Sid;
}

[System.Serializable]
public class AdditionalBall {
    public JsonHandler.BallJSON[] B;   // Balls
    public JsonHandler.PaysJSON P;     // pays
    public int W;                     // win
}

[System.Serializable]
public class BallData {
    public AdditionalBall Ball;   // Balls
    public int Gold;                     // win
    public int Xp;
    public int Lvl;
}

[System.Serializable]
public class ServerBallData : ServerData {
    public BallData data;
}

public class BallEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    void Start() {
        if (main.gameMode == GameMode.SERVER) { 
            var ball = main.network.apiCmd.GetApiEvent(Api.CmdName.Ball);

            ball.OnRespond += (sender, e) => Respond(e.Payload);
            ball.OnError += (sender, e) => Error(e.Type, e.Message);
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public static void getBall() {
        //print("[getBall]");
        var main = MAIN.getMain;
        if (main.isWaitingReplyAboutBallsFromServer) {
            Errors.showTest("Error! [Ball Respond] request already sended!");
            return;
        }
        var ball = new BallApiRequest();
        ball.Bal = 1;
        ball.Sid = main.sessionID;
        main.network.ApiRequest(Api.CmdName.Ball, JsonUtility.ToJson(ball));
        main.isWaitingReplyAboutBallsFromServer = true;
    }


    void Respond(string payload) {
        if (main.isWaitingReplyAboutBallsFromServer) {
            main.isWaitingReplyAboutBallsFromServer = false;
            //Debug.Log("Ball Respond: " + payload);
            main.handlerServerData.receiveAdditionalBall(payload);
        }
        else Errors.showTest("Error! [Ball Respond] message is not waiting!");
    }

    void Error(Api.ErrorType type, string message) {
        Debug.Log("Error: " + type + ", msg: " + message);
        //Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.RAFFLE);
        var errWnd = Errors.show(Errors.connectErrorText2);
        errWnd.setAction(0, () => { ScenesController.loadScene(GameScene.BUY_TICKETS); });
        main.isWaitingReplyAboutBallsFromServer = false;
    }
}
