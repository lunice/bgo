using UnityEngine;
using System.Collections;

public enum RaffleState // состояния розыгрыша
{   
    BUY_TICETS,         // (не актуально) покупка билетов 
    WAIT,               // (не актуально) ожидается нажатие старта
    WAIT_SERVER_DATA,   
    PROCEED,
    FINISHED,
    ADDITIONAL_BALL,
    FINISH
}
//////////////////////////////////////////////////////////////////////
// Класс отвечающий за весь процесс розыгрыша. И его отображение

public class Raffle : MonoBehaviour {
    MAIN main = MAIN.getMain;
    //ScenesController scenesController = ScenesController.getScenesController;
    public RaffleState raffleState = RaffleState.WAIT_SERVER_DATA;  // текущее состояние розыгрыша
    Transform flingBalls;                   // сюда помещаются все летящия шары(ныне звёзды)

    void Awake() {
        main.raffle = this;
        flingBalls = transform.FindChild("flingBalls");
        //print("режим проверки шаблонов: "+main.templetesFrom);
        if (main.deviceUniqueIdentifier == "") {
            main.deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            main.deviceUniqueIdentifier = "\"" + main.deviceUniqueIdentifier + "\" len(" + main.deviceUniqueIdentifier.Length + ")";
        }
    }
    // Ниже две функции созданы для тестеров, кнопки возле панелей шаблонов, при нажатии имитируют выиграшь, сыпятся монеты в количестве 12(*5 = 60); 160(*5 = 800); 100(*5 = 5000)
    /*void initTestButtons(){
        var t1 = GameObject.Find("testWin1");
        var t2 = GameObject.Find("testWin2");
        var t3 = GameObject.Find("testWin3");
        if (t1 == null || t2 == null || t3 == null) return;
        if (MAIN.IS_TEST) {
            if (t1 == null) print("t1 == null");
            t1.GetComponent<PushedStonePlate>().subscribeOnControllEvents(onTestWin);
            t2.GetComponent<PushedStonePlate>().subscribeOnControllEvents(onTestWin);
            t3.GetComponent<PushedStonePlate>().subscribeOnControllEvents(onTestWin);
        } else {
            t1.SetActive(false);
            t2.SetActive(false);
            t3.SetActive(false);
        }
    }
    void onTestWin(BaseController btn, BaseController.TypeEvent te){
        if (te == BaseController.TypeEvent.ON_MOUSE_CLICK) {
            switch(btn.name) {
                case "testWin1" : flyWinMoneyToPocket += 60; break;
                case "testWin2" : flyWinMoneyToPocket += 1000; break;
                case "testWin3" : flyWinMoneyToPocket += 5000; break;
            }
        }
    }*/

