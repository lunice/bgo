using UnityEngine;
using System.Collections;
// Класс отвечающий за управление и отображение HUD-а и отдельных его элементов
public enum VisualValuesType    // перечисление визуально отображаемых основных значений в HUD
{
    VVT_MONEY,      // деньги
    VVT_RUBINS,     // рубины
    VVT_XP          // опыт
}

public class HUD : MonoBehaviour {
    public Transform left;     // левая часть
    public Transform right;    // правая
    public Transform top;      // верхняя часть
    public Transform bottom;   // нижняя часть
    float yScale;       // переменная для вычисления маштабирования относительно резрешения экрана устройства по вертикали
    MAIN main = MAIN.getMain; // для удобного доступа
    //public Vector2 defaultScreen = new Vector2(1920, 1080); // дефолтные значение экрана с которого высчитвывается коофициент маштабирования
    public const float defaultScreenX = 1920;
    public const float defaultScreenY = 1080;
    public const float halfDefaultScreen = 9.6f;    // половина расстояния ширины экрана ( в измерительной системе Unity )

    // перечень основных кнопок HUD-а
    PushDownButton moneyBtn, expBtn, bonusesBtn, rubinsBtn, buyBallBtn;
    BaseController backBtn, settingsBtn;

    public float hudScale = 1.0f;
    public const float minRaffleProportion = 1.74f;
    public const float minDistBetweenFirstSecondPartsOfTube = 1.075818f; // минимальное расстояние между первой и второй частями трубы
    public const float minDistBetweenSecondthirdPartsOfTube = -0.012091f; // минимальное расстояние между первой и второй частями трубы
    public const float maxDistBetweenSecondthirdPartsOfTube = 9.0f;
    public const float maxDistBetweenFirstSecondPartsOfTube = 4.0f;
    const float minHUDproportion = 1.473f; // минимальные пропорции отображения HUD-а. ( приблизительно 4.5/3 )
    public float halfLeftWidth;
    public float halfRightWidth;
    public static bool isVisibleLeftRight; // видимы ли поля по бокам

    static HUD hud;
    bool isInit = false;
    public static HUD getHUD {
        get {
            if (hud == null) return null;//hud = new HUD();
            if (!hud.isInit) hud.isInit = hud.init();
            return hud;
        }
    }

    void Awake(){
        //if (!isInit) init();
    }
    void Start () {
        if (!isInit) init();
        calculateScales();
    }
    void Update() {
        //calculateScales();
    }


    public void calculateReceivingTraySize() {
        var rt = main.receivingTray;
        if (rt == null) return;
        var tube = rt.transform.FindChild("Lottotron").FindChild("Tube");
        var tube2 = rt.transform.FindChild("Tube");
        if (tube == null) print("Error tube == null");
        Transform part1 = tube.GetChild(0);
        Transform part2 = tube.GetChild(1);
        Transform part3 = tube.GetChild(2);
        float leftTubePos = left.position.x + 5.0f - (!isVisibleLeftRight ? halfLeftWidth * 0.5f : 0);
        float rightTubePos1 = right.position.x - 6.0f + (!isVisibleLeftRight ? halfRightWidth * 0.5f : 0);
        float rightTubePos2 = rightTubePos1;
        float diff = rightTubePos1 - leftTubePos;
        if (diff < minDistBetweenFirstSecondPartsOfTube) {
            rightTubePos1 += minDistBetweenFirstSecondPartsOfTube - diff;
            diff = rightTubePos2 - leftTubePos;
            if ( diff < minDistBetweenSecondthirdPartsOfTube)
                rightTubePos2 += minDistBetweenSecondthirdPartsOfTube - diff;
        } else {
            if ( diff > maxDistBetweenFirstSecondPartsOfTube) {
                float d = diff - maxDistBetweenFirstSecondPartsOfTube;
                rightTubePos1 -= d;
                float diff2 = rightTubePos2 - leftTubePos;
                if ( diff2 > maxDistBetweenSecondthirdPartsOfTube)
                {
                    d = (diff2 - maxDistBetweenSecondthirdPartsOfTube) * 0.5f;
                    leftTubePos += d;
                    rightTubePos2 -= d;
                }
            }
        }

        GameObject go = GameObject.Find("buyBallBtn");
        if (go != null && right.position.x < go.transform.position.x )
        {
            GameObject g = GameObject.Find("BottomButtons");
            g.transform.position = g.transform.position + new Vector3(right.position.x - go.transform.position.x, 0.0f);
            
        }
        part2.position = new Vector2(leftTubePos, part2.position.y);
        part1.position = new Vector2(rightTubePos1, part1.position.y);
        part3.position = new Vector2(rightTubePos2, part3.position.y);
    }


