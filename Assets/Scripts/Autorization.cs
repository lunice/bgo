using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class Autorization : MonoBehaviour {
    MAIN main = MAIN.getMain;
    ScenesController scenesController = ScenesController.getScenesController;
    public event EventHandler<EventArgs> OnRegisterDone;
    Button playBtn;     // кнопка играть
    Button vkBtn;       // кнопка вк
    VKontakte VK;       // Вконтакте Api

    Text name;          // имя игрока
    //Text xp;          // опыт
    Text gold;          // золото
    Text countGames;    // количество игр
    Transform infoPanel;// визуальная информационная панель содержащая в себе выше перечисленные элементы

    void printAllSessionsID() // (test) вывод всех используемых ключей
    {
        string s1 = PlayerPrefs.GetString("ApplicationID", "");
        string s2 = PlayerPrefs.GetString(AuthType.VK.ToString(), "");
        string s3 = PlayerPrefs.GetString(AuthType.GUEST.ToString(), "");
        if (s1 != "") print("ApplicationID: " + s1);
        if (s3 != "") print("SessionID(GUEST): " + s3);
        if (s2 != "") print("SessionID(VK): " + s2);
    } 
    void clearAllId() // (test) вывод всех используемых ключей
    {
        printAllSessionsID();
        PlayerPrefs.SetString("ApplicationID", "");
        PlayerPrefs.SetString(AuthType.VK.ToString(), "");
        PlayerPrefs.SetString(AuthType.GUEST.ToString(), "");
        print("ApplicationID, SessionID(VK) and SessionID(GUEST) was deleted from disk!");
    }

    void Awake() // инициализация авторизации и прочих основныых частей приложения
    {
        //█ инициализируются все эвенты
        GameObject go = GameObject.Find("Network");
        if (go == null) {
            go = new GameObject("Network");
            var auhE = go.AddComponent<AuthEvent>();
            var re = go.AddComponent<RegisterEvent>();
            go.AddComponent<PlayEvent>();
            go.AddComponent<BallEvent>();
            VK = go.AddComponent<VKontakte>();
            go.AddComponent<HandlerServerData>();
            go.AddComponent<TemplatesEvent>();
            var ae = go.AddComponent<AccountEvent>();
            go.AddComponent<NetExchanger>();
            go.AddComponent<MarketEvent>();
            go.AddComponent<BuyEvent>();
            go.AddComponent<ExchangeEvent>();
            GameObject.DontDestroyOnLoad(go);
            re.subscribe(onRegisterDone);
            auhE.subscribe(onOnAuthenticationDone);
            var rooms = go.AddComponent<RoomsEvent>();
            rooms.subscribe(onRoomsReceive);
            ae.subscribe(onAccountReceive);
        }
        // инициализация и подпись на событиях кнопок авторизации
        OnRegisterDone += (sender, e) => onRegisterDone();
        playBtn = transform.FindChild("PlayBtn").GetComponent<Button>();
        ///playBtn.enabled = false;// setEnable(false);
        playBtn.interactable = false;
        vkBtn = transform.FindChild("vkBtn").GetComponent<Button>();

        ///playBtn.subscribeOnControllEvents(onButtonClick);
        ///vkBtn.subscribeOnControllEvents(onButtonClick);
        playBtn.onClick.AddListener(() => { onButtonClick(playBtn.name); });
        vkBtn.onClick.AddListener(() => { onButtonClick(vkBtn.name); });

        ///ScenesController.setCurrentScene(ScenesController.GameScene.AUTORIZATION);
        // Инициализация информационной панели
        infoPanel = transform.FindChild("InfoPanel");
        ///name = infoPanel.FindChild("namePlayerLabel").GetComponent<ObjectCaption>();
        ///xp = infoPanel.FindChild("xp").FindChild("value").GetComponent<ObjectCaption>();
        ///gold = infoPanel.FindChild("gold").FindChild("value").GetComponent<ObjectCaption>();
        name = infoPanel.FindChild("namePlayerLabel").GetComponent<Text>();
        ///xp = infoPanel.FindChild("xp").GetComponent<Text>();
        gold = infoPanel.FindChild("gold").GetComponent<Text>();
        countGames = infoPanel.FindChild("countGames").GetComponent<Text>();
        var deviceIdT = transform.FindChild("deviceIdLabel");
        if (MAIN.IS_TEST) { 
            var deviceId = deviceIdT.GetComponent<Text>();
            if (deviceId != null) {
                deviceId.text = "Device ID: \" "+ SystemInfo.deviceUniqueIdentifier+" \"";
            }
        } else deviceIdT.gameObject.SetActive(false);
        //      rt.position = new Vector2((Screen.width + 583.0f)*0.01f, infoPanel.position.y);
        caclulateInfoPanelPos();
        ///Errors.showError("Это окно должно быть с двумя кнопками", "btn1", "btn2");
    }

    void caclulateInfoPanelPos(){
        RectTransform rt = infoPanel.GetComponent<RectTransform>();
        infoPanel.position = new Vector2(HUD.halfDefaultScreen * HUD.getProportionScreen() - 4.0f, rt.position.y);
    }

    void resetParam() // сбрасывает информационную панель блокирует нажатие кнопки "играть"
    {
        if (gold != null) gold.text = "";
        if (name != null) name.text = "";
        if (countGames != null) countGames.text = "";
        if (playBtn!=null) playBtn.interactable = false;
        
    }
    public static void restartAuth() // █ перезапуск аунтификации 
    {
        Autorization auth = null;
        var go = GameObject.Find("Autorization");
        if (go != null){
            auth = go.GetComponent<Autorization>();
            if (auth != null){
                auth.resetParam();
                auth.Start();
                return;
            }
        }
        MAIN.exit();
    }

    void Start() // █ начало аунтификации, проверяем на наличии вфайле ApplicationID если нету, проводим регистрацию игрока, отправляем на сервер получаемммммм ID гостя
    {
        main.applicationID = PlayerPrefs.GetString("ApplicationID", "");
        if (main.applicationID == "") RegisterEvent.OnRegister();
        else onRegisterDone();
    }
    void onRegisterDone() // █ по завершению регистрции, приступаем к аунтификации
    { startAuthentication(); }
    void startAuthentication() // █ начало аунтификации, проверяем наличие в файле ключей вк, еси они есть, пытаемся залогиться под ВК инчае под гостя
    {
        // Начало авторизации: проверяю наявность sessionID VK
        main.sessionID = PlayerPrefs.GetString(AuthType.VK.ToString(), "");
#if UNITY_ANDROID && !UNITY_EDITOR
        print("main.sessionID" + main.sessionID);
        if (main.sessionID != "") {
            vkAuthentication();
            return;
        }
#endif
        authenticationAsGuest();
    }
    
    void vkAuthentication() // █ аунтификация ВК
    {
        ///print("try long auth in vk");
        main.tryRestoreSessionID = true; // █ переменной бозачатся, что попытка восстановления предвеущего ключа ВК ещё не осуществлялась.
        main.authType = AuthType.VK; // этой переменной обзначается тип аунтифкаии на текущий момент
        if (VKontakte.OnClickStatus()) {
            VK.OnClickProfile(); // если быстрая аунтификация успешна...
        } else {
            authenticationAsGuest(); // █ аунтифиация из под гостя
        }
    }
    public static void authenticationAsGuest() // █ аунтификация из под готся
    {
        MAIN main = MAIN.getMain;
        main.sessionID = PlayerPrefs.GetString(AuthType.GUEST.ToString(), ""); 
        main.authType = AuthType.GUEST;
        if (main.sessionID != "") { 
            main.tryRestoreSessionID = true;
            AuthEvent.OnSidAuth(main.sessionID);
        } else {
            main.tryRestoreSessionID = false; 
            AuthEvent.OnGuestAuth();
        }
    }
    public void logOutFromVk() // █ при повторном нажатии на кнопку ВК, происходит лоаути и открывется окно ввода логина пароля для новой аутификации ВК
    {
        PlayerPrefs.DeleteKey(AuthType.VK.ToString());
        VK.OnClickLogout();
        main.sessionID = "";
        main.authType = AuthType.GUEST;
    }

    void onOnAuthenticationDone(string sessionID) // █ При успешной аунтификации, следующий этап: отправляем запрос на получение данных игрока
    {
        print("[onOnAuthenticationDone] sessionID:" + sessionID);
        if ( main.authType == AuthType.VK ) {
            var vkInfo = VKontakte.getVkUserInfo(); // данные из вк
        }
        PlayerPrefs.SetString(main.authType.ToString(), sessionID);
        main.sessionID = sessionID;
        RoomsEvent.getRooms(0);
    }

    bool autoEnterInLobby = false;  // при удачной аунтификации, ожидается нажатие кнопки "играть", если выставить true - ожидния не будет
    void onAccountReceive(AccountData accountData) // при получении данны об игроке от сервера
    {
        updateInfoAccountWnd();                 // визуализация новых данных
        playBtn.interactable = true;
        if (autoEnterInLobby && main.authType == AuthType.VK)
            ScenesController.loadScene(GameScene.MAIN_MENU);
    }
    //void onMarketItemsRecive() {
        //AccountEvent.requestAccountInformation(); // запрашиваем данные об аккаунте
    //}
    void onRoomsReceive(RoomsData rooms) // █ получив комнаты
    {
        Rooms.setNewRoomsData(rooms);
        /////////////////////////////////////////////////////////
        //var controller = WindowController.getWinController;
        //MarketEvent marketEvent = controller.getMarketEvent();
        //marketEvent.OnReady += (sender, e) => controller.onMarketItemsRecive();
        //WindowController.requestContent(WindowController.TypePopUpWindow.NONE);
        /////////////////////////////////////////////////////////
        AccountEvent.requestAccountInformation(); // запрашиваем данные об аккаунте
    }

    void updateInfoAccountWnd() // визуализация новых данных игрока
    {
        switch (main.authType) {
            case AuthType.GUEST: { name.text = "ГОСТЬ"; } break;
            case AuthType.VK: { name.text = VKontakte.user.first_name+" "+ VKontakte.user.last_name; } break;
        }
        //xp.text = "XP: "+main.accountData.Xp.ToString();
        countGames.text = "сыграно партий: "+ main.accountData.GamesPlayed.ToString();
        gold.text = "золото: " + main.accountData.Gold.ToString();
    }

    //void onButtonClick(BaseController btn, BaseController.TypeEvent type) {
    void onButtonClick(string buttonName){
        //print("[onButtonClick]");
        //if (type != BaseController.TypeEvent.ON_MOUSE_CLICK) return;
        switch (buttonName) {
            case "PlayBtn" : {
                    ScenesController.loadScene(GameScene.MAIN_MENU);
                    //ScenesController.hideModule("Autorization",true);
                }  break;
            case "vkBtn": {
                    main.sessionID = "";
                    if ( main.authType == AuthType.VK) {
                        //main.setMessage("current state: login as vk, logout");
                        logOutFromVk();
                        //authenticationAsGuest();
                        ////////////////////////////
                    } else {
                        //main.setMessage("current state: not login as vk, try vk login");
                        print("VKontackte try auth!");
                    }
                    main.authType = AuthType.VK;
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                    autoEnterInLobby = true;
                    //this.gameObject.SetActive(false);
                    VK.OnClickLogin();
                    //this.gameObject.SetActive(true);
                    Screen.autorotateToPortrait = false;
                    Screen.autorotateToPortraitUpsideDown = false;
                } break;
        }
    }

    void Update () {
        //caclulateInfoPanelPos();
    }

    void OnGUI() // только для теста 
    {
        if (MAIN.IS_TEST && main.message.Count > 0) {
            float labelHeight = 20.0f;
            float s = Screen.width * 0.05f;
            float e = Screen.width * 0.9f;
            GUI.Box(new Rect(s, 0.0f, e, main.message.Count * labelHeight), "");
            for (int i = 0; i < main.message.Count; i++)
                GUI.Label(new Rect(s, Screen.height * 0.00f + i * labelHeight, e, labelHeight), "Last Message: " + main.message[i]);
        }
    }
}
