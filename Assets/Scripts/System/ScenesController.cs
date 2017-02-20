using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
// перечень сцен Bingogo
public enum GameScene{
    UNDEF = 0,      // 
    START_LOGO,     // сцена лого
    GAME_LOADING,   // сцена загрузки игры
    AUTORIZATION,   // сцена авторизации
    MAIN_MENU,      // сцена лобби
    BUY_TICKETS,    // сцена покупки билетов
    RAFFLE,         // сцена розыгрыша
    MARKET          // такой сцены нету, но для обработок ошибок нужно
}

// Неудаляемый класс, отвечающий за переходы между игровыми сценами и крупными модулями
// █ не смотря на то что он унаследован от MonoBehaviour внутри есть статичная переменная ссылающаяя на этот калсс, это для удобного доступа по типу синглтона, потому при создании ещё одного экземпляра этого класс, приведёт к необратимым адски глючным последствим, не допускайте этого!
public class ScenesController : MonoBehaviour {
    MAIN main = MAIN.getMain;
    // этот список переменных нужен для помещения в них префабов, в режиме проектировки Unity
    public GameObject resourcesPrefab;      // ресурсы
    public GameObject autorizationPrefab;   // Авторизация
    public GameObject hudPrefab;            // HUD ( Всегда отображается вместе модулями перечисленными ниже )
    public GameObject mainMenuPrefab;       // Лобби
    public GameObject buyTicketsPrefab;     // Покупка билетов
    public GameObject rafflePrefab;         // Розыгрышь
    // переменные хранящие сцены и отдельные крупные модули игрыы, проинициализированых из списка выше
    GameObject hud;
    GameObject autorization;
    GameObject mainMenu;
    GameObject buyTickets;
    GameObject raffle;
    // Не визуальные внутрение модули, которые, возможно, стоило бы сделать независимыми...
    RESOURCES resources;
    GameObject network;
    public GameObject getNetwork(){
        if (network == null) network = GameObject.Find("Network");
        return network;
    }

    public static GameScene currentScene = GameScene.UNDEF; // текущая сцена
    public GameObject currentBackGround;    // текущий фон или его позиция под текущую сцену
    int countPosInBackGroun = 3;            // █ количество позиций по X по которым смещается фон при переходах между основными сценами ( учитывается размер экрана, и используется максимальные возможности длины всего фона ). Была попытка сделать гибкую систему где может быть сколько угодно фонов и у каждого может быть любой набор позиций. Но в систему не вписалась эта переменная, хотя это можно исправить, закоменируя её, и вытаския их количества из словарей описаных и проинициализированых ниже!
    Dictionary<GameScene, float> backGroundPositions = new Dictionary<GameScene, float>(); // все позиции фона для каждой сцены ( запихнул в словарь, под каждую сцену соответственную позицию, для быстрого доступа без лишних расчётов )
    Dictionary<GameScene, BackGroundGroup> backGroundGroups = new Dictionary<GameScene, BackGroundGroup>(); // список всех фонов с учётом того что у каждой из них может быть список позиций... ( но пока только для GAME )
    
    // калбеки для взаимодействия с другими модулями
    public delegate void OnStartLoadScene(GameScene newGameScene);
    protected List<OnStartLoadScene> onLoadSceneCallBacks = new List<OnStartLoadScene>();
    public void subscribeOnLoadScene(OnStartLoadScene newCallBack) { onLoadSceneCallBacks.Add(newCallBack); }
    public void unSubscribeOnLoadScene(OnStartLoadScene callBack)
    {
        if (onLoadSceneCallBacks.Contains(callBack))
            onLoadSceneCallBacks.Remove(callBack);
    }

    // перечень фонов
    enum BackGroundGroup {
        NONE,
        LOGO,
        AUTEREZATION,
        GAME
    }
    BackGroundGroup currentGroup = BackGroundGroup.LOGO; // текущий фон: по умолчанию лого

