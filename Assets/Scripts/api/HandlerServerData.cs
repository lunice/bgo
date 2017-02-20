using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ServerData         // Базовая структура сервеных данных
{
    public Api.ServerErrors res;
    public string text;
    //public PlayData data;
}
[System.Serializable]           // структура сервеных данных розыгрыша
public class ServerRaffleData : ServerData {
    public PlayData data;
}

[System.Serializable]           // структура розыгрыша
public class PlayData
{
    public JsonHandler.RaffleJSON Play;
    public int Gold;
}
// Класс сохраняет в себе полученные от сервера данные, обновляет состояния счетов, контролирует актуальность этих данных(недоделано)
public class HandlerServerData : MonoBehaviour {
    public enum TypeData // энум нужне только для данных розыгрыша, что бы отличить данные о дополнительном шаре или же это новый розыгрышь
    {
        RAFFLE, // розыгрышь
        BALL    // дополнительный шар
    }

    MAIN main = MAIN.getMain;
    public int gold;                // значение золота
    public int costCurrentBall;     // цена текущего дополнительного шара
    public int costNextBall;        // цена следующего дополнительного шара ███ (возможная уязвимость!) знание о цене следующего шара, может давать преимущетсва при решении докупки следующего шара
    public bool isAvailableNextBall;// доступен ли следующий шар ███ (возможная уязвимость!) так же, если кому-то удастся мониторить эти данные игрок будет знать доступен ли следующий шар, или же этот конечный
    public int crystal;             // значение баланса кристалов
    public int xp;                  // опыт

    JsonHandler.BallJSON[] balls;   // розыгрышне шары текущего розыгрыша (30 шт)
    List<JsonHandler.BallJSON> additionalBalls = new List<JsonHandler.BallJSON>();  // дополнительные, докупные шары
    int currentBall;                // █ текущий порядковый номер шара. Это внутрений каунтер, при обращении из класса розыгрыша, который просит просто следующий шар НЕ УКАЗЫАЯ его номер, этот класс его возвращает в зависисмоти от этого счётчика. Правильнее было бы что бы класс розыгрышь сам помнил какой шар ему нужен... Но почему-то мне казалось это удобно...
    JsonHandler.RaffleJSON currentRaffle; // текущий розыгрышь (в нём дублируются информация описана выше)

    public HandlerServerData() { Awake(); }
    void Awake() { main.handlerServerData = this; } // модуль прописывает себя в маин, откуда другие модули имеют к нему доступ
	void Start () { }
	void Update () { }
    public void startNewRaffle() {
        main.isWaitingReplyAboutRaffleFromServer = true; // ожидается ли ответ от сервера о новом розыгрыше
    }

    public void setRaffle(JsonHandler.RaffleJSON serverRaffle) // установить новый розыгыршь (после получения от сервера)
    {
        print("[setServerRaffleFromFile] ballsCount" + serverRaffle.B.Length);
        currentRaffle = serverRaffle;
        balls = currentRaffle.B;
        currentBall = 0;

        main.raffle.setState(RaffleState.WAIT_SERVER_DATA);
        main.isWaitingReplyAboutRaffleFromServer = false;
    }

    public void loadTemplatesData(string line) // получив от сервера шаблоны, запускается их инициализация в классе Rooms
    {
        //print("█ [loadNewData]" + line);
        ServerTemplatesData serverData = JsonUtility.FromJson<ServerTemplatesData>(line);
        main.isWaitingReplyAboutTemplatesFromServer = false;
        if (serverData.res != 0 ) {
            Errors.showError(serverData.res, GameScene.MAIN_MENU);
            return;
        }
        Rooms.get.serverTemplates = serverData.data;
        if (ScenesController.currentScene == GameScene.MAIN_MENU)
            ScenesController.loadScene(GameScene.BUY_TICKETS);
    }