    void Start() {
        //initTestButtons();
        reStart();
    }
    // ряд операций, при смене на соответсвующий стейт
    public void setState(RaffleState newState) {
        //print("[setState] newState: " + newState);
        switch (newState) {
            case RaffleState.WAIT: {
                    if (main.gameMode != GameMode.SERVER) {
                        main.changeNameBtnOn("Restart", "Start");
                        main.setEnableBtn("Start", true);
                        main.setEnableBtn("BuyTicket", true);
                        //main.setCaptionBtn("BuyTicket", "Buy Ticket: " + MAIN.ticketCost.ToString());
                        //main.setCaptionBtn("BuyBall", "Buy Ball: " + MAIN.ballCost1x.ToString());
                    }
                    main.setEnableBtn("Restart", true);
                    //main.setEnableBtn("BuyBallBtn", false);
                } break;
            case RaffleState.WAIT_SERVER_DATA: {
                    main.setEnableBtn("Restart", false);
                    main.setEnableBtn("Start", false);
                    //main.setEnableBtn("BuyBallBtn", false);
                    main.setEnableBtn("BuyTicket", false);
                } break;
            case RaffleState.PROCEED: {
                    main.changeNameBtnOn("Start", "Restart");
                    main.setEnableBtn("BuyTicket", false);
                    //Tutorial.show(TutorialSubject.TS_TEMPLEATES);
                } break;
            case RaffleState.FINISHED: {
                    //HUD.setEnableBackButton(true);
                    //if (main.handlerServerData.isAvailableNextBall) HUD.showBuyBallBtn();
                } break;
            case RaffleState.ADDITIONAL_BALL: {
                    startCheckTime = Time.time;
                    DragonHead.openMore();
                } break;
            case RaffleState.FINISH:{
                    //Tutorial.show(TutorialSubject.TS_BUY_GOLD_BTN);
                    getDrum().state = Drum.State.STOPING;
                    HUD.setEnableBackButton(true);
                    if (main.handlerServerData.isAvailableNextBall) {
                        HUD.showBuyBallBtn();
                        var flickering = Flickering.set(HUD.getBuyBallButton().gameObject, 1.5f); // #V мигание кнопки докупки шаров
                        flickering.setFlickeringUntilPress();
                    } else {
                        var flickering = Flickering.set(HUD.getBackButton().gameObject, 1.5f); // #V мигание кнопки выхода
                        flickering.setFlickeringUntilPress();
                        DragonHead.openFull();
                        main.receivingTray.setGravityForBalls(10);
                        TicketsHolder.startHideTickets();
                    }
                    Utils.screenShot("OnFinishRaffle.png"); // только в режиме тестировки
                }
                break;
        }
        raffleState = newState;
    }
    // перезапус розыгрыша
    public void reStart() {
        var ticketHolder = main.ticketHolder;
        var gameMode = main.gameMode;
        var receivingTray = main.receivingTray;
        startCheckTime = -1.0f;
        DragonHead.close(true);

        ticketHolder.removeAllTickets();
        receivingTray.removeAllBalls();
        receivingTray.resetBalls();
        //main.templatesHolder.expectedWin.setValue(0);

        getDrum().state = Drum.State.START;
        // устарелая логика для одиночной игры(которая уже никогда не используется), и после для мультиплеерной
        if ((gameMode == GameMode.JSON_FILE || gameMode == GameMode.JSON_FILE_IN_ANDROID || gameMode == GameMode.CLIENT_GENERATE)) {
            main.jsonHandler.getNextRuffle();
            print("█ Warnign! GameMode == " + gameMode);
            setState(RaffleState.WAIT);
        } else if (gameMode == GameMode.SERVER) {
            PlayEvent.OnPlay();
            main.handlerServerData.startNewRaffle();
            if (main.templatesHolder != null && main.templatesHolder.expectedWin != null)
                if ( main.templatesHolder.expectedWin.getValue() != 0 )
                    main.templatesHolder.expectedWin.setValue(0);
            setState(RaffleState.WAIT_SERVER_DATA);
        } else
            print("Error! [reStart] unknown GameMode:" + gameMode);
    }
    // При получения розыгрыша от сервера
    public void onReseiveServerData() {
        main.onBuyTicket();
        main.startGame();
        Tutorial.show(TutorialSubject.TS_TEMPLEATES);
    }
    // ( Не используется ) ранее эта функция перезапускала розыгрышь ещё в розыгрыше
    public void onReStartClick(BaseController btn, BaseController.TypeEvent type) {
        if (type != BaseController.TypeEvent.ON_MOUSE_CLICK) return;
        string name = btn.name;
        Debug.Log(name);
        switch (name) {
            case "Start": { main.startGame(); } break;
            case "Restart": { reStart(); } break;
        }
    }
    // удобный доступ к барабану
    public Drum getDrum() {
        var drumGO = main.receivingTray.transform.Find("Drum");
        return drumGO.GetComponent<Drum>();
    }

