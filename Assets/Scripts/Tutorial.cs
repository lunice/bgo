using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
/////////////////////////////////////////////////////////////////////////////////////////////
// █ ВНИМАНИЕ! это перенагруженный скрипт тремя классами которые описывают всю систему обучения:
// - public static class Tutorial                       //  основной управляющий синглтон
// - public class TutorialFrame : MonoBehaviour         //  для визуализация окна
// - public class WritingText : MonoBehaviour           //  для визуализация текста в окне
/////////////////////////////////////////////////////////////////////////////////////////////
// █ Перечень всех обучающих тем, с установленными значениями, для побитового сохранения в файл что было показано а что нет...
public enum TutorialSubject {
    TS_UNDEF = 0,                   // 
    TS_ENTER_IN_ROOM = 1,           // в лобби, как зайти в комнату
    TS_CHOOSE_COUNT_TICKETS = 2,    // выбрать количетсво билетов в комнате
    TS_BUY_TICKETS = 4,             // купить выбраные билеты
    TS_TEMPLEATES = 8,              // объяснение шаблонов в розыгрыше, которые отображаются в верхней части экрана
    TS_PREVIN = 16,                 // объяснение превинов
    TS_BUY_BALL = 32,               // докупка шара
    TS_BUY_GOLD_BTN = 64,           // вызывать всплывающее окно обмена кристалов на золото
    TS_BUY_GOLD = 128,              // нажать кнопку обмена кристало на золото
    TS_EXIT = 256,                  // выход из розыгрыша
    TS_TUTORIAL_REPEAT = 512        // (не подключено) объяснение как повторно запустить туториал
}
// клас синглтон, через который организован весь интерфейс работы с обучением 
// в котором ведётся учёт пройденых сообщений
// и вся управляющая логика над другими связаными классами
public static class Tutorial {                          
    public static float timeShowTutorialFrame = 1.25f;              // время появления обучающего сообщения
    public static float timeHideTutorialFrame = 0.75f;              // время исчезновения обучающего сообщения
    public static Color backGroundColor;                            // подложка обучающего сообщения, вынессена как статик для повышенного контроля, но возможно это было излишне...
    public static int prevActualMask;                               // █ сюда сохраняется слои реагирующие на касания, для востановления после окончания показа обучающего сообщения
    public static float prevTimeScale;                              // особой необходимости нет, но для формальности при некоторых обучающих сообщениях приостанавливается время, и в эту переменную сохраняется предведущее его состояние, которое всегда равно 1.
    public static TutorialFrame tutorialFrame = null;               // █ собственно само обучающее сообщение, доступ к которому через ниже описаную переменную

    // оставил закоментированыый код, на случай если понадобится какому-то классу следить за состоянием обучающей системы.
    // █ возможно для исправлений некоторых багов придётся применить... Но вызовы нигде не прописаны! Это просто заготовка
    //public delegate void TutorialEvent( TutorialSubject subject );
    //protected static List<TutorialEvent> callBacks = new List<TutorialEvent>();
    //public static void subsribeOnTutorialHide(TutorialEvent callBack) { }
    //public static void unSubsribeOnTutorialHide(TutorialEvent callBack ) { }

    public static TutorialFrame frame { 
            get {
                if (tutorialFrame == null ) {
                    Object tutorialPrefab = RESOURCES.getPrefab("TutorialFrame");
                    var tutorialFrameGO = GameObject.Instantiate(tutorialPrefab) as GameObject;
                    tutorialFrame = tutorialFrameGO.AddComponent<TutorialFrame>();
                    tutorialFrameGO.SetActive(false);
                    backGroundColor = tutorialFrameGO.GetComponent<Image>().color;
                }
                return tutorialFrame;
            }
        }
    ///////////////////////////////////////////////
    // интерфейсные функции:
    