    public void receiveNewRaffleData(string line) // (не актуально) В более поздних тестовых версияг где розыгрыши чистались из файла, была приспособлена такая функция позволяющая имитировать инициализацию серверных данных но только из файла 
    {
        //print("[receiveNewRaffleData]");
        ServerRaffleData serverData = JsonUtility.FromJson<ServerRaffleData>(line);
        //serverData.res = Api.ServerErrors.E_DB_ERROR;
        if (serverData.res != 0) {
            Errors.showError(serverData.res, GameScene.RAFFLE);
            return;
        }
        
        var data = serverData.data;
        JsonHandler.RaffleJSON play = null;
        if ( data.Play != null ) { 
            play = data.Play;
            if ( play.P != null ) { 
                costCurrentBall = play.P.C;
                costNextBall = play.P.N;
            } else { }
        } else {
            print("Error! [loadNewData] data.Play == null");
            return;
        }
        isAvailableNextBall = costNextBall > 0;
        //print("===================isAvailableNextBall:" + isAvailableNextBall);
        gold = data.Gold - play.W;
        main.updateCostNextBall(costNextBall);
        main.updateMyMoney(gold);
        currentRaffle = play;
        balls = currentRaffle.B;
        additionalBalls.Clear();
        currentBall = 0;
        main.raffle.setState(RaffleState.WAIT_SERVER_DATA);
        main.raffle.onReseiveServerData();
    }

    public void receiveAdditionalBall(string line) // при получении дополнительного шара (осуществляется визуализация этого процесса - последняя строка этой функции)
    {
        ServerBallData serverData = JsonUtility.FromJson<ServerBallData>(line);
        if ( serverData.res != 0 ) {
            Errors.showError(serverData.res, GameScene.RAFFLE);
            return;
        }
        var data = serverData.data;
        if (data != null) {
            costCurrentBall = data.Ball.P.C;
            costNextBall = data.Ball.P.N;
        } else {
            print("Error! [loadNewData] data == null");
            return;
        }
        isAvailableNextBall = costNextBall > 0;
        //print("===================isAvailableNextBall:" + isAvailableNextBall);
        //if (!isAvailableNextBall) {
            //HUD.ы();
            //HUD.setEnableBackButton(true);
        //}
        main.updateCostNextBall(costNextBall);
        gold = data.Gold - data.Ball.W;
        main.updateMyMoney(gold);
        additionalBalls.Add(data.Ball.B[0]);
        main.raffle.onReceiveAdditionalBall(data.Ball.B[0]);
        
    }

    public bool loadAccount(string account) // при получении данных об аккаунте. По сути лишняя функция, так как модуль (авторизации) аунтификации не удаляется и данные дублируются...
    {
        ServerAccountData sad = JsonUtility.FromJson<ServerAccountData>(account);
        if ( sad.res != 0 ) {
            Errors.showError(sad.res);
            return false;
        }
        AccountData serverData = sad.data;
        main.accountData = serverData;
        gold = serverData.Gold;
        xp = serverData.Xp;
        crystal = serverData.Crystal;
        //var hud = HUD.getHUD;
        if (HUD.getHUD != null) {
            main.money.setValue(gold);
            main.rubins.setValue(crystal);
        }
        /*print("crystal = " + crystal);
        print("gold = " + gold);
        print("xp = " + xp);*/
        return true;
    }

    public JsonHandler.TicketJSON[] getTickets() // возвращает билеты текущего розыгрыша
    {
        if (currentRaffle != null ) return currentRaffle.T;
        //print("Error! [getTickets] raffle == null ");
        Debug.Log("Error! [getTickets] raffle == null");
        return null;
    }

    public JsonHandler.TicketJSON getTicket(int numTicket) // возвращает билет текущего розыгрыша, за его номером, номером билета
    {
        if (currentRaffle != null)
            for (int i = 0; i < currentRaffle.T.Length; i++)
                if (currentRaffle.T[i].N == numTicket)
                    return currentRaffle.T[i];
        Debug.Log("Error! [getTickets] raffle == null");
        return null;
    }

    public JsonHandler.BallJSON getBall(int serialNumber = -1) // получить шар за его порядковым номером (-1 один означает получить следующий шар(каунтер которого, находится в этом классе))
    {
        return ( balls == null || balls.Length == 0) ? null :
            (serialNumber != -1) ? balls[serialNumber] : balls[currentBall++];
    }
    public JsonHandler.BallJSON getLastAdditionalBall() // выдаёт последний дополнительный шар из списка дополнительных шаров(т.е. текущий) по факту хранить их все в списке лишнее, можно их слаживать в одну переменную затирая предведущие...
    {
        if (additionalBalls.Count > 0)
            return additionalBalls[additionalBalls.Count - 1];
        return null;
    }
}
