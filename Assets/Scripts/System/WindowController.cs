using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
// Класс управления всплвающими окнами
// ███ данные для покупки рубинов и обмена золота, могут обноваться каждый час, смотреть ниже, но только при пересоздании окна, нужно что бы эта логика вызывалась каждый раз при вызове Show!
public class WindowController
{
    private static WindowController winController = null;
    private bool isInit;
    public static WindowController getWinController
    {
        get
        {
            if (winController == null) winController = new WindowController();
            if (!winController.isInit) winController.init();
            return winController;
        }
    }
    MAIN main = MAIN.getMain;
    public enum PopUpWindowEventType
    {
        PW_SHOW,
        PW_HIDE
    }
    /*
    public delegate void PopUpWindowEvent(PopUpWindow pWindow, PopUpWindowEventType typeEvent);
    protected List<PopUpWindowEvent> callBacks = new List<PopUpWindowEvent>();
    public void subscribeOnControllEvents(PopUpWindowEvent newCallBack) { callBacks.Add(newCallBack); }
    */
    // здесь обрабатываются разные события под разные виды окнон
    public static void onWindow(PopUpWindow pWindow, PopUpWindowEventType eventType)
    {
        var typeCW = getTypeCurrentWindow();
        if (/*ScenesController.currentScene == GameScene.RAFFLE && */
            (typeCW == TypePopUpWindow.GOLD_EXCHANGE || typeCW == TypePopUpWindow.CRYSTALS_BUY)){
            if (eventType == PopUpWindowEventType.PW_SHOW){
                HUD.hideBuyBallBtn();
                HUD.showBuyRubins();
                if (typeCW == TypePopUpWindow.GOLD_EXCHANGE) Tutorial.showFirst(TutorialSubject.TS_BUY_GOLD); // после отображения окна пытаемся показать обучающее сообщение обмена кристалов на золото
            } else if (ScenesController.currentScene == GameScene.RAFFLE && eventType == PopUpWindowEventType.PW_HIDE) {
                MAIN main = MAIN.getMain;
                if (main.handlerServerData.isAvailableNextBall && main.raffle.raffleState == RaffleState.FINISH)
                    HUD.showBuyBallBtn();
                HUD.hideBuyRubins();
            }
        }
    }

    bool init(){
        //var controller = WindowController.getWinController;
        //Debug.Log("[requestContent] type:" + type);
        marketEvent = getMarketEvent();
        marketEvent.OnReady += (sender, e) => OnMarketEventRespound();

        isInit = true;
        return true;
    }
    public RESOURCES getResources()
    {
        GameObject resGO = GameObject.Find("RESOURCES");
        return resGO.GetComponent<RESOURCES>();
    }

    MarketEvent marketEvent;
    public MarketEvent getMarketEvent()
    {
        if (marketEvent == null){
            marketEvent = GameObject.Find("Network").GetComponent<MarketEvent>();
            if (marketEvent == null)
            {
                Debug.Log("Error! [requestContent] marketEvent == null");
                return null;
            };
        }
        //Debug.Log("Error! [requestContent] ok");
        return marketEvent;
    }

    public enum TypePopUpWindow // виды всплвыюащих окон
    {
        NONE = 0,       // ничего (возвращается, на запрос что отборажается, если никаки окна не отображаются)
        CRYSTALS_BUY,   // покупка кристалов
        GOLD_EXCHANGE,  // обмен золота
        SETTINGS,       // настройки
        MARATHON        // окно марафона
    }

    static Dictionary<TypePopUpWindow, PopUpWindow> popUpWindows = new Dictionary<TypePopUpWindow, PopUpWindow>(); // █ сюда сохраняются созданые окна, каждый тип окна только в одном экземпляре
    static TypePopUpWindow showingWindow = TypePopUpWindow.NONE; // отображаемое окно
    public static bool showPopUpWindow(TypePopUpWindow type, bool hideIfShowing = false) // показать окно. Последний параметр, (не актуально) спрячет окно если оно уже показывалось
    {
        var controller = WindowController.getWinController;
        if (!hideIfShowing || controller.getShowingWindow() != type){
            return controller._showPopUpWindow(type);
        }
        else WindowController.hideCurrentWindow();
        return false;
    }
    public static TypePopUpWindow getTypeCurrentWindow() { return showingWindow; }
    public static bool isShowing()
    {
        return getWinController.isWindowShow();
    }
    public static void destroyBuyableWindows() // Удаляения покупных окно, нужно для их пересоздания, после повторного запроса на свежую информацию от сервера или гуглМаркета.
    {
        if (popUpWindows.ContainsKey(TypePopUpWindow.GOLD_EXCHANGE))
        {
            GameObject.Destroy(popUpWindows[TypePopUpWindow.GOLD_EXCHANGE]);
            popUpWindows.Remove(TypePopUpWindow.GOLD_EXCHANGE);
        }
        if (popUpWindows.ContainsKey(TypePopUpWindow.CRYSTALS_BUY))
        {
            GameObject.Destroy(popUpWindows[TypePopUpWindow.CRYSTALS_BUY]);
            popUpWindows.Remove(TypePopUpWindow.CRYSTALS_BUY);
        }
    }
    static float lastTimeRequest;       // время последнего запроса на данные магазинов
    static float waitingPeriod = 3600.0f; // время актуальности валидности данных полученых по сети.
    public static void rebildCurrentWindow() // пересоздание новых окон
    {
        if (waitingPeriod > Time.time - lastTimeRequest)
            return;
        var curWindow = WindowController.getTypeCurrentWindow();
        WindowController.destroyBuyableWindows();
        if (curWindow == WindowController.TypePopUpWindow.CRYSTALS_BUY || curWindow == WindowController.TypePopUpWindow.GOLD_EXCHANGE)
            WindowController.showPopUpWindow(curWindow);
    }

