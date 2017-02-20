using UnityEngine;
using UnityEngine.Events;
//using System.Collections;
using System.Collections.Generic;

public class Errors {
    // Текстовки
    public const string connectErrorText = "Ошибка связи";
    public const string connectErrorText2 = "Сессия прервана.\nОтсутствует подключение к сети";

    public enum TypeError {
        E_NONE,
        E_TEST,                     // тестовая ошибка
        //E_TIME_OUT,             // ошибка соединения timeOut
        //E_NETWORK,
        ES_SERVER_ERROR,
        ES_NEED_UPDATE,        // тут две кнопки: обновить и выход
        ES_CONNECT_ERROR,      // █ ошибка соединения timeOut
        ES_SESSION_EXPIRED,     // Пытаемся переподключится, в противном случае выкидываем в авторизацию
        //MAIN_TIME         
        //SERVER_DOWN
        //█ ES_DRAWING_END,        // молча выйти в лобби
        //ES_NOT_ENOUGH,         // молча синхронизировать, запросить аккаунт данные, установить валидные данные, и проиграть анимацию нехватки денег/кристалов
                                 //---- Purchaser
        EP_ON_INITIALIZE_FAILED, // если вообще не отображается ничего, то кнопка нажимается но ничего не происходит, и выводить только то что проинициализировать то что можем
        EP_ON_BUY_NOT_FIND,      // один раз я пытаюсь обновить список покупаемых лотов(незаметно) // 10 сек. (только при ошибке)
        EP_ON_FAILED_CONFIRM_PURCHASE, // это капец, покупка, обратитесь в тех поддержку!

        EC_NOT_ENOUGH_MONEY,        // показываем сообщение о нехватке золота, и после показываем всплывающее окно обмена рубинов на золото
        EC_NOT_ENOUGH_RUBINS        // показываем сообщение о нехватке рубинов, и после показываем всплывающее окно покупки рубинов
    }
    // описания синглтона
    private static Errors errors;
    public static Errors getErrors {
        get {
            if (errors == null) errors = new Errors();
            return errors;
        }
    }

    //static ErrorWindow errorWindow = null;      // █ показываемое окно ошибки, при показе новой, предведущая теряется ( но её удаление происходит при нажатие на кнопки )
    public static WarningWindow warningWindow = null;
    List<ErrorWindow> errorWindows = new List<ErrorWindow>();
    static ErrorWindow errorWindow {
        get {
            List<ErrorWindow> errorWindows = getErrors.errorWindows;
            if (errorWindows == null || errorWindows.Count == 0)
                return null;
            return errorWindows[errorWindows.Count - 1];
        }
        set {
            List<ErrorWindow> errorWindows = getErrors.errorWindows;
            errorWindows.Add(value);
        }
    }
    // показывается ли окно
    public static bool isShowing() { return errorWindow != null && errorWindow.gameObject.activeSelf || warningWindow != null; }
    // Установить действие для кнопки, по порядку слева на право нумерация кнопки с нуля
    public static void setActionForButton(int indexButton, UnityEngine.Events.UnityAction action){
        errorWindow.setAction(indexButton, action);
    }
    public static void setActionForButton(string buttonCaption, UnityEngine.Events.UnityAction action){
        errorWindow.setAction(buttonCaption, action);
    }
    // (недоделано и в не нужно) более мелкий шрифт описывающий детали ошибки
    public static void addDetails(string text){
        errorWindow.addDetails(text);
    }
    // Ошибки показаны через эту функцию будут отображаться только при тестировании и помечаются в тексте ошибки смиволом "█"
    public static ErrorWindow showTest(string text){
        if (MAIN.IS_TEST)
            return Errors.showError("█ "+text, TypeError.E_NONE);
        return null;
    }
    // Прочие функици отображенияw
    public static ErrorWindow show(string text, UnityAction action) // ███ с ДОБАВЛЕНИЕМ дополнительных действий по закрытию окна, которые выполнятся ПОСЛЕ удаления окна 
    {
        var eWnd = show(text);
        eWnd.setAction(0, action);
        return eWnd;
    }
    public static ErrorWindow show(string text, params string[] btnText) {
        return Errors.showError(text, TypeError.E_NONE, btnText);
    }
    public static ErrorWindow showErrorAndReAutification(string text, GameScene fromScene = GameScene.UNDEF) // Показать ошибку и выйти в окно авторизации с повторной аунтификацией ( кнопка в ошибке всегда одна - "ок" )
    {
        var eWnd = show(text);
        eWnd.setAction(0, () => {
            if (fromScene == GameScene.AUTORIZATION) Autorization.restartAuth();
            else ScenesController.loadScene(GameScene.AUTORIZATION);
        });
        return eWnd;
    }
    public static WarningWindow showWarningWindow()
    {
        var warningWindowPrefab = RESOURCES.getPrefab("WarningBuyWindow");
        GameObject warningWindowCanvasGO = GameObject.Instantiate(warningWindowPrefab) as GameObject;
        warningWindowCanvasGO.name = "WarningWindow";
        SoundsSystem.play(Sound.S_ERROR);
        warningWindow = warningWindowCanvasGO.transform.FindChild("WarningWindow").GetComponent<WarningWindow>();
        ScenesController.updateGetActualInputLayer();
        return warningWindow;
    }