    public static float getProportionScreen() // пропорция экрана относительно дефолтного
    {
        float xToDefault = Screen.width / defaultScreenX;
        float yToDefault = Screen.height / defaultScreenY;
        return (xToDefault != yToDefault) ? xToDefault / yToDefault : 1.0f;
    }
    //public static float xProportionScreen;
    public void calculateScales() // расчёт позиций левой и правой части HUD-а
    {
        float xProportionScreen = getProportionScreen();
        isVisibleLeftRight = (float)Screen.width / Screen.height > minHUDproportion;
        left.GetComponent<SpriteRenderer>().enabled = isVisibleLeftRight;
        right.GetComponent<SpriteRenderer>().enabled = isVisibleLeftRight;
        float crutch1 = 0.2f; // █ (костыль), обрезается на эту велечину
        float leftAndRightHalfSize = halfLeftWidth + crutch1;
        left.localPosition = new Vector2(-halfDefaultScreen * xProportionScreen + leftAndRightHalfSize, left.localPosition.y);
        right.localPosition = new Vector3(halfDefaultScreen * xProportionScreen - leftAndRightHalfSize, left.localPosition.y);
        /////// calculate ReceivingTray part position 
        calculateReceivingTraySize();
    }

    const float minXProportion = 1.74f;
    bool init() // █ инициализация HUD-а и подгонка его элементов под особенности текущего экрана
    {
        left = transform.FindChild("Left");
        right = transform.FindChild("Right");
        top = transform.FindChild("Top");
        bottom = transform.FindChild("Bottom");
        halfLeftWidth = getWidth(left) * 0.005f;
        halfRightWidth = getWidth(right) * 0.005f;

        Vector2 sizeLeft = getHUD_elementSize(left);
        Vector2 sizeBottom = getHUD_elementSize(bottom);
        Vector2 sizeRight = getHUD_elementSize(right);
        Vector2 sizeTop = getHUD_elementSize(top);
        calculateScales();

        moneyBtn = GameObject.Find("moneyBtn").GetComponent<PushDownButton>();
        expBtn = GameObject.Find("expBtn").GetComponent<PushDownButton>();
        buyBallBtn = GameObject.Find("buyBallBtn").GetComponent<PushDownButton>();
        bonusesBtn = GameObject.Find("bonusesBtn").GetComponent<PushDownButton>();
        rubinsBtn = GameObject.Find("rubinsBtn").GetComponent<PushDownButton>();
        backBtn = GameObject.Find("backBtn").GetComponent<BaseController>();
        settingsBtn = GameObject.Find("settingsBtn").GetComponent<BaseController>();
        // █ подключение кнопок на обработку их событий в классе MAIN.
        buyBallBtn.subscribeOnControllEvents(main.onButtonClick);
        expBtn.subscribeOnControllEvents(main.onButtonClick);
        moneyBtn.subscribeOnControllEvents(main.onButtonClick);
        bonusesBtn.subscribeOnControllEvents(main.onButtonClick);
        rubinsBtn.subscribeOnControllEvents(main.onButtonClick);
        backBtn.subscribeOnControllEvents(main.onButtonClick);
        settingsBtn.subscribeOnControllEvents(main.onButtonClick);
        // █ инициализация игровых сердств
        main.updateMyRubins();
        main.updateMyMoney();

        isInit = true;
        hud = this;
        return isInit;
    }
    public static BaseController getBackButton() { return getHUD.backBtn; }
    public static BaseController getBuyBallButton() { return getHUD.buyBallBtn; }
    public static void showBuyRubins() { getHUD.rubinsBtn.show(); }
    public static void hideBuyRubins() { getHUD.rubinsBtn.hide(); }
    public static void hideBuyBallBtn() { getHUD.buyBallBtn.hide(); setEnableBuyBall(false); }
    public static void showBuyBallBtn() { getHUD.buyBallBtn.show(); setEnableBuyBall(true); }
    public static void setEnableBackButton(bool value){
        getHUD.backBtn.setEnable(value);
        if (value && !MAIN.getMain.handlerServerData.isAvailableNextBall)
            Tutorial.show(TutorialSubject.TS_EXIT);
    }
    public static void setEnableBuyBall(bool value) {
        getHUD.buyBallBtn.setEnable(value);
        if (value) Tutorial.showBefore(TutorialSubject.TS_EXIT, TutorialSubject.TS_BUY_BALL);
    }
    public static bool isVisible() // отображается ли HUD
    {
        return getHUD.gameObject.activeInHierarchy;
    }
    // (испольузется только в одном месте) ниже функции были описаны для удобства получения ширины игровых объектов
    float getWidth(Transform t) { return getWidth(t.gameObject); }
    float getWidth(GameObject go) {
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr.sprite.texture.width;
        print("Error! [getWidth] sr == null");
        return 0.0f;
    }
    // показать части HUD-а
    public static void showTop() { getHUD.top.gameObject.SetActive(true); }
    public static void hideTop() { getHUD.top.gameObject.SetActive(false); }

