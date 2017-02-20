using UnityEngine;
using System.Collections;
// Класс содержащий в себе часть игровых ресурсов, часть находится внутнри соответсвенных классов
public class RESOURCES : MonoBehaviour {
    /*public GameObject hudPrefab;
    public GameObject autorizationPrefab;
    public GameObject mainMenuPrefab;
    public GameObject buyTicketsPrefab;
    public GameObject rafflePrefab;*/
    /*public static RESOURCES getResources  // IT NOT WORK! :( попытка создания удобного доступа, и самопроверки на своё же присутствие
    {
        get {
            GameObject go = GameObject.Find("RESOURCES");
            RESOURCES res = null;
            if (go == null) {
                Object resources = Resources.Load<Object>("Prefabs/RESOURCES");
                if (resources == null) MAIN.getMain.setMessage("[getResources] cannot load resourses prefab!", true);
                go = Instantiate(resources) as GameObject;
                go.name = "RESOURCES";
                res = go.AddComponent<RESOURCES>();
                GameObject.DontDestroyOnLoad(go);
            } else res = go.GetComponent<RESOURCES>();
            return res;
        }
    }*/

    public static Object getPrefab(string name) // описание доступа по типу синглтона
    {
        Object res = Resources.Load<Object>("Prefabs/"+name);
        if ( res == null ) 
            Errors.showTest("[getResources] cannot load \"" + name + "\" prefab!");
        return res;
    }
    // ███ Внимание! При отключении(коментировании) каких либо ресурсов, из списка ниже
    // может привести к потерям установок на заготовленном префабе RESOURCES
    // перед чисткой убедитесь что в проекте ресурсы нигде не используются
    //------- основные игровые спрайты символьного типа
    public Sprite[] backDigits;             // (не используется) чёрные цифры
    public Sprite[] whiteDigits;            // (не используется) белые цифры
    public Sprite[] ticketDigits;           // цирфы под клетки билетов
    public Sprite[] moneyDigits;            // цифры под лейбел денег

    public Sprite[] yellowChars;            // жёлтые символы
    public Sprite[] cyrillicYellowChars;    // жёлтые символы кирилица
    public Sprite[] greenChars;             // зелёные символы
    //------- основные игровые спрайты
    public Sprite ball;                     // спрайт шара
	public Sprite greenBall;                // (не используется) спрайт зелёного шара
	public Sprite BallAureole1;             // ареол шара тип1
	public Sprite BallAureole2;             // (какой-то из ариолов не используется)ареол шара тип1
    public Sprite horseshoeTicketCell;      // (не используется и не по дизайну) клетка - подкова
    public Sprite checkBoxFalse;            // отрицательное значение checkBox
    //------- основные игровые префабы
    public GameObject templateElement;      // префаб отображаемого шаблона, в верхней позиции экрана в розыгрыше
    public GameObject coinPrefab;           // префаб монеты
    public GameObject popUpWndPrefab;       // префаб всплывающего окна покупки кристалов и обмена кристалов на золото
    public GameObject crystalBuyItemPrefab; // префаб элемента списка покупки кристалов
    public GameObject goldBuyItemPrefab;    // префаб элемента списка обмена кристалов на золото
    public GameObject errorWindPrefab;      // префаб окна ошибки
    //------- звуки
    public AudioClip coinDropSound;         // падения монеты
    public AudioClip buttonPushSound;       // нажатие кнопки
    public AudioClip ballKickSound;         // соударение шаров
    public AudioClip winSound;              // малый выиграшь
    public AudioClip bigWinSound;           // крумный выиграшь
    public AudioClip starsDisapear;         // сокрытие шаров, по окончанию розыгрыша
    public AudioClip dragon;                // приоткрытие челюсти дракона
    public AudioClip dragonFull;            // полное открытие челюсти дракона
    public AudioClip prewin;                // появление превина
    public AudioClip ticketTurn;            // поворот билета
    public AudioClip startTransform;        // исчезновение звёзд, при прибытие на клетки билетов
    public AudioClip goldBuy;               // (покупка) обмен монет
    public AudioClip rubinsBuy;             // покупка рубинов
    public AudioClip errorSound;            // звук ошибки
    public AudioClip radioBtn;              // звук радиобатомной кнопки
    public AudioClip checkBtn;              // звук CheckBox-ов в меню настроек
    public AudioClip needMoreFunds;         // недостаточно средств
    public AudioClip musik;                 // музыка

    public AudioClip musikEnter;            // (нет ресурсов, логика не реализована) вохдная музыкальная часть
    public AudioClip[] musikAfrickanStyle;  // (нет ресурсов, логика не реализована) музыкальные африканские части
    public AudioClip[] musikFankStyle;      // (нет ресурсов, логика не реализована) музыкальные фанковые части

    void Start () {}
	void Update () {}
}