    public static ErrorWindow showError(string text, TypeError typeError_, params string[] btnText) {
        //Debug.Log("Error! \"" + text + "\", from game scene:" + typeError_.ToString() + Application.stackTraceLogType);
        //Application.RegisterLogCallback(HandleLog);
        //Application.logMessageReceived
        //var resourses = RESOURCES.getResources;
        var errorWindowPrefab = RESOURCES.getPrefab("ErrorWindow");
        GameObject errorWindowCanvasGO =  GameObject.Instantiate(errorWindowPrefab) as GameObject;
        errorWindowCanvasGO.name = "ErrorWindowCanvas";
        errorWindow = errorWindowCanvasGO.transform.FindChild("ErrorWindow").GetComponent<ErrorWindow>();
        errorWindow.init(text, typeError_, btnText);
        Utils.screenShot("OnError.png"); // только в режиме тестировки
        SoundsSystem.play(Sound.S_ERROR);
        return errorWindow;
    }
    // --------------[ ACTIONS ]----------------
    //static UnityAction goToAutorizations = () => { ScenesController.loadScene(GameScene.AUTORIZATION); Errors.errorWindow.hideWindow(); };
    //public static UnityAction exit = () => { MAIN.exit(); };
    // -----------------------------------------
    public static void showConnectError(string s, GameScene fromScene = GameScene.UNDEF)
    {
        string buttonText = "Повтор";
        if (fromScene == GameScene.MARKET) buttonText = "OK";
        var wnd = showError(s, TypeError.ES_CONNECT_ERROR, buttonText);
        wnd.setAction(0, () => {
            if (fromScene == GameScene.AUTORIZATION) Autorization.restartAuth();
            else if (fromScene == GameScene.MARKET) { }
            else ScenesController.loadScene(GameScene.AUTORIZATION); // и там уже будет рестарт
            //wnd.hideWindow();
        });
        //wnd.setAction(1, exit);
    }
    // -----------------------------------------

    public static void showError(TypeError typeError, GameScene fromScene = GameScene.UNDEF) {
        //currentTypeError = typeError;
        switch (typeError){
            case TypeError.ES_NEED_UPDATE:{
                    //Ваша версия устарела, нужно обновится до последней версии
                    var wnd = showError("Ваша версия не совместима, обновитесь до последней версии", typeError, "Выход");
                    //wnd.setAction(0, () => { /* ЗДЕСЬ НУЖНО ОБНОВИТЬСЯ */ wnd.hideWindow(); });
                    //wnd.setAction(1, exit);
                    wnd.setAction(0, () => { MAIN.exit(); } );
                } break;
            case TypeError.ES_SERVER_ERROR: { showErrorAndReAutification("Ошибка сервера", fromScene); }  break;
            case TypeError.ES_SESSION_EXPIRED: { showErrorAndReAutification("Время сессии истекло войдите заново",fromScene); }  break;
            case TypeError.ES_CONNECT_ERROR: { showConnectError("Ошибка связи", fromScene); } break;
            case TypeError.EP_ON_INITIALIZE_FAILED: buyError(); break;
            case TypeError.EP_ON_BUY_NOT_FIND: buyError(); break;
            case TypeError.EP_ON_FAILED_CONFIRM_PURCHASE: buyError("Ошибка покупки, обратитесь в техническую поддержку"); break;
            case TypeError.EC_NOT_ENOUGH_MONEY: {
                    var wnd = show("Недостаточно денег!");
                    wnd.setAction(0, () => {
                        if (!Tutorial.show(TutorialSubject.TS_BUY_GOLD_BTN)) { 
                            WindowController.showPopUpWindow(WindowController.TypePopUpWindow.GOLD_EXCHANGE);
                            Tutorial.show(); // В этом месте обрывается обучения, для избежании этого вызвается команда показать без параметров, в ней внутри перепроверяется наличие списка сообщений которые нужно показать, и если список не пустой обучение продолжается
                        }
                    });
                } break;
            case TypeError.EC_NOT_ENOUGH_RUBINS: {
                    //WindowController.showPopUpWindow(WindowController.TypePopUpWindow.CRYSTALS_BUY);
                    var wnd = show("Недостаточно рубинов!");
                    wnd.setAction(0, () =>{
                        WindowController.showPopUpWindow(WindowController.TypePopUpWindow.CRYSTALS_BUY);
                        Tutorial.show(); // В этом месте обрывается обучения, для избежании этого вызвается команда показать без параметров, в ней внутри перепроверяется наличие списка сообщений которые нужно показать, и если список не пустой обучение продолжается
                    });
                    } break;
            default: { show("Неизвестная ошибка"); } break;
        }
    }

