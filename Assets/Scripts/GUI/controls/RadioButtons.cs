using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Класс полностью описывающий такой контрол как RadioButtons (переключающиеся кнопки)
public class RadioButtons : MonoBehaviour {
    public enum TypeDisposition {
        HORIZONTAL,
        VERTICAL
    }
    public GameObject buttonPrefab;     // префаб кнопки. █ Eсли перфаб сложный класс, то для корректной работы он должен быть унаследован от RadioButton!
    public int startNumbersFrom = 1;    // █ Нумирация кнопки с указаного числа (если у нас 2 кнопки и нужно установить значение 3-4, соответственно устанавливается значение 3 )
    public int countButtons = 0;        // Количество кнопок
    public float indent;                // Отступ между кнопками
    public TypeDisposition typeDisposition = TypeDisposition.HORIZONTAL; // Горизонтальное/вертикальное построение
    public Sprite selectedSprite;       // Спрайт для подкраски выбранной кнопки
    public int selectedButtonNum = -1;  // █ Номер выбранной кнопки. -1 по умолчанию не выбрано ничего привязано к нумирации кнопок описаных выше!!
    public Vector2 shift = new Vector2(0.0f,0.0f);  // Смещение позиции всех кнопок (нужно если в текущем трансформе позицию трогать неальзя)
    public bool autoUnSelect = false;   // Автоотжатие выбранной кнопки...
    bool isInit = false;

    BaseController selectedButton = null;   // Выбранная кнопка
    Sprite defaultSprite = null;            // дефолтный спрайт кнопки (для подмены в дефалтную картинку, после переизбрания другой кнопки)
    // Подписываемся
    public delegate void OnSelectRadioBtn(BaseController selectedBtn);
    protected List<OnSelectRadioBtn> callBacks = new List<OnSelectRadioBtn>();
    public void subscribeOnRadioBtnSelected(OnSelectRadioBtn newCallBack) { callBacks.Add(newCallBack); }

    void Awake() { init(); }
    void Start() { init(); }
    // █ Некий костыль. Поскольку префаб кнопок может быть сложной структурой, которая ещё не успевает проинициализироваться, была описана эта функция, которая инициализирует их, и некоторые свои параметры...
    public void init(){ if (!isInit) init(buttonPrefab, countButtons); }
    public void init(GameObject prefabItem, int countPrefabs, TypeDisposition type, float indentItems, bool autoUnSelectBtn = false) {
        typeDisposition = type;
        indent = indentItems;
        autoUnSelect = autoUnSelectBtn;
        init(prefabItem, countPrefabs);
        isInit = true;
    }
    public void init( GameObject prefabItem, int countPrefabs ) {
        //print("init");
        countButtons = countPrefabs;
        buttonPrefab = prefabItem;
        if (buttonPrefab == null){
            print("Errro! [Start] buttonPrefab == null");
            return;
        }
        bool isHoriz = TypeDisposition.HORIZONTAL == typeDisposition;

        float buttonSize;
        BaseController baseControllerPrefab = Utils.findBaseControllIn(buttonPrefab.transform);
        SpriteRenderer sr = buttonPrefab.GetComponent<SpriteRenderer>();

        if (sr != null) {
            defaultSprite = sr.sprite;
            buttonSize = ((isHoriz) ? sr.sprite.texture.width : sr.sprite.texture.height) * 0.01f;
        } else {
            buttonSize = indent;
            defaultSprite = baseControllerPrefab.gameObject.GetComponent<SpriteRenderer>().sprite;
        }
        float buttonsSize = buttonSize * (countButtons - 1) + indent * (countButtons - 1);
        float np = buttonSize + indent;
        Vector2 nextPos = new Vector2(isHoriz ? np : 0, !isHoriz ? np : 0);
        Vector2 cursor = new Vector2(isHoriz ? -buttonsSize * 0.5f : 0, !isHoriz ? -buttonsSize * 0.5f : 0) + shift;
        for (int i = isHoriz?0:countButtons; isHoriz?(i < countButtons):(i>0); /*isHoriz ? i++ : i--*/){
            
            GameObject radioBtn = Instantiate(buttonPrefab).gameObject;
            radioBtn.name = (i + startNumbersFrom).ToString();
            SpriteRenderer _sr = radioBtn.GetComponent<SpriteRenderer>();
            var t = radioBtn.transform;
            t.parent = transform;
            t.localScale = transform.localScale;
            t.localPosition = addVectrors2(cursor, new Vector3(nextPos.x, nextPos.y, 0.0f) * i);
            BaseController bC = Utils.findBaseControllIn(radioBtn.transform);
            bC.subscribeOnControllEvents(OnButton);
            if (i == selectedButtonNum) {
                _sr.sprite = selectedSprite;
                selectedButton = bC;
            }
            if (isHoriz) i++; else i--;
        }
        if (selectedButton != null ) sendEvents(selectedButton);
        isInit = true;
    }
    // Возвращает выбранную кнопку
    public BaseController getSelectedButton() { return selectedButton; }
    Vector2 addVectrors2(Vector2 v1, Vector2 v2) { return new Vector2(v1.x + v2.x, v1.y + v2.y); }
    // Функции ниже: Проверка на то что нажата именно другая кнопка, её выбор, и соответсвенное отжатие текущей
    void OnButton(BaseController btn, BaseController.TypeEvent typeEvent){
        //print(typeEvent+ "  autoUnSelect:"+ autoUnSelect);
        if (BaseController.TypeEvent.ON_MOUSE_DOWN == typeEvent && (btn != selectedButton || autoUnSelect))
            selectBtn(btn);
        if (autoUnSelect && typeEvent == BaseController.TypeEvent.ON_MOUSE_UP)
            unselect();
    }
    public void selectBtn(int num) {
        if (transform.childCount == 0) selectedButtonNum = num;
        else if (num > transform.childCount) {
            Errors.showTest("количество кнопок меньше чем номер выбранной");
        } else { 
            var t = transform.GetChild(num);
            selectBtn(Utils.findBaseControllIn(t));
        }
    }
    void selectBtn(BaseController btn) {
        //print(111);
        SoundsSystem.play(Sound.S_RADIO_BUTTON);
        if (selectedSprite != null)
            btn.GetComponent<SpriteRenderer>().sprite = selectedSprite;
        unselect();
        //selectedButtonNum = int.Parse( btn.name );
        selectedButton = btn;
        sendEvents(btn);
    }
    void unselect(){
        if (selectedButton != null)
            selectedButton.GetComponent<SpriteRenderer>().sprite = defaultSprite;
        selectedButton = null;
        selectedButtonNum = -1;
    }
    // Рассылка события
    void sendEvents(BaseController btn){
        if (callBacks.Count > 0)
            for (int i = 0; i < callBacks.Count; i++)
                if (callBacks[i] != null) { callBacks[i](btn); }
                else callBacks.Remove(callBacks[i]);
    }
}