    public static void hideCurrentWindow(){
        if (showingWindow != TypePopUpWindow.NONE) {
            if (popUpWindows.ContainsKey(showingWindow)) {
                popUpWindows[showingWindow].hide();
                showingWindow = TypePopUpWindow.NONE;
            }
        }
        //MAIN.getMain.actualInputLayer = MAIN.defaultLayer;
        ScenesController.updateGetActualInputLayer();
    }
    Purchaser getPurchaser()
    {
        return main.purchase;
    }
    public bool _showPopUpWindow(TypePopUpWindow type) // █ показать окно, если его нет, создать, если данных нет, запросить
    {
        hideCurrentWindow();
        //Debug.Log("[_showPopUpWindow]");
        //main.actualInputLayer = MAIN.popUpWindowLayer;
        PopUpWindow wnd = null;
        if (popUpWindows.ContainsKey(type))
        {
            wnd = popUpWindows[type];
            showingWindow = type;
            wnd.show();
        } else {
            bool isPurchasingWind = type == TypePopUpWindow.CRYSTALS_BUY || type == TypePopUpWindow.GOLD_EXCHANGE;
            var me = getMarketEvent();
            if (((me.response == null || me.response.data.Exchange.Length == 0 || me.response.data.Purchase.Length == 0))
                && isPurchasingWind)
            {
                requestContent(type);
                return false;
            } else {
                if (!isPurchasingWind || crystalItems != null && goldFromCrystalExchange != null)
                {
                    showingWindow = type;
                    wnd = createAndShowWindow(type);
                } else { return false; }
            }
        }
        
        //Debug.Log("█3 showingWindow:" + showingWindow);
        return wnd != null;
    }

    public TypePopUpWindow getShowingWindow() // возвращает отображаемое окно
    {
        if (showingWindow != TypePopUpWindow.NONE && popUpWindows.ContainsKey(showingWindow) && popUpWindows[showingWindow].isActive())
            return showingWindow;
        return TypePopUpWindow.NONE;
    }
    public bool isWindowShow() { return showingWindow != TypePopUpWindow.NONE; }

    TypePopUpWindow waitToShowWindow = TypePopUpWindow.NONE; // █ окно которое ожидается для отображения (возможно ожидается ответ от сервера)
    public static void requestContent(TypePopUpWindow type) // █ запросить контент для окна (обычно приходит контент сразу для всех окон) контент сохраняется, потому окна при созданиях перед отправлением этого запроса туда заглядывают().
    {
        lastTimeRequest = Time.time;
        MarketEvent.requestMarketItems();
        //Debug.Log("█ запрос на получение данных, и ожидается открытие окна:" + type);
        getWinController.waitToShowWindow = type;
    }
    void OnMarketEventRespound() // получения ответа от сервера
    {
        //Debug.Log("█ данные получил");
        MarketEvent marketEvent = GameObject.Find("Network").GetComponent<MarketEvent>();
        //marketEvent.response;
        //Debug.Log("[OnReady] in WindowController" + waitToShowWindow);
        var name = getStringByType(waitToShowWindow);
        crystalItems = main.purchase.getMarketItesByName(name);
        goldFromCrystalExchange = getMarketEvent().response.data.Exchange;
        sortRubinItems();
        sortGoldItems();

        if (waitToShowWindow != TypePopUpWindow.NONE)
        {
            showingWindow = waitToShowWindow;
            Debug.Log("█ showingWindow == " + showingWindow);
            createAndShowWindow(showingWindow);
            //waitToShowWindow = TypePopUpWindow.NONE;
        }
    }

    string getStringByType(TypePopUpWindow type)
    {
        switch (type)
        {
            case TypePopUpWindow.CRYSTALS_BUY: return "crystal";
        }
        //Debug.Log("Error![getStringByType] undifine type:" + type);
        return "";
    }