        // сбрасывает все пройденые сообщения и начинает туториал заново
    public static void restart() {         
        showedTutorialSubjectsMask = 0;
        PlayerPrefs.SetInt("showedTutorialSubjects", 0);
        show(TutorialSubject.TS_ENTER_IN_ROOM);
    }
    // спрятать текущее сообщение
    public static void hide() { if (frame != null ) frame.hide(); } 
    // через эту функцию можно узнать показывалось ли уже заданое сообщение
    public static bool wasShowed(TutorialSubject subject) {
        if (showedTutorialSubjectsMask == -1) showedTutorialSubjectsMask = PlayerPrefs.GetInt("showedTutorialSubjects", 0);
        return (showedTutorialSubjectsMask & (int)subject) != 0;
    } 
    // показано или показывается ли обучающее сообщение
    public static bool isShowing() { return frame!=null && frame.isShowing(); } 
    // если есть сообщения которые нужно показать сразу же за предведущими, то они помещаются в эту очередь сообщений
    static List<TutorialSubject> queueTutorialsSubject = new List<TutorialSubject>(); 
    // показать сообщение или очередь сообщений или добавить в очередь сообщений
    public static bool show(params TutorialSubject[] subjects) {
        bool res = false;
        for (int i = 0; i < subjects.Length; i++) {
            if (!queueTutorialsSubject.Contains(subjects[i]) && !wasShowed(subjects[i])){
                queueTutorialsSubject.Add(subjects[i]);
                res = true;
            }
                
        }
        if (!isShowing() && res) show(queueTutorialsSubject);
        return res;
    }
    // показать сообщение или эту очередь сообщений не смотря на существующие
    public static bool showFirst(params TutorialSubject[] subjects) {
        bool res = false;
        for (int i = 0; i < subjects.Length; i++) {
            if (!queueTutorialsSubject.Contains(subjects[i]) && !wasShowed(subjects[i])) { 
                queueTutorialsSubject.Insert(0+i, subjects[i]);
                res = true;
            }
        }
        if (!isShowing() && res) show(queueTutorialsSubject);
        return res; 
    }
    // показать сообщение(или очередь сообщений) перед указаным сообщением
    public static bool showBefore(TutorialSubject before, params TutorialSubject[] subjects) {
        int pos = 0;
        bool res = false;
        if (queueTutorialsSubject.Contains(before)) pos = queueTutorialsSubject.IndexOf(before);
        else return show(subjects);
        for (int i = 0; i < subjects.Length; i++) {
            if (!queueTutorialsSubject.Contains(subjects[i]) && !wasShowed(subjects[i])) { 
                queueTutorialsSubject.Insert(pos + i, subjects[i]);
                res = true;
            }
        }
        if (!isShowing() && res) show(queueTutorialsSubject);
        return res;
    }