    public void destroy(ErrorWindow wnd){ GameObject.Destroy(wnd.transform.parent.gameObject);}
    public static void buyError(string text = "Ошибка покупки") {
        var wnd = show(text);
        wnd.setAction(0, () => { WindowController.rebildCurrentWindow(); });
    }
    public static void showServerError(string textError = "Ошибка сервера", GameScene fromScene = GameScene.UNDEF) {
        if (fromScene == GameScene.AUTORIZATION) {
            var errorW = show(textError, "Повтор","Выход");
            errorW.setAction(0, () => { Autorization.restartAuth(); });
            errorW.setAction(1, () => { MAIN.exit(); });
            return;
        }
        var errorWnd = show("Ошибка сервера");
        errorWnd.setAction(0, () => {
            switch(fromScene){
                case GameScene.BUY_TICKETS: ScenesController.loadScene(GameScene.MAIN_MENU); break;
                case GameScene.RAFFLE: ScenesController.loadScene(GameScene.BUY_TICKETS); break;
            }
        });
    }
    public static void showError(Api.ServerErrors serverErrorType, GameScene fromScene = GameScene.UNDEF) {
        switch (serverErrorType) {
            case Api.ServerErrors.E_VERSION_ERROR: showError(TypeError.ES_NEED_UPDATE, fromScene); break;
            case Api.ServerErrors.E_SESSION_EXPIRED: showError(TypeError.ES_SESSION_EXPIRED, fromScene); break;
            case Api.ServerErrors.E_REQUEST_PARAMS: show("Ошибка покупки", () => { WindowController.rebildCurrentWindow(); }); break;
            case Api.ServerErrors.E_DRAWING_END: ScenesController.loadScene(GameScene.MAIN_MENU); break;
            case Api.ServerErrors.E_NOT_ENOUGH: {
                    var errWnd = show("Недостаточно средств для совершения операции.\nПопробуйте ещё.");
                    errWnd.setAction(0, () => { 
                        WindowController.rebildCurrentWindow();
                        AccountEvent.requestAccountInformation();
                        if (fromScene == GameScene.RAFFLE) ScenesController.loadScene(GameScene.MAIN_MENU);
                    });
                } break;
            case Api.ServerErrors.E_SESSION: { showErrorAndReAutification("Ошибка сессии"); } break;
            case Api.ServerErrors.E_SESSION_ID: { showErrorAndReAutification("Ошибка сессии"); } break;
            case Api.ServerErrors.E_DB_ERROR: showServerError("Ошибка сервера",fromScene); break;
            case Api.ServerErrors.E_TEMP_ERROR: showServerError("Временная ошибка сервера", fromScene); break;
            case Api.ServerErrors.E_PENDING: showServerError("ошибка сервера", fromScene); break;
            default: {
                    if (fromScene != GameScene.AUTORIZATION) { 
                        var errWnd = show("Неизвестная ошибка");
                        errWnd.setAction(0, () =>{ ScenesController.loadScene(GameScene.AUTORIZATION);});
                    } else {
                        var errWnd = show("Приложение не рабочее","выход");
                        errWnd.setAction(0, () => { MAIN.exit(); });
                    }
                }  break;
        }
    }

    public static void onErrorButtonClick(ErrorWindow window, string btnCaption = "") {
        /*switch (window.typeError) {
            case TypeError.ES_CONNECT_ERROR: { ScenesController.loadScene(GameScene.AUTORIZATION); }; break;
        }*/
        List<ErrorWindow> errorWindows = getErrors.errorWindows;
        if (errorWindows.Contains(window))
            errorWindows.Remove(window);
        window.hideWindow();
        
        //if (btnCaption == "OK" || btnCaption == "Ну ладно") 
        //GameObject.Destroy(window.transform.parent.gameObject);
    }
}
