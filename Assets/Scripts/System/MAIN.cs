//////////////////////////////////////////////////////////////////////////////////////////////////
// █ главный синглтон, в котором хранятся:
// - основные модули а именно ссылки на них
// - основные игровые режимы
// █ многое уже не используется, к примеру режимы игры  GameMode всегда SERVER,

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public enum GameMode{       // желательно вывести из проекта
    CLIENT_GENERATE = 0,    // устарелый режим игры, одиночный, где клиент сам генерил билеты
    JSON_FILE,              // устарелый режим игры, одиночный, где клиент поддтягивал розыгрыша из билета в формате JSON
    JSON_FILE_IN_ANDROID,   // тестовый режим игры такой же как предведущий только для андродида
    SERVER                  // █ единственно рабочий режим игры, многопользовательский
}
public enum AuthType {
    GUEST,                  // тип авторизации - гость
    VK                      // тип авториазции - вк
}

public class MAIN {
    public const bool IS_TEST = true;   // █ ВАЖНО! эта переменна обозначает тестовое состояние всего клиента, к ней привязан ряд процессов для тестирования, детали - по поиску.
    public const bool isTutorialEnable = true;

    public enum GameState {         // Практически не используется, желательно вывести из проекта
        MENU,                           // состояние игры - меню
        RAFFLE                          // состояние игры - розыгрышь
    }
    public static UInt32 ApplicationVersion = 0;   // версия приложения

    // ---------------- main variables
    public GameMode gameMode = GameMode.SERVER;     // устарелый режим клиента, для многопользовательской и одиночной игры. Поскольку у нас только многопользовательская, то это состояние то при дальнейшей разработке проверки на это состояния не ставились. Так что если такой режим будет внедрятся, лучше отрефакторить, удалить везде это состояние и заново прописать...
    public GameState mainState = GameState.RAFFLE;  // Вместо GameState был создан отдельный независемый синглтон ScenesController, а для розыгрыша соответственно: raffleState. Потому здесь желательно так же отрефакторить, удалить эту переменную, и в условиях где она используется, использовать эти новые переменные.
    //public TemplatesFrom templetesFrom = TemplatesFrom.CLIENT_OLD;  // так же не используемая переменная, 
    // ---------------- internal system variables ------------------
    string _sessionID;
    public string sessionID // █ сюда помещается ключь текущей игровой сессии. (Нет проверки на её валидность по времени)
    {
        get { return _sessionID; }
        set {
            if (IS_TEST)
            {
                //Application.
                Debug.Log("### SessionID chenge on:\"" + value + "\" from:\"" + _sessionID + "\" (auth type:" + authType + ")");
                //Debug.
            }
            _sessionID = value;
        }
    }
    /*void HandleLog(string logString, string stackTrace, LogType type){
        output = logString;
        stack = stackTrace;
    }*/

    public string applicationID;// = "";            // сюда помещается ID устройства.
    public AuthType authType;                       // типа аунтификации GUEST / VK / ...
    public bool tryRestoreSessionID = false;        // при аунтификации, и наличии ID эта переменна помечается как true для, попытки быстрой аунтификации. Передавая соответственно этот ай ди и помня что сей час проходит попытка его восстановления, в противном случае выставляется false и вызывается полная аунтификация ( с получением нового ключа )
    public const float coordSystemCoef = 0.01f;     // не везде используется, на через этот кофициент производится првидение размеров текстур к размерам в Unity
    public const float mouseCoef = 0.25f;           // коэфициент для перемещения объектов в пространстве на движение мыши. Используется только для ползунка, это кривая логика, по правельному нужно перемещать ручку ползунка на позиции по факту где находится курсор мыши, а не наскольколь он сместился...
    public const float timeOutTutorialUnLock = 4.0f;// таймаут для туториала, при котором разрешается скрыть окно сообщения, нажимая на любую часть экрана
    public float ticketScale = 1.25f;               // для подгонки размеров билетов