    Vector2 getHUD_elementSize(Transform element) // возвращает размеры объекта при наличии SpriteRenderer в нём в измерительных единицах Unity
    {
        SpriteRenderer sp = element.GetComponent<SpriteRenderer>();
        if (sp) {
            return new Vector2(sp.sprite.texture.width * yScale, sp.sprite.texture.height * yScale);
        }
        //print("Error![getHUD_elementSize]");
        return new Vector2(0.0f, 0.0f);
    }

    public static void playAnimNeedMoreMoney()  // █ (недоделано) проигрование звуков и анимации при нехватке денег
    {
        //Errors.showError(Errors.TypeError.EC_NOT_ENOUGH_MONEY); // █ В окне ошибок прописаны дополнителные действия по закрытию окна смотреть в Errors
        getHUD.playAnimNeedMore(VisualValuesType.VVT_MONEY);

    }
    public static void playAnimNeedMoreRubins() // █ (недоделано) проигрование звуков и анимации при нехватке рубинов
    {
        //Errors.showError(Errors.TypeError.EC_NOT_ENOUGH_RUBINS); // █ В окне ошибок прописаны дополнителные действия по закрытию окна смотреть в Errors
        getHUD.playAnimNeedMore(VisualValuesType.VVT_RUBINS);
    }

    public static bool isPlaingAnim(){
        return getHUD._isPlaingAnim;
    }
    bool _isPlaingAnim = false;
    int changeOrderValueForNeedMoreButton = 30;
    public void playAnimNeedMore(VisualValuesType typeT){
        PushDownButton target = null;
        DigitsLabel targetD = null;
        MAIN main = MAIN.getMain;
        switch(typeT) {
            case VisualValuesType.VVT_MONEY: { target = moneyBtn; targetD = main.money; } break;
            case VisualValuesType.VVT_RUBINS: { target = rubinsBtn; targetD = main.rubins; } break;
            case VisualValuesType.VVT_XP: { target = expBtn; /* EXP! */ } break;
            default: { if (MAIN.IS_TEST) { Errors.show("Неизвестный тип отображаемых значений HUD-a:" + typeT); } } return;
        }
        SoundsSystem.play(Sound.S_NEED_MORE_FUNDS);
        if (target.GetComponent<Scaling>() != null) return;
        Utils.increaseOrder(target.transform, changeOrderValueForNeedMoreButton);
        var scalingAnim = Scaling.set(target.gameObject, 1.3f, 1.5f, 1);
        scalingAnim.subscribeOnScalingFinish(onNeedMoreScalingAnim);
        ChangeColor.set(targetD.transform, new Color(1.0f, 0.0f, 0.0f, 1.0f),0.25f,true,6);
        _isPlaingAnim = true;
    }
    void onNeedMoreScalingAnim(GameObject target)
    {
        _isPlaingAnim = false;
        Utils.increaseOrder(target.transform, -changeOrderValueForNeedMoreButton);
        if (target == moneyBtn.gameObject){
            if (!Tutorial.show(TutorialSubject.TS_BUY_GOLD_BTN))
                WindowController.showPopUpWindow(WindowController.TypePopUpWindow.GOLD_EXCHANGE);
        }
        else if (target == rubinsBtn.gameObject) WindowController.showPopUpWindow(WindowController.TypePopUpWindow.CRYSTALS_BUY);
    }

    FontaineCoins exchengeFontainCoins = null;  // эффект фонтана монет при обмене рубинов на золото
    public int flyExchengeMoneyToPocket // организация доступа и функционала фонтана монет при обмене рубинов на золото
    {
        //get { return (exchengeFontainCoins == null) ? 0 : exchengeFontainCoins.getTotalCount(); }
        set{
            if (exchengeFontainCoins == null){
                exchengeFontainCoins = Effects.addFontaineCoins(main.rubins.transform.parent.gameObject, main.money.gameObject, value, 0.0125f);
                exchengeFontainCoins.shiftStartPosOn(new Vector2(0.6f, 0.6f));
                exchengeFontainCoins.setNominalCoin(250);
            } else exchengeFontainCoins.setTotalCount(value);
            if (value > 0) SoundsSystem.play(Sound.S_GOLD_BUY);
        }
    }
}