    float stepSize;             // размер шага ( для проигрования смещения при переходам между сценами, только для GAME )
    float timeStepPlay = 0.3f;  // скорость смещения фона
    float currentBackGroundPos; // текущая позиция фона от которой фон смещается
    float nextBackGroundPos;    // позиция фона к которой происходит смещение
    bool init() { // инициализация фонов и их возможных позиций
        backGroundGroups.Add(GameScene.UNDEF, BackGroundGroup.NONE);
        backGroundGroups.Add(GameScene.START_LOGO, BackGroundGroup.LOGO);
        backGroundGroups.Add(GameScene.GAME_LOADING, BackGroundGroup.GAME);
        backGroundGroups.Add(GameScene.AUTORIZATION, BackGroundGroup.AUTEREZATION);
        backGroundGroups.Add(GameScene.MAIN_MENU, BackGroundGroup.GAME);
        backGroundGroups.Add(GameScene.BUY_TICKETS, BackGroundGroup.GAME);
        backGroundGroups.Add(GameScene.RAFFLE, BackGroundGroup.GAME);
        return true;
    }
    bool isInit = false;
    private static ScenesController scenesController = null; // █ сюда помещается ссылка на этот класс, подразумивается что он будет создаваться один раз. И использоваться как синглтон
    public static ScenesController getScenesController{
        get{
            if (scenesController == null) scenesController = new ScenesController();
            if (!scenesController.isInit) scenesController.isInit = scenesController.init();
            return scenesController;
        }
    }
    /*public static GameScene currentSene { 
        get { return GameScene.RAFFLE; }  
    }*/

    void Awake() {
        GameObject.DontDestroyOnLoad(gameObject);                   // установка неудаляемости модуля
        if (resourcesPrefab != null) {                              // далее инициализация ресурсов и так же установка на неудаляемость
            var resGO = Instantiate(resourcesPrefab).gameObject;    
            resGO.name = "RESOURCES";
            resources = resGO.GetComponent<RESOURCES>();
            GameObject.DontDestroyOnLoad(resGO);
        }
        else Debug.Log("Error! [Awake] resourcesPrefab == null");
        scenesController = this;                                    // для быстрого и удобого доступа, к модулю, через статическую переменную
    }

    void Start() {
        loadScene(GameScene.AUTORIZATION);
        main.loadData();                // для загрузки из файла нужжных пользовательских данных и настроек.
    }

    /*
    public enum InputLayer // перечисление видов масок тачей, под разные окна/сцены
    {
        TM_DEFAULT = 1025,          // дефолтный слой GAME (все игровые сцены)
        TM_POP_UP_WINDOW = 1536,    // слой для popUp окна
        TM_TUTORIAL_FRAME = LayerMask.GetMask("Tutorial");
        TM_ERROR_WINDOW = 0         // тачь отключен (для ошибок и туториала)
    }*/
    const int defaultInputLayer = 1025;
    const int popUpWindowLayer = 1536;
    int _actualInputLayer = defaultInputLayer;//InputLayer.TM_DEFAULT;
    public int actualInputLayer {
        get {
            //updateGetActualInputLayer();
            return _actualInputLayer;
        }
    }

    public static int updateGetActualInputLayer() // █ обновление слоя маски тачей, по факту имеющихся активных окон
    {
        int res = defaultInputLayer; //(int)InputLayer.TM_DEFAULT; // окна отсутствует, игровая маска
        if (Errors.isShowing()) res = 0; // █ окна ошибок построены на новой UI системе, где есть своя обработка тачей, потому маска отключается //InputLayer.TM_POP_UP_WINDOW;
        else if (WaitingServerAnsver.isShowing()) res = 0; // игровой тачь отключен
        else if (Tutorial.isShowing()) { res = LayerMask.GetMask("Tutorial");
            //print("Tutorial!");
        } // показано обущающее сообщение, маска берётся из списка масок заданых в редакторе Unity
        else if (WindowController.getWinController.isWindowShow()) res = popUpWindowLayer; //(int)InputLayer.TM_ZERO; // отображается вспвлывающее окно
            getScenesController._actualInputLayer = res;
        //print("█ updateGetActualInputLayer == "+ res);
        return res;
    }