    public float timeLastFilingBall = 0;            // Время выпадения последнего шара, это для системы подачи шаров, это время + задержка до выпадения следущего шара в апдейте розыгрыша, производится проверка, для вычилсения прошедшего времени. И сравнении с задержкой... (стоило бы вынести в класс raffle )
    bool isInit = false;                            // после успешной инициализации синглтона, должна быть тру, но нигде не используется
    public string deviceUniqueIdentifier = "";      // = SystemInfo.deviceUniqueIdentifier
    public List<string> message = new List<string>();   // Сюда помещатеся текст для вывода логово, в тестовом режиме. Очистка списка приводит к очистке на экране. Так как он обновляется каждый игровой тик ( так устроена старая GUI Unity )
                 // ════════════════════════════ const system game values ════════════════════════════
    //public const string jsonFileFilePath = Application.dataPath + "/Resources/combinations.txt";
    //public const string digitsBlackFilePath = "Assets/Resources/digitsBlack.psd";
    //public const string digitsWhiteFilePath = "Assets/Resources/digitsWhite.psd";
    public bool isShowWarningWindow;                // показывать ли предупреждающее окно при покупке рубинов
    public const int totalCountBalls = 75;          // (используется формально) Максимальное число шаров, и номеров на билете. Раньше использовалось при генерации розыгрышей, сей час для перепроверки серверных данных...
    public int maxCountTrayedFreeBalls = 30;        // (не используется) Число высыпаемых шаров. 
    public int maxCountTrayedAdditionalBalls = 10;  // (не используется) Число шаров высыпаемых в лоток
    public int countHorseshoeInTicket = 2;          // (не используется) максимальное число подков в одном билете

    public Vector2 indentTempletesCells = new Vector2( 0.2f, 0.2f); // отступы между клетками в билете (стоило бы вынести в TicketHolder)
    public float timeTemplateVariablesShow = 0.15f; // Время отображения шаблона в категории. В верхней панеле ожидаемого выиграша
	public float templatesWidth = 1.173f;           // Отступ между шаблонами тех самых описаных на строчку выше
	public float ticketWidth = 3.45f;               // Отступ между билетами.
	public float indentTicketDigets = 0.17f;        // Отступ между цифрами в клетках билетов
    //public const int maxCountTickets = 4;
    public const int ticketCountColumns = 5;        // количетсво столбцов в одном билете
    public const int ticketCountRows = 5;           // количетсво рядов в одном билете
    public const int countTicketNumbersInColum = totalCountBalls / ticketCountRows; // (используется формально, для перепроверки сервера, но ошибок не выводит... ) Припадаемое количество цифр в одмом ряду к примеру на 75 шаров на столбец припадает по 15 чисел. Потому во втором стобце может быть числа от 16 до 30...
    //public int countTickets;
    public MarketPurchase[] marketPurchaser = null; // Список покупаемых билтов в магазине
    //public int actualInputLayer = defaultLayer;     // █ текущий актуальный слой, для регуляции нажимаемости объектов мышью
    // ════════════════════════════ const game values ════════════════════════════
    //public const int defaultLayer = 1025;           // Слой по умолчанию ( слои идут побитно )
    //public const int popUpWindowLayer = 1536;       // Слой всплывающих окон ( так же побитовая маска )
    public const int startMoney = 2000;             // (не используется)
    public const int ticketCost = 1; // TO DO       // (не используется)
    public const int lineWin1x = 50;                // (не используется) награда за одну линию
    public const int lineWin2x = 150;               // (не используется) награда за две линии
    public const int lineWin3x = 300;               // (не используется) награда за три линии
    public const int ballCost1x = 10;               // (не используется) цена за докупаемый шар. цена умнож на 2 за каждый нов шар.

    public float timeDelayFilingBalls = 0.3f;       // █ задержка в секундах, при подаче шаров. Этим параметрочм и регулируется скорость подачи шаров
    public float minSpeedBallsBeforBornStarts = 5.0f; // #V здесь указывается минимальная скорость движения шаров по лотку, ниже которой, шар считается отсановленный и запускается процесс отмечания номеров на билетах(появление звёзд)

    public bool isWaitingReplyAboutBallsFromServer = false; // (используется формально) ожидается ли ответ сервера на дополнительные шары. Во избежания повторных отправок
    public bool isWaitingReplyAboutRaffleFromServer = false;  // (используется формально) ожидается ли розыгрышь
    public bool isWaitingReplyAboutAccountFromServer = false;  // (используется формально) ожидается ли аккаунт
    public bool isWaitingReplyAboutTemplatesFromServer = false;  // (используется формально) ожидается ли шаблоны
    public bool isWaitingReplyAboutMarket = false; // (используется формально) ожидается магазин

    // ========================= main methods ======================
    private static MAIN manager = null;             // сам синглтон и ниже его получение
    public static MAIN getMain {
		get {
			if ( manager == null ) { manager = new MAIN(); }
			if ( !manager.isInit ) manager.isInit = manager.init();
            return manager;
		}
	}
    bool init(){
        switch (gameMode) {
            case GameMode.CLIENT_GENERATE: {} break;
            case GameMode.JSON_FILE: {} break;
            case GameMode.SERVER: {
                    _network = new Network();
                    _network.Init();
                }
                break;
        }
        return true;
    }
    public void changeGameMode(GameMode newGameMode) {
        gameMode = newGameMode;
    }
    