    ///////////////////////////////////////////////
    // внутрение функции
    static bool show(List<TutorialSubject> listSubjects) { // показать сообщение или очередь сообщений (внутреняя функция!)
        if (!MAIN.isTutorialEnable) {
            queueTutorialsSubject.Clear();
            return false;
        }

        if (queueTutorialsSubject.Count == 0) return false;
        Debug.Log("█ список для отображения обучения обновлён: ========================");
        for (int i = 0; i < listSubjects.Count; i++)
            Debug.Log("  обучающая тема в списке №" + i+ ": " + listSubjects[i]);
        TutorialSubject subject = queueTutorialsSubject[0];
        if (Errors.isShowing() || WaitingServerAnsver.isShowing()) {
            return false;
        }
        if ( isShowing() ) return false;

        WritingText wt = null;
        switch (subject) {
            case TutorialSubject.TS_ENTER_IN_ROOM: {
                    var targetT = GameObject.Find("playBtn").transform;
                    frame.setTarget(targetT.gameObject, true);
                    wt = WritingText.create("Нажмите для начала игры.",
                        new Vector2(targetT.position.x, targetT.position.y - 3.5f),
                        new Vector2(530.0f, 200.0f), 0.05f);
                } break;
            case TutorialSubject.TS_CHOOSE_COUNT_TICKETS: {
                    GameObject ticketsButtonsGO = GameObject.Find("CountTicketsButtons");
                    if (ticketsButtonsGO == null){
                        Errors.showTest("<< TestError >> Не найдены кнопки radioButton(а)");
                        return false;
                    }
                    //var targetT = ticketsButtonsGO.transform.GetChild(3);
                    frame.setTarget(ticketsButtonsGO, true);
                    wt = WritingText.create("Выбирете количество билетов в розыгрыше.\nНажмите для выбора.",
                        new Vector2(ticketsButtonsGO.transform.position.x, ticketsButtonsGO.transform.position.y - 2.7f),
                        new Vector2(900.0f, 300.0f));
                } break;
            case TutorialSubject.TS_BUY_TICKETS: {
                    var targetT = GameObject.Find("buyTicketsBtn").transform;
                    frame.setTarget(targetT.gameObject, true);
                    wt = WritingText.create("Нажмите для покупки выбранных билетов.",
                        new Vector2(targetT.position.x, targetT.position.y + 0.6f),
                        new Vector2(800.0f, 200.0f),0.035f);
                    frame.hideImmeaiately = true;
                } break;
            case TutorialSubject.TS_TEMPLEATES: {
                    if (WindowController.isShowing()) WindowController.hideCurrentWindow();
                    var targetT = GameObject.Find("TemplatesHolder").transform;
                    frame.setTarget(targetT.gameObject);
                    prevTimeScale = Time.timeScale;
                    MAIN.setGameTimeSpeed(0.05f, 1.0f);
                    wt = WritingText.create("Цветными точками показаны возможные комбинации выигрышей.\nНиже - ожидаемый выигрыш прямо сейчас.",
                        new Vector2(targetT.position.x, targetT.position.y - 4.0f),
                        new Vector2(1300.0f, 500.0f), 0.0035f);
                } break;
            case TutorialSubject.TS_PREVIN: {
                    if (WindowController.isShowing()) WindowController.hideCurrentWindow();
                    var aureols = GameObject.FindGameObjectsWithTag("aureols");
                    var withOut = GameObject.FindGameObjectsWithTag("missingBall");
                    if (aureols.Length == 0) {
                        Errors.showTest("Ошибка обучения, не найдены предвыиграшные шаблоны");
                        return false;
                    }
                    GameObject[] targets = new GameObject[aureols.Length + withOut.Length];
                    for(int i=0; i<aureols.Length; i++)
                        targets[i] = aureols[i].gameObject.transform.parent.gameObject;
                    for(int i=0; i < withOut.Length; i++) { 
                        targets[i+ aureols.Length] = withOut[i].gameObject;
                    }
                    //for (int i = 0; i < targets.Length; i++)
                    frame.setTargets(false, targets);
                    wt = WritingText.create("Обратите внимание на мигающий шар - именно его не хватает до заполнения линии и получения выигрыша!",
                        new Vector2(0.0f, -4.0f),
                        new Vector2(1050.0f, 300.0f), 0.025f);
                    prevTimeScale = Time.timeScale;
                    MAIN.setGameTimeSpeed(0.05f, 1.0f);
                } break;
            case TutorialSubject.TS_BUY_BALL: {
                    //if (!HUD.getBuyBallButton().isActiveAndEnabled ) return false;
                    if (HUD.isPlaingAnim()){
                        Debug.Log("█ HUD.isPlaingAnim()");
                        queueTutorialsSubject.Remove(TutorialSubject.TS_BUY_BALL);
                        return false;
                    }
                    if (WindowController.isShowing()) { WindowController.hideCurrentWindow();}
                    var targetT = GameObject.Find("buyBallBtn").transform;
                    if (targetT == null ){
                        queueTutorialsSubject.Remove(TutorialSubject.TS_BUY_BALL);
                        return false;
                    } 

                    frame.setTarget(targetT.gameObject,true);
                    wt = WritingText.create("Нажмите,чтобы испытать удачу\nи найти недостающие шары!)",
                        new Vector2(targetT.position.x - 1.0f, Screen.height * 0.001f - 3.4f),
                        new Vector2(650.0f, 200.0f), 0.025f);
                } break;
            case TutorialSubject.TS_BUY_GOLD_BTN: {
                    if (WindowController.isShowing()) WindowController.hideCurrentWindow();
                    var targetT = GameObject.Find("moneyBtn").transform;
                    frame.setTarget(targetT.gameObject, true);
                    
                    //frame.setScreenTouch( true );
                    wt = WritingText.create("Чтобы раздобыть еще золота - загляните в магазин",
                        new Vector2(targetT.position.x + 2.0f, targetT.position.y + 0.5f),
                        new Vector2(1200.0f, 300.0f), 0.05f);
                    frame.dontShowTip = true;
                    //Debug.Log("█ [in create] tipWasShow == " + frame.tipWasShow);
                } break;
            case TutorialSubject.TS_BUY_GOLD: {
                    if (!wasShowed(TutorialSubject.TS_BUY_GOLD_BTN)) {
                        queueTutorialsSubject.Remove( TutorialSubject.TS_BUY_GOLD_BTN);
                        queueTutorialsSubject.Remove(TutorialSubject.TS_BUY_GOLD);
                        return false;
                    }
                    var goldBtns = GameObject.FindGameObjectsWithTag("exchangeGoldBtns");
                    if (goldBtns.Length == 0) return false;
                    var targetT = goldBtns[0].transform; ;
                    frame.setTarget(targetT.gameObject,true);
                    wt = WritingText.create("Для покупки золота нужны\nкристаллы. Попробуйте",
                        new Vector2(targetT.position.x, Screen.height * 0.001f + 2.5f),
                        new Vector2(550.0f, 300.0f), 0.05f);
                } break;
            case TutorialSubject.TS_EXIT: {
                    if (ScenesController.currentScene != GameScene.RAFFLE) {
                        queueTutorialsSubject.Remove(TutorialSubject.TS_EXIT);
                        Debug.Log("█ TS_EXIT когда не в розыгрыше");
                        for(int i=0;i<queueTutorialsSubject.Count;i++) {
                            Debug.Log("в очереди:["+i+"]" + queueTutorialsSubject[i]);
                        }
                        return show(queueTutorialsSubject);
                    }
                    var targetT = GameObject.Find("backBtn").transform;
                    frame.setTarget(targetT.gameObject, true);
                    wt = WritingText.create("Нажмите,чтобы начать\nновый розыгрыш. Удачи вам!",
                        new Vector2(targetT.position.x-3.0f, targetT.position.y - 2.8f),
                        new Vector2(550.0f, 400.0f), 0.04f);
                } break;
            case TutorialSubject.TS_TUTORIAL_REPEAT: {
                    var targetT = GameObject.Find("settingsBtn").transform;
                    frame.setTarget(targetT.gameObject, true);
                    frame.setScreenTouch( true );
                    wt = WritingText.create("Обучение завершенно! Повторный курс и\nпрочие возможности можно найти здесь.\nДля продолжения коснитесь екрана...",
                        new Vector2(targetT.position.x + 4.0f, targetT.position.y - 3.0f),
                        new Vector2(800.0f, 400.0f), 0.04f);
                } break;
        }
        if (wt != null) wt.transform.SetParent(frame.transform);
        //prevActualMask = MAIN.getMain.actualInputLayer;
        //MAIN.getMain.actualInputLayer = LayerMask.GetMask("Tutorial");
        frame.mySubject = subject;
        frame.show(subject);
        ScenesController.updateGetActualInputLayer();
        //------------[ save ]--------------
        showedTutorialSubjectsMask |= (int)subject;
        PlayerPrefs.SetInt("showedTutorialSubjects", showedTutorialSubjectsMask);
        return true;
    }
    // █ эта функция вызывается описаным класом ниже, по закрытию фрейма, для того что бы показать всю очередь сообщений
    // по правильному при создании обучающих окон, этот класс должен подписаться на них,
    public static void onTutorialFrameHide(TutorialSubject subject) {
        //bool isContinueTutorialShowing = true;
        switch (subject) {
            //case TutorialSubject.TS_SHOW_COUNT_TICKETS: show(TutorialSubject.TS_CHOOSE_COUNT_TICKETS); break;
            //case TutorialSubject.TS_CHOOSE_COUNT_TICKETS: show(TutorialSubject.TS_BUY_TICKETS); break;
            case TutorialSubject.TS_TEMPLEATES: MAIN.restoreGameTime(); /* MAIN.setGameTimeSpeed(1.0f, 0.5f); */ break;
            case TutorialSubject.TS_PREVIN: MAIN.restoreGameTime();  /*MAIN.setGameTimeSpeed(1.0f, 0.5f);*/ break;
            //case TutorialSubject.TS_EXIT: show(TutorialSubject.TS_TUTORIAL_REPEAT); break;
            //case TutorialSubject.TS_BUY_GOLD_BTN: show(TutorialSubject.TS_BUY_GOLD); break;
            //case TutorialSubject.TS_BUY_BALL: isContinueTutorialShowing = !WindowController.isShowing(); break;
            case TutorialSubject.TS_BUY_GOLD: {
                    WindowController.hideCurrentWindow(); // если нажато на любую часть экрана, окно нужно принудительно спрятать
                    //show(TutorialSubject.TS_EXIT);
                } break;
        }
        //Debug.Log("█ Remove cur subject:" + subject);
        queueTutorialsSubject.Remove(subject);
        //for(int i=0; i<queueTutorialsSubject.Count; i++) Debug.Log("subject["+i+"]:" + queueTutorialsSubject[i]);
        if (queueTutorialsSubject.Count != 0) show(queueTutorialsSubject);
    }
    static int showedTutorialSubjectsMask = -1; // текущая маска для изменения реагирующих слоёв на касание. -1 означет - отключена
}