    // функция для загрузки фона под текущую сцену, так же расчёт позиций смещения если этот фон смещаемый
    bool loadCurrentBackGroundOnPlayShift() {   
        GameObject bgGO = GameObject.Find("BackGround");
        SpriteRenderer bgSR = null;
        if (bgGO == null) {
            Debug.Log("Error! [calculateBackGroundPos] BackGround == null");
            return false;
        }
        
        bgSR = bgGO.GetComponent<SpriteRenderer>();
        if (bgSR == null) {
            Debug.Log("Error![init] backGround Sprite not finded!");
            return false;
        }
        if (bgSR.sprite.name == "bag") {
            if (backGroundPositions.Count == 0)
                return calculateBackGroundPos(bgGO);
            return true;
        }
        return false;
    }
    // здесь и происходит расчёт позиций фона разбивая его на равные заданые куски, и расспределяя позици для семещния по Х,  с учётом ширины экрана
    bool calculateBackGroundPos(GameObject backGround) {
        currentBackGround = backGround;
        //GameObject.DontDestroyOnLoad(backGround);
        var backGroundSR = backGround.GetComponent<SpriteRenderer>();
        float width = backGroundSR.sprite.texture.width * 0.01f;
        float widthWithOutScreenSize = width - Screen.width * 0.01f;
        stepSize = widthWithOutScreenSize / countPosInBackGroun;
        float startPoint = -width * 0.5f + Screen.width * 0.005f;

        /*Debug.Log("textureWidth:" + (width * 0.01f));
        Debug.Log("screenWidt:" + (Screen.width * 0.01f));
        Debug.Log("------------------------------------------");
        Debug.Log(startPoint + stepSize * 2);
        Debug.Log(startPoint + stepSize);
        Debug.Log(startPoint);
        Debug.Log("------------------------------------------");*/
        backGroundPositions.Add(GameScene.MAIN_MENU, startPoint);
        backGroundPositions.Add(GameScene.BUY_TICKETS, startPoint + stepSize);
        backGroundPositions.Add(GameScene.RAFFLE, startPoint + stepSize * 2);
        currentBackGroundPos = startPoint;
        nextBackGroundPos = startPoint;        
        return true;
    }
    // Начало проигрования анимации смещения
    bool startPlayBackGroundMoveTo(GameScene toGameScene) {
        //if ( !backGroundPositions.ContainsKey(currentScene) ) return false;
        nextBackGroundPos = backGroundPositions[toGameScene];
        return true;
    }
    // для регулировки скорости смещения
    float shiftValueOnOneTick = 0.2f; 
    void FixedUpdate() { // вся логика тольки дла анимации смещения фона
        if (currentBackGroundPos == nextBackGroundPos) return;

        float dif = currentBackGroundPos - nextBackGroundPos;
        if (Mathf.Abs(dif) < shiftValueOnOneTick) currentBackGroundPos = nextBackGroundPos;
        else if (currentBackGroundPos > nextBackGroundPos) currentBackGroundPos -= shiftValueOnOneTick;
        else currentBackGroundPos += shiftValueOnOneTick;

        Vector3 cP = currentBackGround.transform.position;
        currentBackGround.transform.position = new Vector3(currentBackGroundPos, cP.y, cP.z);
    }
    // Калл бэк при старте модуля
    public static void OnStartGameScene( GameScene scene ) {    // калл бэк при загрузке модуля: ...
        //var _this = getScenesController;
        if (scene == GameScene.RAFFLE){
            //var accountEvent = _this.getNetwork().GetComponent<AccountEvent>();
            AccountEvent.requestAccountInformation();
        }
    }
    // Ниже внутрение фунции сокрытия модулей/сцен, и одна публичаня, для получения самого модуля
    static void hideModule(string name, bool destroy = false) {
        ScenesController sc = ScenesController.getScenesController;
        sc._hideModule(name, destroy);
    }
    public GameObject getModuleByName(string name) {
        switch (name) {
            //case GameScene.START_LOGO: { } break;
            //case GameScene.GAME_LOADING: { } break;
            case "HUD": return hud;
            case "Autorization": return autorization;
            case "MainMenu": return mainMenu;
            case "BuyTickets": return buyTickets;
            case "Raffle": return raffle;
        }
        return null;
    }
    void _hideModule(string name, bool destroy = false) {
        //print("[_hideModule] name:" + name+" destroy:"+ destroy);
        GameObject module = getModuleByName(name);
        if (module == null) {
            Debug.Log("Error! [_hideModule] module(" + name + ") == null");
            return;
        }
        _hideModule(module, destroy);
    }
    void _hideModule(GameObject module, bool destroy = false) {
        if (destroy) { Destroy(module); }
        else module.SetActive(false);
    }
    // █ основная интерфейсная функция, загрузки сцены / модуля, предведущие прячутся автоматически, удаляются ли они или прячутся, прописано здесь же  в одно из функции onLeaveCurrentScene
    public static void loadScene( GameScene nextGameScene ) {
        ScenesController.getScenesController._loadScene(nextGameScene);
    }
    public static GameObject setGameModule(GameObject prefab, bool stateValue = true){
        GameObject module = scenesController.getModuleByName(prefab.name);//GameObject.Find(name);
        if (module == null ) {
            if (stateValue == false) return null;
            module = Instantiate(prefab);
            module.name = prefab.name;
        } 
        module.SetActive(stateValue);
        return module;
    }
    // █ здесь прописаны инструкции выполняемые при покидании, сцен, модулей.
    void _loadScene( GameScene nextGameScene ) {
        if (nextGameScene == currentScene) return;
        switch (nextGameScene) {
            case GameScene.START_LOGO: { } break;
            case GameScene.GAME_LOADING: { } break;
            case GameScene.AUTORIZATION:{
                    if (autorization == null)
                        autorization = setGameModule(autorizationPrefab);
                    else{
                        autorization.SetActive(true);
                        if (hud != null) hud.SetActive(false);
                        Autorization.restartAuth();
                    }
                } break;
            case GameScene.MAIN_MENU: {
                    hud = setGameModule(hudPrefab);
                    mainMenu = setGameModule(mainMenuPrefab);
                } break;
            case GameScene.BUY_TICKETS: {
                    if (Rooms.get.serverTemplates == null /*|| !Rooms.get.isCurrentRoomIsInit*/ ) { // ███ TODO при добавлении новых комнат, нужно поменять эту проверку
                        TemplatesEvent.upateTemplates(1, 0); //
                        return;
                    } else {
                        buyTickets = setGameModule(buyTicketsPrefab);
                        HUD.setEnableBackButton(true);
                        Tutorial.show(TutorialSubject.TS_CHOOSE_COUNT_TICKETS, TutorialSubject.TS_BUY_TICKETS);
                    }
                } break;
            case GameScene.RAFFLE:{
                    setGameModule(buyTicketsPrefab, false);
                    if (raffle == null) raffle = setGameModule(rafflePrefab);
                    else {
                        raffle.SetActive(true);
                        main.raffle.reStart();
                    }
                    HUD.setEnableBackButton(false);
                    HUD.hideTop();
                    HUD.hideBuyRubins();
                    HUD.getHUD.calculateScales();
                } break;
        }
        bool res1 = (backGroundGroups[currentScene] == BackGroundGroup.GAME);
        bool res2 = (backGroundGroups[nextGameScene] == BackGroundGroup.GAME);
        if (res1 && res2)
            if (loadCurrentBackGroundOnPlayShift()) startPlayBackGroundMoveTo(nextGameScene);
        for (int i = 0; i < onLoadSceneCallBacks.Count; i++)
            if (onLoadSceneCallBacks[i] != null) onLoadSceneCallBacks[i](nextGameScene);
            else onLoadSceneCallBacks.Remove(onLoadSceneCallBacks[i]);
        onLeaveCurrentScene();
        currentScene = nextGameScene;
        currentGroup = backGroundGroups[currentScene];
    }