    // ====================== access functions =====================
    // ----------------- prefabs
    Ticket _ticketPrefab;           // префаб билетов
    GameObject _ballPrefab;         // префаб шара
    // ----------------- modules
    JsonHandler _jsonHandler;       // JSON reader
    HandlerServerData _handlerServerData; // держатель данных сервера, туда слаживается всё что получено от него. Оттудаже и берётся.
    //AdminPanel _adminPanel;         // несделанная клиентская панель для геймдиза ( при наличии множества ресурсов и кнопок, сделать её уже будет проще )
    Network _network;               //
    Purchaser _purchase;            // 
    AccountData _accountData;       // данные аккаунта в формате JSON, получив от сервера слаживаются прямо сюда
    public Raffle raffle;           // модуль сцены розыграша(ниже модули этой сцены)
    ReceivingTray _receivingTray;   // модуль лотка, барабана и шаров 
    TicketsHolder _ticketsHolder;   // модуль билетов
    TemplatesHolder _templatesHolder;//модуль шаблонов
    RESOURCES _resources;           // часть унифицированых игровых ресурсов
    DigitsLabel _money = null;      // для быстрого доступа к: монет в HUD
    //DigitsLabel _xp = null;       // для быстрого доступа к: опыта в HUD
    DigitsLabel _rubins = null;     // для быстрого доступа к: рубинов в HUD
    DigitsLabel _costNextBall = null;// для быстрого доступа к: стоимость следующего шара, на кнопке докупки шаров в HUD
    // Ниже вышеописаные модули ( сделано излишне, но так исторически сложилось )
    public AccountData accountData    { get { return _accountData; }   set { _accountData = value; } }
    public Network network            { get { return _network; }       set { _network = value; } }
    public Purchaser purchase         { get { return _purchase; }      set { _purchase = value; } }
    public RESOURCES resources        { get { return _resources; }     set { _resources = value; } }
    public ReceivingTray receivingTray{ get { return _receivingTray; } set { _receivingTray = value; } }
    public TicketsHolder ticketHolder { get { return _ticketsHolder; } set { _ticketsHolder = value; } }
    public Ticket ticketPrefab        { get { return _ticketPrefab; }  set { _ticketPrefab = value; } }
    public GameObject ballPrefab      { get { return _ballPrefab; }    set { _ballPrefab = value; } }
    public JsonHandler jsonHandler    { get { return _jsonHandler; }   set { _jsonHandler = value; } }
    //public AdminPanel adminPanel      { get { return _adminPanel; }    set { _adminPanel = value; } }
    public TemplatesHolder templatesHolder{ get{ return _templatesHolder; } set { _templatesHolder = value; } }
    public HandlerServerData handlerServerData{ get { return _handlerServerData; } set { _handlerServerData = value; } }
    public DigitsLabel money {
        get {
            if (_money == null) {
                var myMoney = GameObject.Find("myMoney");
                _money = myMoney.GetComponent<DigitsLabel>();
            }
            return _money;
        }
    }
    public DigitsLabel rubins{
        get{
            if (_rubins == null) {
                var myRubins = GameObject.Find("myRubins");
                _rubins = myRubins.GetComponent<DigitsLabel>();
            }
            return _rubins;
        }
    }
    public DigitsLabel costNextBall {
        get {
            if (_costNextBall == null) {
                var myMoney = GameObject.Find("ballCost");
                _costNextBall = myMoney.GetComponent<DigitsLabel>();
            }
            return _costNextBall;
        }
    }
    //==========================[ MAIN GAME PROCESSES ]==========================
    // перезапуск розыгрыша
    public void restartGame() { raffle.reStart(); }
    // запуск розыгрыша. Для удобного доступа из любого места кода
    public void startGame() { raffle.setState(RaffleState.PROCEED); }
    // Просто удобный доступ к ресурсам
    public RESOURCES getResources(){
        GameObject resGO = GameObject.Find("RESOURCES");
        return resGO.GetComponent<RESOURCES>();
    }
    // установка игрового времени
    public static void setGameTimeSpeed( float value, float delay = 0.1f ) {
        var system = GameObject.Find("System");
        var gSystem = system.GetComponent<GameSystem>();
        if (gSystem == null) gSystem = system.AddComponent<GameSystem>();
        gSystem.setGameTimeSpeed(value, delay);
    }
    // востанновеление игрового времени в дефолтный ( Time.scale == 1 )
    public static void restoreGameTime(){
        var system = GameObject.Find("System");
        var gSystem = system.GetComponent<GameSystem>();
        if (gSystem == null) gSystem = system.AddComponent<GameSystem>();
        gSystem.restoreTime();
    }
    //--- editing controlls 
    // Включение отключение кнопок, сделано доволно попросотому, стоило бы доработать
    public void setEnableBtn(string name, bool val = true) {
        //Debug.Log("[setEnableBtn] name:"+ name+" val="+ val);
        GameObject go = GameObject.Find(name);
        if (go) {
            BaseController btn = go.GetComponent<BaseController>();
            if (btn) btn.setEnable(val);
        } else {
            //Debug.Log("Error! [setEnableBtn] button with name:" + name + " not find! or not create already");
        }
    }
    // (не испольузется) при использовании старых GUI менялась подпись кнопок в зависимости от состояния игры, от этого менялось и их функциональность
    public bool changeNameBtnOn(string from, string to) {
        //print("[changeNameBtnOn] from:" + from + " to:" + to);
        GameObject go = GameObject.Find(from);
        if (go) {
            BaseController btn = go.GetComponent<BaseController>();
            //ObjectCaption oc = go.GetComponent<ObjectCaption>();
            if (btn) {
                btn.name = to;
                //oc.caption = to;
                return true;
            }
        }
        return false;
    }
    // (не используется) практически такое же назначение как и у предведущей функции, только меняет не имя а подпись кнопки
    public void setCaptionBtn(string name, string caption){
        GameObject go = GameObject.Find(name);
        if (go){
            BaseController btn = go.GetComponent<BaseController>();
            ObjectCaption oc = go.GetComponent<ObjectCaption>();
            if (btn && oc) oc.caption = caption;
        }
    }
    // функции изменения значений денег, рубинов и некоторых цен на некоторых объектах
    public void updateMyRubins()                { rubins.setValue(handlerServerData.crystal); }
    public void updateMyRubins(int newValue)    { rubins.setValue(newValue); }
    public void addToMyRubins(int addValue)     { rubins.setValue(rubins.getValue() + addValue); }
    public void addToMyMoney(int addValue)  { money.setValue(money.getValue() + addValue); }
    public void updateMyMoney()             { money.setValue(handlerServerData.gold);  }
    public void updateMyMoney(int newValue) { money.setValue(newValue); }
    public void updateCostNextBall(int v)   { costNextBall.setValue(v); }
    // ================ Вместо калбеков ==================
    // старая логика, по которой нажатие кнопок обрабатывалось здесь, в мейне, частично здесь осталась
    // при таскании ползунка скорости подачи шаров в меню настроек
    public void onSliderValueUpdate(string name, float val) {
        switch (name) {
            case "speedBallsSlider": { timeDelayFilingBalls = val; } break;
        }
    }
    // В момент покупки билетов, и перед началом розыгрыша
    /*public void onBuyTickets() {
        var raffleScene = GameObject.Find("RaffleScene");
        var buyTickets = GameObject.Find("BuyTickets");
        buyTickets.SetActive(false);
        raffleScene.SetActive(true);
    }*/
    // При нажатиях различных кнопок BaseController или унаследованных от них.
    public void onButtonClick(BaseController btn, BaseController.TypeEvent type) {
        if (type != BaseController.TypeEvent.ON_MOUSE_CLICK) return;
        string name = btn.name;
        //Debug.Log(name);
        switch (name) {
            //case "BuyTicket": { onBuyTicket(); } break;
            //case "BuyTickets":{ onBuyTickets(); } break;
            case "moneyBtn": WindowController.showPopUpWindow(WindowController.TypePopUpWindow.GOLD_EXCHANGE, true); break;
            case "rubinsBtn": WindowController.showPopUpWindow(WindowController.TypePopUpWindow.CRYSTALS_BUY, true); break; 
            case "settingsBtn":WindowController.showPopUpWindow(WindowController.TypePopUpWindow.SETTINGS, true); break;
            case "expBtn": /* ---------[ TODO exp ]---------- */ break;
            case "backBtn": ScenesController.onBackBtn(); break;
            case "buyBallBtn": { 
                    if (gameMode == GameMode.SERVER ) {
                        if (handlerServerData.isAvailableNextBall)
                            if (money.getValue() >= handlerServerData.costNextBall){
                                BallEvent.getBall();
                                HUD.setEnableBackButton(false);
                                HUD.hideBuyBallBtn();
                            }
                            else HUD.playAnimNeedMoreMoney();
                        //addToMyMoney(-costNextBall.getValue());
                    } else receivingTray.buyBall();
                } break;
            case "Exit": { ScenesController.showWindowExit(); } break;
        }
    }
    // В момент покупки билетов, и перед началом розыгрыша
    public void onBuyTicket() {
        int _ticketCost = ticketCost;
        if (gameMode == GameMode.JSON_FILE || gameMode == GameMode.SERVER)
            _ticketCost = ticketCost * 4;
        ticketHolder.createNewTicket();
    }