/////////////////////////////////////////////////////////
// этот класс отвечает за логику визуального отображения обучающих сообщений
public class TutorialFrame : MonoBehaviour {
    enum State {
        TF_HIDE,        // спрятано
        TF_SHOWING,     // в процессе отображения
        TF_HIDING,      // в процессе сокрытия
        TF_SHOW,        // отображено
    }
    //List<GameObject> tutorialTargets = new List<GameObject>();
    //GameObject[] tutorialTargets;
    GameObject[] tTargets;  // сюда запихиваются объект(ы) которые нужно подсветить в текущем сообщении
    Image backGroundImage;  // чёрная подкладка (для удобного доступа при изменении прозрачности)
    float startTimeTutorialFrame;   // переменная для плавого отображения И СОКРЫТИЯ сообщения. Она помнит когда была отдана команда
    float unLockTime = 0.0f;        // время для разблокировки туториала, для его пропуска
    float currentAlpha;             // текущая альфа, всех объектов обучения, включая надписи и подложки!
    public bool dontShowTip = false;// не показывать подсказку: "нажмите на экран" в текущем сообщении
    int myOrder;  // █ необходим для корректной смены порядка прорисовки над другими объектами. Ордеры каждого объекта добавляются на указаное число, а при закрытии обучающего сообщения, обратно отнимает указаное число
    public bool tipWasShow; // была ли уже показана подсказка
    State state = State.TF_HIDE;    // текущее состояние
    public TutorialSubject mySubject = TutorialSubject.TS_UNDEF; // текущая тема обучения
    

