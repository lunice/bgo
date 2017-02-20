using UnityEngine;
using System.Collections;

//================ структура запроса ================
[System.Serializable]
public class RoomsApiRequest {
    public uint Ver;    // Rooms Version
}

[System.Serializable]
public class RoomInfo {
    public byte   Id; // rooms id
    public string Name; // name
    public string Desc; // descriotion
    public byte   Level; // min plyaer level
    public float  XpMultiplier; // gold to xp multiplayer
    public byte   TicketMin; // min ticket count
    public byte   TicketMax; // max ticket count
    public short  TicketPrice; // ticket price
    public uint   Ver; // room version
}

[System.Serializable]
public class RoomsData {
    public uint       Ver;  // rooms version
    public RoomInfo[] Room; // rooms info
}

[System.Serializable]
public class RoomsApiResponse : ServerData {
    public RoomsData data;
}

public class RoomsEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;

    public delegate void OnRoomsReceive(RoomsData rooms);
    protected OnRoomsReceive callback;
    public void subscribe(OnRoomsReceive NewCallBackFunction) { callback = NewCallBackFunction; }

    void Awake() {
        if (main.gameMode == GameMode.SERVER) {
            var rooms = main.network.apiCmd.GetApiEvent(Api.CmdName.Rooms);

            rooms.OnRespond += (sender, e) => Respond(e.Payload);
            rooms.OnError += (sender, e) => Error(e.Type, e.Message);
        }
    }

    // Update is called once per frame
    void Update() { }

    public static void getRooms(uint version) {
        var main = MAIN.getMain;
        var rooms = new RoomsApiRequest();
        rooms.Ver = version;
        main.network.ApiRequest(Api.CmdName.Rooms, JsonUtility.ToJson(rooms));
    }

    void Respond(string payload) {
        //Debug.Log("Rooms respond: " + payload);
        RoomsApiResponse response = JsonUtility.FromJson<RoomsApiResponse>(payload);
        var version = response.data.Ver;

        if (response.res == Api.ServerErrors.E_OK) {
            if (callback != null) callback(response.data);
            else Errors.showTest("[RoomsEvent] callback == null");
        } else Errors.showError(response.res, GameScene.AUTORIZATION);
    }

    void Error(Api.ErrorType type, string message) {
        string errorMessage = "Error: " + type + ", msg: " + message;
        Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.AUTORIZATION);
        Debug.Log(errorMessage);
    }
}