    PopUpWindow createAndShowWindow(TypePopUpWindow type) // █ создать и после показать окно
    {
        waitToShowWindow = TypePopUpWindow.NONE;
        //Debug.Log("[createAndShowWindow] type:" + type);
        GameObject wndGO = null;
        if (type == TypePopUpWindow.SETTINGS)
        {
            var obj = Resources.Load("prefabs/Settings");
            wndGO = GameObject.Instantiate(obj) as GameObject;
        }
        else wndGO = GameObject.Instantiate(getResources().popUpWndPrefab);


        wndGO.name = type.ToString();
        //Debug.Log(wndGO.name);
        //System.Type typeWnd = < PopUpWindow >;
        PopUpWindow wnd = wndGO.GetComponent<PopUpWindow>();
        float speed = wnd.speedMove;
        var hPos = wnd.hidePosition;
        var sPos = wnd.showPosition;
        if (type != TypePopUpWindow.SETTINGS)
            GameObject.DestroyImmediate(wnd);
        switch (type)
        {
            case TypePopUpWindow.GOLD_EXCHANGE: wnd = wndGO.AddComponent<BuyGoldWnd>(); break;
            case TypePopUpWindow.CRYSTALS_BUY: wnd = wndGO.AddComponent<BuyCrystalsWnd>(); break;
            case TypePopUpWindow.SETTINGS: { } break;
            default: wnd = wndGO.AddComponent<PopUpWindow>(); break;
        }
        wnd.speedMove = speed;
        wnd.hidePosition = hPos;
        wnd.showPosition = sPos;
        if (type != TypePopUpWindow.SETTINGS)
            wnd.content = wnd.createContent();

        popUpWindows[type] = wnd;
        float txtH = wndGO.GetComponent<SpriteRenderer>().sprite.texture.height * 0.5f;
        wnd.hidePosition = new Vector2(0.0f, -(txtH + Screen.height) * 0.01f);
        wnd.transform.position = new Vector3(0.0f, wnd.hidePosition.y, 0.0f);
        wnd.show();
        return wnd;
    }

    PurchaseableItem[] crystalItems;    // айтемы покупки кристалов
    MarketExchange[] goldFromCrystalExchange; // айтемы обмена золота
    public PurchaseableItem[] getCrystalItems() { return crystalItems; } // получить айтемы покупки кристалов
    public MarketExchange[] getMarketExchange() { return goldFromCrystalExchange; } // получить айтемы обмена золота
    public void sortGoldItems() // █ сортировка айтемов в контенте обмена золота. Возможно стоило бы вынести эту функцию в класс: "контент окна для ОБмена золота"
    {
        for (int i = 0; i < goldFromCrystalExchange.Length; i++)
        {
            int min = i;
            for (int j = i + 1; j < goldFromCrystalExchange.Length; j++)
                if (goldFromCrystalExchange[min].From.Count > goldFromCrystalExchange[j].From.Count) min = j;
            if (i != min)
            {
                var temp = goldFromCrystalExchange[i];
                goldFromCrystalExchange[i] = goldFromCrystalExchange[min];
                goldFromCrystalExchange[min] = temp;
            }
        }
    }
    public void sortRubinItems() // █ сортировка айтемо в конткте возможно стоило бы вынести функцию в класс конткет рубинов...
    {
        for (int i = 0; i < crystalItems.Length; i++)
        {
            int min = i;
            for (int j = i + 1; j < crystalItems.Length; j++)
            {
                int len1 = crystalItems[min].Name.Length;
                int len2 = crystalItems[j].Name.Length;
                string str1 = crystalItems[min].Name.Substring(8, len1 - 8);
                string str2 = crystalItems[j].Name.Substring(8, len2 - 8);
                if (int.Parse(str1) > int.Parse(str2)) min = j;
            }
            if (i != min)
            {
                var temp = crystalItems[i];
                crystalItems[i] = crystalItems[min];
                crystalItems[min] = temp;
            }
        }
    }

    //==================================[ test ]==================================
    void testCall()
    {
        //Debug.Log("[testCall]");
        if (main.purchase != null)
            return;
        testInit();
        var me = GameObject.Find("Network").GetComponent<MarketEvent>();
        me.testRespound(main.marketPurchaser);
    }

    void testInit()
    {
        if (main.marketPurchaser != null) return;
        int count = 3;
        main.marketPurchaser = new MarketPurchase[count];
        for (int i = 0; i < count; i++)
        {
            main.marketPurchaser[i] = new MarketPurchase();
            main.marketPurchaser[i].Id = (short)(i + 1);
            main.marketPurchaser[i].Name = "crystal_" + i * 3 + 1;
            main.marketPurchaser[i].Free = i + 1;
        }
    }
}