    void Awake() {
        backGroundImage = GetComponent<Image>();
        var canvas = GetComponent<Canvas>();
        myOrder = canvas.sortingOrder;
    }
    void Start() { show(); }
    //public void setTarget(params GameObject[] target) {
    private int nativeTargetLayer;  // █ сюда помещается родной слой в котором был(и) объект(ы). А текущим назначается другой - туториальный, на который и будет реагировать касание игрока, после завершения обучения, из этой переменной востанаавливается родной слой.
                                    // █ ВАЖНО! если цель не одна и они будут из разных слоёв, то в эту переменную будет записан слой первого объекта, и при востановлении родных слоёв, назначен всем, что потенциально может нарушить работу в дальнешем!
                                    // █ В таком случае будет показана ошибка (в том числе в релизе!)

    // сюда указывается(ются) объект(ы) которые будут показаны при обучении
    // в первом параметре указывает, на обязательное нажатие только по целям, или же сообщение закроется просто по каснию экрана
    public void setTargets(bool clickEnable, params GameObject[] targets) { 
        if (targets.Length == 0) {
            Errors.showTest("Не передано никаких целей для туториала");
            return;
        }
        nativeTargetLayer = targets[0].layer; // запоминание родного слоя перед показом

        for (int i=0; i <targets.Length;i++) {
            if (targets[i].layer != nativeTargetLayer) {
                Errors.showTest("Предупреждение!, не все объекты из одного слоя:\n targets[i].layer != nativeTargetLayer");
            }

            //target.layer = LayerMask.NameToLayer("Tutorial");
            if (clickEnable) subscribeOnBaseControlls(targets[i]);
            else screenTouch = true;
            itWasClickEnable = clickEnable;
            bool visualRes = uperSpritesSortinOrder(targets[i]);
            if ( !visualRes ) Errors.showTest("Ошибка обучения! Как не печально, но в заданом объекте не найдены спрайты. При попытке показать в теме:"+mySubject);
        }

        tTargets = targets;
    }
    // устарелая но оставленая функция, без которой можно обойтись используя функцию выше,
    public void setTarget(GameObject target, bool clickEnable = false) { 
        setTargets(clickEnable, target);
    }
    bool itWasClickEnable; // сюда помещается значения того нужно ли нажимать только на указанные цели или сообщение спрячется когда нажмут на любую точку экрана
    bool screenTouch = false; // переменная в которой и указывается реагировать ли на весь экран
    public void setScreenTouch(bool newValue){
        screenTouch = newValue;
        //if (screenTouch) showTip();
    }
    // установка параметров цели обратно по умолчанию до показа сообщения
    public void unSetTarget() { 
        for(int i=0;i<tTargets.Length;i++) { 
            //print("targetName:" + tTargets[i].name);
            if (itWasClickEnable) subscribeOnBaseControlls(tTargets[i], true);
            uperSpritesSortinOrder(tTargets[i], true);
        }
    }
    // пробегает по всем детям указанной цели, и устанавливает им слой и ордер для туториала, или же наоборот возвращает, в зависимости от последнего параметра.
    bool uperSpritesSortinOrder(GameObject target, bool returnEverythingBack = false) {
        var dl = target.GetComponent<DigitsLabel>();
        if (dl != null) dl.orderLayer = (returnEverythingBack) ? dl.orderLayer - (myOrder + 100) : dl.orderLayer + (myOrder + 100);
        var sr = target.GetComponent<SpriteRenderer>();
        bool res = sr != null;
        if (res) sr.sortingOrder = (returnEverythingBack) ? sr.sortingOrder - (myOrder + 100) : sr.sortingOrder + (myOrder + 100);
        for (int i = 0; i < target.transform.childCount; i++) {
            res = res | uperSpritesSortinOrder(target.transform.GetChild(i).gameObject, returnEverythingBack);
        }
        return res;
    }
    // █ ВАЖНО! 
    // Если цель является кнопкой или же среди детей есть объекты что являются кнопкой, то реакция на нажатие будет происходит только по ним!
    // А так же подписывается на их нажаетие что бы закрыть окно, и обратно отписывается для этого и указывается последний параметр.
    // Этот функционал значительно облегчает добавление таких целей как группу кнопок!
    bool subscribeOnBaseControlls(GameObject target, bool unSubscibe = false) {
        var bc = target.GetComponent<BaseController>();
        bool res = bc != null;
        if (res) {
            //print("█ изменение в \"" + target.name + "\", unSubscibe:" + unSubscibe + " nativeTargetLayer:" + nativeTargetLayer);
            if (unSubscibe){
                bc.unSubscribeOnControllEvents(onTargetClick);
                target.layer = nativeTargetLayer;
            } else {
                if (nativeTargetLayer != target.layer && MAIN.IS_TEST)
                    Errors.showTest("Предупреждение! Туториальные объекты имеют разные слои:(" + target.layer + ":\"" + target.name + "\" и " + nativeTargetLayer + ":\"" + tTargets[0].name + "\") возможны нарушения работы с этим модулем");
                //prevTargetLayer = nativeTargetLayer
                target.layer = LayerMask.NameToLayer("Tutorial");
                bc.subscribeOnControllEvents(onTargetClick);
            }
        }
        for (int i=0; i<target.transform.childCount; i++)
            res = res | subscribeOnBaseControlls(target.transform.GetChild(i).gameObject, unSubscibe);
        return res;
    }
    // Здесь и происходит реакция на подписаную кнопку (подробнее см. функцию выше)
    void onTargetClick(BaseController btn, BaseController.TypeEvent typeEvent){
        //print("onTutorialTargetClick");
        hide();
    }
    // █ Здесь при закрытии и осуществаляется "не совсем правильный" обратный вызвов на синглтон, для продолжения вызова сообщений в его очереди...
    void onFinishHide(bool withOutCallBackCall = false){
        unSetTarget();
        //MAIN.getMain.actualInputLayer = Tutorial.prevActualMask;
        gameObject.SetActive(false);
        hideImmeaiately = false;
        for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
        ScenesController.updateGetActualInputLayer();
        //=======================[ для цепочки вызовов ]========================
        Tutorial.onTutorialFrameHide(mySubject);
    }
    void showTip() {
        var wt = WritingText.create("Нажмите на экран", new Vector2(0.0f, Screen.height * 0.001f - 5.2f),
        new Vector2(630.0f, 100.0f), 0.00001f);
        wt.text.color = new Color(1.0f, 1.0f, 1.0f);
        wt.text.alignment = TextAnchor.MiddleCenter;
        wt.text.fontStyle = FontStyle.BoldAndItalic;
        wt.text.fontSize = 60;
        var scaling = Scaling.set(wt.gameObject, 1.1f, 1.5f);
        scaling.isUnScalingTime = true;
        if (wt != null) wt.transform.SetParent(transform);
    }
    // Все ниже функции - процессы визуализации окна, которые, думаю, не требуют коментирования
    
