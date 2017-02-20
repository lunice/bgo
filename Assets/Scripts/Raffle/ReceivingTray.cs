using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Класс - лоток (лототрон) Отвечает за работу лотка, шаров...
public class ReceivingTray : MonoBehaviour {
    MAIN main = MAIN.getMain;
    public GameObject ballPrefab;   // префаб шара
    JsonHandler.BallJSON[] freeBallsFromServer; // Свободные шары(30 шт), полученые от сервера
    List<JsonHandler.BallJSON> additionalBallsFromServer = new List<JsonHandler.BallJSON>(); // Дополнительные шары
    public Transform spawnPoint;    // Точка где создаются шары
    Transform movedBalls;           // контейнер, в который помещены шары со статусом "Движущие"
    Transform stopingBalls;         // контейнер, в который помещаются шары после остановки (порождения звёзд)
    public int countBalls = 0;      // текущее количество шаров в лотке
    int currentCountLottotronBalls; // Общий остаток шаров (в барабане) от 75 шаров после выпадения каждого шара...
    int[] lottotronBalls = new int[MAIN.totalCountBalls];   // все 75 шаров, отсюда изымаются выпавшие шары, и таким образом происходит перепроверка сервера, в плане повторяемости шаров

    void Awake() {
        MAIN main = MAIN.getMain;
        main.ballPrefab = ballPrefab;
        main.receivingTray = GetComponent<ReceivingTray>();
        movedBalls = GameObject.Find("movedBalls").transform;
        stopingBalls = GameObject.Find("stopingBalls").transform;
        spawnPoint = GameObject.Find("spawnPoint").transform;
    }
    // для вывода состояния некоторых переменных и объектов... (вызывается из Raffle)
    public void testPitnt(){
        main.setMessage("ReceivingTray:");
        main.setMessage("  --movedBalls.childCount:" + movedBalls.childCount);
        main.setMessage("  --stopingBalls.childCount:" + stopingBalls.childCount);
        for (int i = 0; i < movedBalls.childCount; i++) movedBalls.GetChild(i).GetComponent<Ball>().testPrint();
    }
    // все ли шарі остановлены
    public bool isAllBallsStop() {
        return movedBalls.childCount == 0;
    }
    // изменение гравитации шаров в лотке
    public void setGravityForBalls(float gravity) {
        for (int i = 0; i < stopingBalls.childCount; i++)
            stopingBalls.GetChild(i).GetComponent<Rigidbody2D>().gravityScale = gravity;
    }
    // выталкиваются шары по окончанию розыгрыша, и высыпании шаров, это включает дополнительную силу выталкивания шаров из лотка
    public static bool kickOutBalls = false;
    // █ приводит к полному обнулению шаров во всех масивах, и к готовности к новому розыгрышу
    public void resetBalls() {
        currentCountLottotronBalls = MAIN.totalCountBalls;
        freeBallsFromServer = null;
        additionalBallsFromServer.Clear();
        for (int i = 0; i < currentCountLottotronBalls; i++) {
            lottotronBalls[i] = i+1;
        }
    }
    // создать шары, по JSON данным (из сервера)
    public void setBallsFromServer(JsonHandler.BallJSON[] balls ) {
        int countBalls = balls.Length;
        if (countBalls > 1) {
            freeBallsFromServer = balls;
        } else if (countBalls == 1) {
            additionalBallsFromServer.Add(balls[0]);
        } else print("Error! [onRecievingBalls] count receiving balls == " + countBalls);
    }
    // █ изъять шар из барабана(полагаясь на заранее установленный список шаров, в предведущей функции). Не допускает возможным изымать дважды шар с одним и тем же номером, но в случае попытки выдаёт ошибку в тестовом режиме, приведёт к сбою работы розыгрыша.
    public int takeAwayLottotronBall() {
        int numBall;
        if (main.gameMode == GameMode.SERVER) {
            if (main.isWaitingReplyAboutBallsFromServer) {
                //Errors.show("Error![takeAwayLottotronBall] request for new Ball Already sended")
                return -1;
            }

            JsonHandler.BallJSON newBall = null;
            if (countBalls < main.maxCountTrayedFreeBalls) {
                newBall = main.handlerServerData.getBall(countBalls);
            }

            if (newBall != null) takeAwayLottotronBallByNum(newBall.N);
            else Errors.showTest("Error! [takeAwayLottotronBall] newBall == null");
        }
        else if (main.gameMode == GameMode.JSON_FILE ) {
            if (countBalls < main.maxCountTrayedFreeBalls) { 
                numBall = main.jsonHandler.getNumberBall(countBalls);
                takeAwayLottotronBallByNum(numBall);
            } else {
                numBall = takeAwayLottotronBall(Random.Range(0, currentCountLottotronBalls - 1));
            }
            countBalls++;
            return numBall;
        }
        countBalls++;
        return takeAwayLottotronBall(Random.Range(0, currentCountLottotronBalls - 1));
    }
    // изъять шар из барабана, по указанному номеру шара
    public bool takeAwayLottotronBallByNum(int numBall) {
        //print(currentCountLottotronBalls);
        for (int i=0; i < currentCountLottotronBalls; i++) {
            if (numBall == lottotronBalls[i]) { 
                takeAwayLottotronBall(i);
                return true;
            }
        }
        return false;
    }
    // изъять шар из барабана, по указанному порядковому номеру в общем общем массиве (в барабане(75 шаров))
    public int takeAwayLottotronBall(int ballNumInArray) {
        int res = lottotronBalls[ballNumInArray];
        lottotronBalls[ballNumInArray] = lottotronBalls[currentCountLottotronBalls-- - 1];
        return res + 1;
    }
    // Удалить шар, за его номером
    void removeBallByNum(int num) {
        int maxBalls = main.maxCountTrayedFreeBalls + main.maxCountTrayedAdditionalBalls;
    }
    // █ Докупить шар. Задизейбливает кнопку, а так же посылает запрос на сервер на получение нового дополнительного шара, (если текущем есть поментка на его доступность)
    public void buyBall() {
        int ballCost = 0;
        var gameMode = main.gameMode;
        //print("main.raffle.raffleState:"+main.raffle.raffleState);
        if (gameMode == GameMode.SERVER) {
            if (!main.isWaitingReplyAboutBallsFromServer) {
                //main.isWaitingReplyAboutBallsFromServer = true;
                if (!main.handlerServerData.isAvailableNextBall) {
                    //print("last Ball!");
                    //main.raffle.onFinishRaffle();
                    main.setEnableBtn("BuyBallBtn", false);
                }
            } else print("main.isWaitingReplyAboutBallsFromServer == true");
            //return;
        } else { //if (gameMode == GameMode.CLIENT_GENERATE || gameMode == GameMode.JSON_FILE || gameMode == GameMode.JSON_FILE_IN_ANDROID) {
            ballCost = MAIN.ballCost1x;
            for (int i = 0; i < countBalls - main.maxCountTrayedFreeBalls + 1; i++)
                ballCost *= 2;
            //main.setCaptionBtn("BuyBall", "Buy Ball: " + ballCost.ToString());
        }

        //main.addMoneyValue(-ballCost);

        if (main.raffle.raffleState == RaffleState.ADDITIONAL_BALL) {
            //Debug.Log("[Buy additional Ball!]");
            createVisualBall(true);
            if (main.maxCountTrayedFreeBalls + main.maxCountTrayedAdditionalBalls < countBalls) {
                main.setEnableBtn("BuyBall", false);
            }
        }
    }
    // взять шар из барабана, и создаёт визуальную модель шара. (Инициализирует его функциями описаными выше)
    Ball getNewBall(int num = -1) {
        GameObject go = Instantiate(main.ballPrefab);
        Ball ball = go.GetComponent<Ball>();
        ball.setNumber( (num >= 0 ) ? num : takeAwayLottotronBall());
        
        ball.transform.parent = movedBalls;
        return ball;
    }
    // (не используется) проверяет на наличие застряхших шаров.
    //public bool isHasStuckedBalls(){ return movedBalls.childCount > 0; }
    // (не используется) останавливает все застрявшие шары.
    /*public bool stopAllStuckedBalls() {
        //Errors.show("[stopAllStuckedBalls]");
        bool res = false;
        for (int i = 0; i < movedBalls.childCount; i++) { 
            main.raffle.onNewBallStop(movedBalls.GetChild(i).gameObject.GetComponent<Ball>());
            res = true;
        }
        //setBallToStopingBalls(movedBalls.GetChild(i).gameObject );
        return res;
    }*/
    bool isWinBall(JsonHandler.BallJSON ball) // несёт ли в себе шар выиграш
    {
        if (ball.T != null)
            for (int i = 0; i < ball.T.Length; i++){
                if (ball.T[i].W != null && ball.T[i].W.Length > 0) return true;
            }
        return false;
    }
    // переносит указаный шар, из контейнера движущих шаров, в контейнер остановленных
    public void setBallToStopingBalls(GameObject ball) { ball.transform.parent = stopingBalls; }
    // ( не оптимизировано, не лучший способ... ) Возвращает количество шаров в лотке
    public int getCountBallsInTray() { return GameObject.FindGameObjectsWithTag("ReceivingTrayBall").Length; }
    // █ создаёт, визуальную модель используя предведущие функции, инициализируют её... Часть логики устрала, так как одиночной игры уже нет
    public int createVisualBall(bool additional = false) {
        Ball newBall;
        if ( main.gameMode == GameMode.SERVER ) {
            JsonHandler.BallJSON ball = !additional ? main.handlerServerData.getBall() : main.handlerServerData.getLastAdditionalBall();
            if ( !takeAwayLottotronBallByNum(ball.N) ) {
                Errors.showTest("Error! [createNewBall] ball #" + ball.N + " already taken from lottotron!");
                return -1;
            }
            newBall = getNewBall(ball.N);
            countBalls++;
            main.raffle.onBornNewBall(newBall,ball);
            //print(countBalls == main.maxCountTrayedFreeBalls);
            /*if (countBalls == main.maxCountTrayedFreeBalls)
                if (!main.handlerServerData.isAvailableNextBall)
                    main.raffle.onFinishRaffle();*/
        } else {
            newBall = getNewBall();
            main.raffle.onBornNewBall(newBall);
        }
        newBall.gameObject.name = "Ball";
        main.timeLastFilingBall = Time.time;
        if (spawnPoint != null) newBall.transform.position = spawnPoint.position;
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D> ();
        float startSpeed = -1.1f / main.timeDelayFilingBalls;
        if (additional){
            bool isFake = Random.Range(0, 10) <= 3; // этот коэфициент влияет на скорость вылета шара, этот параметр влияет только на стартовую скорость шара, по дизайну есть 30% вероятность того что это будет фиктивное замедление стартовой скорости.
            if (isFake || this.isWinBall(newBall.jsonBallInfo)){
                startSpeed *= 0.1f;
                rb.gravityScale = 0.2f;
            }
        }
        int v = startSpeed < 0 ? -1 : 1;
        if (startSpeed * v < main.minSpeedBallsBeforBornStarts) {
            startSpeed = main.minSpeedBallsBeforBornStarts * v;
        }
        //print("startSpeed:" + startSpeed);
        rb.velocity = new Vector2 (0.0f, startSpeed);
        newBall.tag = "ReceivingTrayBall";
        return newBall.number;
    }
    // █ удаляет все визуальные шары
    public void removeAllBalls() {
        for (int i = 0; i < stopingBalls.childCount; i++) {
            Transform obj = stopingBalls.GetChild(i);
            if (obj.GetComponent<Ball>()) {
                Destroy(obj.gameObject);
            }
        }
        countBalls = 0;
    }

	void Start () {}
	void Update () {}
}