    // =================================[ работа с файлами ]=================================
    public void loadData(){
        //totalCountBalls 	= PlayerPrefs.GetInt("_coutTileObjY",totalCountBalls);
        //money = 2000;//PlayerPrefs.GetInt("PlayerMoney", startMoney);
        isShowWarningWindow = PlayerPrefs.GetInt("isShowWarningWindow", 1) > 0;
#if UNITY_EDITOR
        // более ускоренная подача шаров только в юнити редакторе. (приводит к баге непоследовательности подачи шаров)
        timeDelayFilingBalls = 0.066f;
        minSpeedBallsBeforBornStarts = 10.0f;
#else
        timeDelayFilingBalls = PlayerPrefs.GetFloat("SpeedGenerateBalls", timeDelayFilingBalls);
#endif
        //raffleMode = (GameMode)PlayerPrefs.GetInt("RafleMode",1);
    }
    // Операции перед выходом из приложения
    public static void exit() {
        var _this = MAIN.getMain;
        _this.saveData();
        //Debug.Log(Application.version);
        //Debug.Log(Application.unityVersion);
        Application.Quit();
        //Application.CancelQuit();

        if (Application.platform == RuntimePlatform.Android) { 
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("finishAndRemoveTask");
        }
    }
    // Сохранение данных перед выходом
	public void saveData(){
		//PlayerPrefs.SetInt("PlayerMoney", money );
        //PlayerPrefs.SetInt("GameMode", (int)gameMode);
        PlayerPrefs.SetFloat("SpeedGenerateBalls", timeDelayFilingBalls);
    }
    //================================================================
    // (не используется) В случае отсутствия связи с сервером загружались шаблоны генерируемые клиентом, и играть можно было в одиночную игру
    /*public void loadDefaultTemplates() {
        Debug.Log("█ █ █ [loadDefaultTemplates]");
        templatesHolder.removeAllTemplates();
        List<string> templatesStringL = new List<string>();
        templatesStringL.Add("{S\"углы\":C00400444,R50fs2}");           // углы и центр
        templatesStringL.Add("{B\"вертикаль\":C0010203040,R50n}");             // горизонталь
        templatesStringL.Add("{B\"горизонталь\":C0001020304,R50nm1}");           // вертикаль
        templatesStringL.Add("{U\"горизонталь или вертикаль\":\"горизонталь\"|\"вертикаль\",RV1,R50}");           // объединение вертикаль или горизонталь
        //templatesStringL.Add("{U\"горизонталь или вертикаль\":\"горизонталь\"|\"вертикаль\",RV2,R200}");           // объединение вертикаль или горизонталь
        //templatesStringL.Add("{U\"горизонталь или вертикаль\":\"горизонталь\"|\"вертикаль\",RV3,R500}");           // объединение вертикаль или горизонталь
        templatesHolder.loadTemplatesFromStrings(templatesStringL);
    }*/
    // визуальный вывод логов вверху экрана
    public void setMessage(string _message, bool isCriticalError = false) {
        Debug.Log(_message);
        message.Add(_message);
    }
    // визуальный вывод логов вверху экрана но только вместо добавления новой строки замещается последняя, нужно было для вывода каких-то процентных соотношений к примеру при загрузки клиента...
    public void setLastMessage(string _message) {
        if (message.Count > 0)  message[message.Count - 1] = _message;
        else setMessage(_message);
    }

    public static UInt32 MakeVersion() {
        string[] values = Application.version.Split('.');
        UInt32 version = 0;
        UInt32 major = UInt32.Parse(values[0]);
        UInt32 minor = UInt32.Parse(values[1]);
        UInt32 patch = UInt32.Parse(values[2]);

        version += major << 16;
        version += minor << 8;
        version += patch;
        return version;
    }
}