    void Update(){
        if (screenTouch && Input.GetMouseButton(0) ) hide();
        switch (state) {
            case State.TF_SHOWING: showing(); break;
            case State.TF_HIDING:  hiding();  break;
            case State.TF_SHOW: if (!tipWasShow && Time.time - unLockTime > MAIN.timeOutTutorialUnLock * Time.timeScale) {
                    screenTouch = true;
                    tipWasShow = true;
                    if (!dontShowTip) showTip();
                } break;
        }
    }
    void hiding() {
        float timePass = Time.time - startTimeTutorialFrame;
        if (Tutorial.timeHideTutorialFrame * Time.timeScale > timePass){
            float coef = timePass / Tutorial.timeHideTutorialFrame;
            setFrameAlpha(1.0f-coef);
        } else {
            setFrameAlpha(0.0f);
            state = State.TF_HIDE;
            onFinishHide();
        }
    }
    void showing(){
        float timePass = Time.time - startTimeTutorialFrame;
        if (Tutorial.timeShowTutorialFrame * Time.timeScale > timePass) {
            float coef = timePass / Tutorial.timeShowTutorialFrame;
            setFrameAlpha(coef);
        } else {
            setFrameAlpha(1.0f);
            unLockTime = Time.time;
            state = State.TF_SHOW;
        }
    }
    void setFrameAlpha(float val) {
        currentAlpha = val;
        backGroundImage.color = Tutorial.backGroundColor * val;
        for(int i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            if (child.name == "TypingText") {
                var text = child.GetComponent<Text>();
                text.color = new Color(text.color.r, text.color.g, text.color.b, val);
            }
            //transform.GetChild(i).get
        }
    }
    public bool isShowing() { return state != State.TF_HIDE; }
    public void show(TutorialSubject subject = TutorialSubject.TS_UNDEF){
        state = State.TF_SHOWING;
        //print("█ [show] tipWasShow == " + tipWasShow);
        startTimeTutorialFrame = Time.time;
        backGroundImage.color = new Color(backGroundImage.color.r, backGroundImage.color.g, backGroundImage.color.b, 0.0f);
        gameObject.SetActive(true);
    }
    public bool hideImmeaiately = false;
    public void hide(bool Immediately = false) {
        if (state == State.TF_HIDE) return;
        tipWasShow = false;
        screenTouch = false;
        dontShowTip = false;
        state = State.TF_HIDING;
        if (hideImmeaiately || Immediately){
            state = State.TF_HIDE;
            onFinishHide(); // Immediately
        }
        else startTimeTutorialFrame = Time.time;
    }
}

