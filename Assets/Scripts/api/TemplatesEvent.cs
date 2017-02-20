using UnityEngine;
using System.Collections;

//================ структура запроса ================
[System.Serializable]
public class TemplatesApiRequest {
    public uint Ver;    // Templates Version
    public short Rid;   // Room id (номер комнаты)
}

public class TemplatesEvent : MonoBehaviour {
    MAIN main = MAIN.getMain;
    
    void Awake() {
        if (main.gameMode == GameMode.SERVER) { 
            var templates = main.network.apiCmd.GetApiEvent(Api.CmdName.Templates);

            templates.OnRespond += (sender, e) => Respond(e.Payload);
            templates.OnError += (sender, e) => Error(e.Type, e.Message);

            //upateTemplates(1, 0);
        }
    }

    // Update is called once per frame
    void Update() {}

    public static void upateTemplates(short idRoom, uint version) {
        var main = MAIN.getMain;
        var templates = new TemplatesApiRequest();
        templates.Ver = version;
        templates.Rid = idRoom;
        main.network.ApiRequest(Api.CmdName.Templates, JsonUtility.ToJson(templates));
    }


    void Respond(string payload) {
        //Debug.Log("Templates respond: " + payload);
        main.handlerServerData.loadTemplatesData(payload);
    }

    void Error(Api.ErrorType type, string message) {
        Debug.Log("Error: " + type + ", msg: " + message);
        var errWnd = Errors.show(Errors.connectErrorText,"Повтор");
        errWnd.setAction(0, () => { TemplatesEvent.upateTemplates(1, 0); });
                                                 //Errors.showError(Errors.TypeError.ES_CONNECT_ERROR, GameScene.MAIN_MENU);
                                                 //main.jsonHandler.loadServerTempaltesFromFile();
        }
}
