using UnityEngine;
using Api;

[System.Serializable] 
public class TimeMarathonLevels {
    public int Level;   // уровень
    public int Time;    // время ожидания до следующего уровня, в минутах
    public string Item; // тип бонуса
    public int Count;   // кол-во бонуса
}

[System.Serializable]
public class TimeMarathonCurrentPosition {
    public int Level;       // текущий уровень
    public int TimeLeft;    // время ожидания до следующего уровня, в минутах
}

[System.Serializable]           // данные марафона по времени
public class TimeMarathon {
    public int Repeat;                          // время ожидания до следующего повтора, в минутах
    public TimeMarathonLevels[] Levels;         // набор данных по уровням
    public TimeMarathonCurrentPosition Current; // текущая позиция 
    public int Gold;                            // баланс золота
    public int Crystal;                         //баланс кристаллов
}

[System.Serializable]          // данные марафона звезд
public class StarMarathon { 
    public int Stars;   // кол-во звезд
    public int Level;   // текущий уровень
    public int Left;    // уровней до конца
    public string Item; // тип бонуса
    public int Count;   // кол-во бонуса
}


[System.Serializable]
public class MarathonApiRequest {
    public string Sid; // session id
    public string Type; // session id
}

[System.Serializable]  
public class MarathonTimeLoad {
}

[System.Serializable]   
public class MarathonStarLoad {
}

[System.Serializable]
public class MarathonsLoadApiRequest : MarathonApiRequest {
    public MarathonTimeLoad Time;
    public MarathonStarLoad Star;
}

[System.Serializable]
public class MarathonTimeUpdate {
    public int Level;
}

[System.Serializable]
public class MarathonStarUpdate {
}

[System.Serializable]
public class MarathonTimeUpdateApiRequest : MarathonApiRequest {
    public MarathonTimeUpdate Time;
}

[System.Serializable]
public class MarathonStarUpdateApiRequest : MarathonApiRequest {
    public MarathonStarUpdate Star;
}

[System.Serializable]
public class MarathonResponseData {
    public TimeMarathon Time;
    public StarMarathon Star;
}

[System.Serializable]
class MarathonApiResponse : ServerData{
    public MarathonResponseData data;
}

public class MarathonEvent : MonoBehaviour
{
    private const string MarathonActionGet = "get";
    private const string MarathonActionSet = "set";

    // for test
    public static int Level = 1;
    public void UpdateTimeMarathon() {
        Debug.Log("click");
        updateTimeMarathon(Level);
    }
    public void UpdateStarMarathon() {
        Debug.Log("click");
        updateStarMarathon();
    }
    public void LoadMarathonsInfo() {
        Debug.Log("click");
        loadMarathonsInfo();
    }

    MAIN main = MAIN.getMain;

    void Start () {
        if (main.gameMode != GameMode.SERVER) return;
        var account = main.network.apiCmd.GetApiEvent(Api.CmdName.Marathon);

        account.OnRespond += (sender, e) => Respond(e.Payload);
        account.OnError += (sender, e) => Error(e.Type, e.Message);
    }

    public static void loadMarathonsInfo(){
        var main = MAIN.getMain;
        var marathon = new MarathonsLoadApiRequest();
        marathon.Sid = main.sessionID;
        marathon.Type = MarathonActionGet;
        main.network.ApiRequest(Api.CmdName.Marathon, JsonUtility.ToJson(marathon));        
    }

    public static void updateTimeMarathon(int level) {
        var main = MAIN.getMain;
        var marathon = new MarathonTimeUpdateApiRequest();
        marathon.Sid = main.sessionID;
        marathon.Type = MarathonActionSet;
        marathon.Time.Level = level;
        main.network.ApiRequest(Api.CmdName.Marathon, JsonUtility.ToJson(marathon));
    }

    public static void updateStarMarathon() {
        var main = MAIN.getMain;
        var marathon = new MarathonStarUpdateApiRequest();
        marathon.Sid = main.sessionID;
        marathon.Type = MarathonActionSet;
        main.network.ApiRequest(Api.CmdName.Marathon, JsonUtility.ToJson(marathon));
    }

    void Respond(string payload) {
        Debug.Log("Respond: " + payload);
        MarathonApiResponse response = JsonUtility.FromJson<MarathonApiResponse>(payload);
        if (response.res != 0) {
            if (response.res == ServerErrors.E_MARATHON_TIME) {
                // еще не вышло время ожидания
            }
            if (response.res == ServerErrors.E_MARATHON_LEVEL) {
                // неверно указан уровень или текущий уровень непозволяет выполнить оперцию
            }

            Errors.showError(response.res, GameScene.UNDEF);
            return;
        }

        if (response.data.Star != null) {
            Marathon.updateStar(response.data.Star); // обрабатываем ответ марафона звезд
        }

        if (response.data.Time != null) {
            Marathon.updateTime(response.data.Time); // обрабатываем ответ марафона по времени
        }
    }

    void Error(Api.ErrorType type, string message) {
        Debug.Log("Error: " + message);
    }
}