    // █ Необходимые операции при покидании модуля ( возможно стоило бы использовать эвентовую систему и ловить события в тех модулях где нужно выполнить соответственные действия для них, на месте... Но уж так.. собрано всё здесь
    static void onLeaveCurrentScene() {
        MAIN main = MAIN.getMain;
        main.message.Clear();
        var _this = ScenesController.getScenesController;
        switch (currentScene) {
            case GameScene.UNDEF: return;
            case GameScene.START_LOGO: return; // hide LOGO;
            case GameScene.AUTORIZATION: {
                    _this._hideModule(_this.autorization );
                    SoundsSystem.getSoundSystem.init();
                    if (SoundsSystem.musikOn)
                        SoundsSystem.play(Sound.S_MUSICK);
                    //main.message.Clear();
                }
                break;
            case GameScene.MAIN_MENU: _this._hideModule(_this.mainMenu); break;
            case GameScene.BUY_TICKETS: { _this._hideModule(_this.buyTickets); }
                break;
            case GameScene.RAFFLE: {
                    _this._hideModule(_this.raffle);
                    HUD.showTop();
                    HUD.hideBuyBallBtn();
                    HUD.showBuyRubins();
                } break;
        }
    }
    // пперед выходом из игры диалоговое окно
    public static void showWindowExit(){
        var wnd = Errors.show("Вы действительно хотите выйти из игры?", "Да", "Нет");
        wnd.setAction(0, ()=> { MAIN.exit(); });
    }
    // ( TODO ) При нажатии на интерфесную кнопку назад (сюда столило бы привязать и кнопку на дейвасйе "назад" )
    public static void onBackBtn() {
        var _this = ScenesController.getScenesController;
        switch(currentScene) {
            case GameScene.RAFFLE: {
                    _this._loadScene(GameScene.BUY_TICKETS);
                } break;
            case GameScene.BUY_TICKETS: _this._loadScene(GameScene.MAIN_MENU); break;
            case GameScene.MAIN_MENU: showWindowExit(); break;
        }
    }
}