/////////////////////////////////////////////////////////////////////
// этот класс выводит печатающейся текст в обучающих сообщениях
public class WritingText : MonoBehaviour {
    string line;
    float delayTypewriting;
    float lastSymbolWrittenAt;
    public Text text;
    // тут создание...
    public static WritingText create(string messeage, Vector2 pos, Vector2 size, float typewritingTextDelay = 0.01f){
        var go = new GameObject();
        go.name = "TypingText";
        go.transform.position = pos;
        var wt = go.AddComponent<WritingText>();
        wt.text = wt.GetComponent<Text>();
        if (wt.text == null) wt.text = wt.gameObject.AddComponent<Text>();
        wt.text.rectTransform.localScale = new Vector2(0.01f, 0.01f);
        wt.text.font = Font.CreateDynamicFontFromOSFont("Arial", 50);
        wt.text.color = new Color(1.0f, 1.0f, 0.5f, 1.0f);
        //text.alignment = TextAnchor.MiddleLeft;
        wt.text.fontSize = 40;
        //text.rectTransform.sizeDelta = new Vector2(600.0f, 300.0f);
        wt.text.rectTransform.sizeDelta = size;
        wt.setText(messeage, typewritingTextDelay);
        //print(go.transform.position);
        return wt;
    }
    public static bool addToWritingText(string messeage)
    {
        var go = GameObject.Find("TypingText");
        if (go == null) return false;
        var wt = go.GetComponent<WritingText>();
        wt.addToText(messeage);
        return true;
    }
    // Тут инициализация самого тескта, вызывается только в фунцкии выше
    public void setText(string typewritingText, float typewritingTextDelay = 0.025f) {
        delayTypewriting = typewritingTextDelay;
        line = typewritingText;
        text.text = "";
    }
    public void addToText(string typewritingText)
    {
        line += typewritingText;
    }

    // А тут и вся работа этого класса.
    void Update(){
        if (text != null && text.text.Length < line.Length && Time.time - lastSymbolWrittenAt >= delayTypewriting * Time.timeScale) {
            text.text = text.text + line[text.text.Length];
            lastSymbolWrittenAt = Time.time;
        }
    }
}