    float checkTimeWaitEndRaffle = 2.0f;    // (не актуален так же переменная ниже) костыль, которым были попытки закрыть, зависания игры при застрявании шара. Или при длительном его путишествии по лотку, запускалося таймер с указанным сдесь таймером, по истичению которого, этот шар принудительно выпускал звёзды, и розыгрышь заканчивался
    float startCheckTime = -1;              // так же относится к описанию на строчку выше
    // Для кнопки в найстройка DEBUG LOG, выводит состояния различных переменных так же вне этого класса
    public void testPrint() {
        // ("█ ================================================");
        main.message.Clear();
        main.setMessage("raffleState == " + raffleState);
        main.setMessage("flingBalls.childCount == " + flingBalls.childCount);
        main.receivingTray.testPitnt();
        main.ticketHolder.testPrint();
    }
    void Update() {
        // Кнопки для теста, только для редактора Unity
        if (MAIN.IS_TEST && Input.anyKeyDown == true ) {
            if (Input.GetKeyDown("tab")) testPrint();
            else if (Input.GetKeyDown("a")) main.money.setValue(5);
            //else if (Input.GetKeyDown("c")) flyWinMoneyToPocket += 100;
            else if (Input.GetKeyDown("1")) flyWinMoneyToPocket += 60;
            else if (Input.GetKeyDown("2")) flyWinMoneyToPocket += 600;
            else if (Input.GetKeyDown("3")) flyWinMoneyToPocket += 5000;
        }
        // Проверка на окончание высыпания свободных шаров
        if (raffleState == RaffleState.PROCEED && (!getDrum().gameObject.activeSelf || getDrum().state == Drum.State.ROLL) &&
            Time.time - main.timeLastFilingBall > main.timeDelayFilingBalls && main.ticketHolder.getTickets().Length > 0) {
            if (main.receivingTray.countBalls >= main.maxCountTrayedFreeBalls) {
                startCheckTime = Time.time;
                setState(RaffleState.FINISHED);
            } else main.receivingTray.createVisualBall();
        } //else if (raffleState == RaffleState.FINISHED && startCheckTime != -1 && startCheckTime - Time)
        // проверка на полное окончание розыгрыша
        if (flyWinMoneyToPocket == 0 && flingBalls.childCount == 0 && (raffleState == RaffleState.ADDITIONAL_BALL || raffleState == RaffleState.FINISHED)) {
            if ( main.receivingTray.isAllBallsStop() ) setState(RaffleState.FINISH);
        }
    }
    // Высыпались ли в лоток все свободные шары?
    public bool isShoweredFreeBalls() {
        return flingBalls.childCount == 0 && (raffleState == RaffleState.ADDITIONAL_BALL || raffleState == RaffleState.FINISHED);
    }
    // Ниже описано объявление и инициализация фонтана выиграшных монет
    FontaineCoins winFontainCoins = null;
    public int flyWinMoneyToPocket {
        get { return (winFontainCoins == null) ? 0 : winFontainCoins.getTotalCount(); }
        set {
            if (winFontainCoins == null){
                winFontainCoins = Effects.addFontaineCoins(main.templatesHolder.expectedWin.gameObject, main.money.gameObject, value);
                winFontainCoins.shiftStartPosOn(new Vector2(0.0f, -0.6f));
                winFontainCoins.setNominalCoin(5);
            } else winFontainCoins.setTotalCount(value);
            //Debug.Log("█ [flyWinMoneyToPocket] vaule == " + value);
            int totalValue = winFontainCoins.getTotalCount();
            if (totalValue > 400) SoundsSystem.play(Sound.S_BIG_WIN);
            else if (totalValue > 0) SoundsSystem.play(Sound.S_WIN);
        }
    }
    // При получении от сервера дополнительного шара
    public void onReceiveAdditionalBall(JsonHandler.BallJSON additionalBall){
        setState(RaffleState.ADDITIONAL_BALL);
        main.receivingTray.buyBall();
    }
    // Ниже две функции: при появлении нового шара в лотке
    public void onBornNewBall(Ball newBall, JsonHandler.BallJSON jsonBall){
        newBall.jsonBallInfo = jsonBall;
        onBornNewBall(newBall);
    } 
    public void onBornNewBall(Ball newBall) {
        //print("onBornNewBall:" + newBall.number + "main.ticketHolder.transform.childCount = "+ main.ticketHolder.transform.childCount);
        newBall.subscribeOnBallStop(onNewBallStop);
        Ticket[] tickets = main.ticketHolder.getTickets();
        for (int i = 0; (i < main.ticketHolder.transform.childCount ); i++){
            if (tickets[i] == null ) Errors.showTest("Ошибка! Билетов ещё нет, а шары уже сыпятся");
            //print("bornBall:"+ newBall.number+ ", tickets:" +tickets[i]);
            TicketCell tC = tickets[i].getCellByNum(newBall.number);
            //print(newBall.number);
            if (tC != null){
                newBall.setGreen();
                return;
            }
        }
    }   // проверка всех билетов
    // █ Функция ниже вызывается из самих шаров, при их столкновении. Соотвественно здесь производится проверка зелёный ли этот шар или белый, на каких билетах нужно отметить, пораждает звёзды и отправляет в добрый путь, подписываясь на их прибытие, в функции ниже
    public GameObject flyingBallsPrefab;
    // Калбэк приходящий непосредственно от самих шаров, при их столкновении с другими шарами, порождает звёзды, а сам шар переводит в состояние "остановлен"
    public void onNewBallStop(Ball newBall){
        main.receivingTray.setBallToStopingBalls(newBall.gameObject);
        Ticket[] tickets = main.ticketHolder.getTickets();
        var rbB = newBall.GetComponent<Rigidbody2D>();
        for (int i = 0; (i < main.ticketHolder.transform.childCount && tickets[i] != null); i++){
            TicketCell tC = tickets[i].getCellByNum(newBall.number);
            if (tC){
                GameObject flyBall;
                if (flyingBallsPrefab != null && flyingBallsPrefab.GetComponent<SpriteRenderer>().sprite != null)
                    flyBall = Instantiate(flyingBallsPrefab);
                else flyBall = Instantiate(newBall.gameObject);
                flyBall.transform.parent = flingBalls;
                Flying f = flyBall.AddComponent<Flying>();
                f.destroyOnArrive = false;
                f.slowdown(3.0f, 0.02f, 0.9f); // #V здесь торможение звёзд: 1 - дистанция с которой начинается торможение, 2 - до скорости, 3 - сила торможения(если 1 то нулевая сила, если 0 то мгновеная остановка, если больше одного наоборот ускорение)
                flyBall.transform.position = newBall.transform.position;
                var b = flyBall.GetComponent<Ball>();
                if (b != null) { 
                    flyBall.name = "flingBall_" + b.number;
                    b.setOrderLayer(9);
                }
                var circleCollider = flyBall.GetComponent<CircleCollider2D>();
                if (circleCollider !=null) circleCollider.enabled = false;
                var rb = flyBall.GetComponent<Rigidbody2D>();
                if (rb != null){
                    Destroy(rb);
                }
                var rotating = Rotating.set(flyBall, rbB.angularVelocity);
                rotating.slowdown(0.1f, 0.96f); // #V замедление вращение звёздочек
                f.init(tC.transform.position, 0.02f, newBall.prevVelocity[0] * 0.005f);
                f.subscribe(onBallArrive, tC.gameObject, newBall.gameObject);
                // #V эффекты на звёздочках:
                Flickering.set(flyBall, 0.15f);
                Scaling.set(flyBall, 0.5f, 0.15f);
            }
        }
    }
    // Вызывается когда звёздочка прилетает на билет. Запускается логика отмечания этого шара, проверка на наличие вина или превина, и их отмечание
    void onBallArrive(GameObject flingGO, GameObject ticketCell) {
        var f = flingGO.GetComponent<Flying>();
        if (f == null) return;
        GameObject fromGO = f.getStartFlyFromGameObject();
        if (ticketCell == null) return;
        Ball ball = fromGO.GetComponent<Ball>();
        if (ball == null ) print("Error! ball == null, fromGO.name:" + fromGO.name);
        var tC = ticketCell.GetComponent<TicketCell>();
        var t = tC.getMyTicket();
        tC.mark(TicketCell.TypeMark.PRESENT);
        float transformTime = 1.0f; // #V общее значение времени для затухания звезды и появления шара(используется чуть ниже)
        Flickering.stop(flingGO, true);
        var fade = Effects.addFade(flingGO, transformTime); // #V исчезновение звезды
        fade.subscribeOnEffectDone(onFadeStar);             // для удаления звезды, после её исчезновения
        SoundsSystem.play(Sound.S_STAR_TRANSFORM, ticketCell.transform.position);
        var tb = ticketCell.transform.FindChild("ticketBall");
        if (tb != null) Effects.addFade(tb.gameObject, transformTime, false); // #V появление шара
        if (main.gameMode != GameMode.SERVER){
            // (по факту не используется) 
            bool isHaveLine = t.checkOnWin(tC);
            if (isHaveLine) t.setPreWinsTest(); //main.updateWinMoney();
        } else {
            // проверка на наличие превинов и винов, и соответственном маркеровании клеток
            if (ball.jsonBallInfo.T == null ) return;
            int myIndex = getMyIndex(t, ball);
            if (myIndex == -1) return;
            JsonHandler.WinTickets tPWin = ball.jsonBallInfo.T[myIndex];
            var ticket = main.ticketHolder.getTicket(tPWin.N);
            if (tPWin.P != null) {
                ticket.setPreWins(tPWin.P);
                Tutorial.show(TutorialSubject.TS_PREVIN);
            }
            if (tPWin.W != null && tPWin.W.Length > 0) { ticket.setWins(tPWin.W); }
            main.templatesHolder.updateExpectedWinByNewPrewinsAndWins(tPWin.P, tPWin.W);
        }
    }
    // Это окончания работы эффекта: плавного сичезновения зведы, после чего она удаляется, подпись на эту функцию осуществляется отсюда: Raffle::onBallArrive()
    static void onFadeStar(BaseEffect starEffect) { Destroy(starEffect.target.gameObject); }
    // Возвращает порядковый номер шара в лотке
    int getMyIndex(Ticket t, Ball ball){
        for (int i = 0; i < ball.jsonBallInfo.T.Length; i++)
            if (ball.jsonBallInfo.T[i].N == t.number)
                return i;
        return -1;
    }
    // █ вывод визуальных логов вверху экрана (только при MAIN.IS_TEST == true)
    void OnGUI() {
        if (MAIN.IS_TEST && main.message.Count>0) { 
            float labelHeight = 20.0f;
            float s = Screen.width * 0.05f;
            float e = Screen.width * 0.9f;
            GUI.Box(new Rect(s, 0.0f, e, main.message.Count * labelHeight), "");
            for (int i = 0; i < main.message.Count; i++)
                GUI.Label(new Rect(s, Screen.height * 0.00f + i * labelHeight, e, labelHeight), "Last Message: " + main.message[i]);
        }
    }
}