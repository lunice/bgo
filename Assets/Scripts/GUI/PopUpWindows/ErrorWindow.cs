using UnityEngine;
//using System.Collections;
using UnityEngine.UI;
// Класс отвечающий за визуальное отображение окна
public class ErrorWindow : MonoBehaviour {
    public float marginsSize = 2.0f;  // размер краёв, для композитивного окна
    public float buttonSize = 200.0f;// максимальная ширина кнопки
    const float buttonHeight = 160.0f;
    public float buttonsIndent = 0.1f; // отступ между кнопками
    int prevActualLayer;                // предведущий слой маски тачей
    //public float minButtonSize = 280.0f;
    //[SerializeField]
    //private InputField nameInputField = null;   
    public Errors.TypeError typeError;  // тип ошибки

    void Start () {}
	void Update () {}
    // получить все кнопки
    Button[] getAllButtons() {
        var buttons = transform.FindChild("buttons");
        Button[] res = new Button[buttons.childCount];
        for(int i=0; i< buttons.childCount; i++)
            res[i] = buttons.GetChild(i).GetComponent<Button>();
        return res;
    }
    // Получить кнопку за именем
    Button getButtonByCaption(string caption){
        var buttons = transform.FindChild("buttons");
        for (int i = 0; i < buttons.childCount; i++){
            var btn = buttons.GetComponent<Button>();
            Text textBtn = btn.transform.FindChild("buttonText").GetComponent<Text>();
            if (textBtn.text == caption)
                return btn;
        }
        MAIN.getMain.setMessage("Error! [getButtonByCaption] not find button with caption:\"" + caption + "\"");
        return null;
    }
    // █ инициализация за указанными параметрами обязательна, после создания окна
    public void init(string text, Errors.TypeError typeError_ = Errors.TypeError.E_NONE, params string[] btnsText) {
        //print("█ start init error window!");
        //MAIN.getMain.actualInputLayer;
        ScenesController.updateGetActualInputLayer();
        //MAIN.getMain.actualInputLayer = 0;
        typeError = typeError_;
        var cr = transform.FindChild("windowText").GetComponent<CanvasRenderer>();
        var uiText = transform.FindChild("windowText").GetComponent<Text>();
        
        RectTransform windRT = transform.GetComponent<RectTransform>();
        uiText.text = text;

        int countBtns = btnsText.Length;
        Button[] buttonsInWindow = getAllButtons();
        Transform btnT = transform.FindChild("slicedButton");
        if (countBtns == 0) btnsText = new string[1] { "OK" };

        float buttonSize = (windRT.sizeDelta.x - marginsSize) / countBtns - (buttonsIndent * countBtns);

        var buttons = transform.FindChild("buttons");
        var windBut = getAllButtons();
        //print("█ count buttons:"+ windBut.Length);
        if (windBut.Length > btnsText.Length)
            for (int i = btnsText.Length; i < windBut.Length; i++) {
                print("destroy button#" + i);
                Destroy(windBut[i].transform);
            }
        var img = buttonsInWindow[0].GetComponent<Image>();
        float indent = 0.3f;//30.0f;
        float buttonSizeX = 4.4f;//240.0f;//img.preferredWidth;
        float totalSizeX = buttonSizeX * btnsText.Length + indent * (btnsText.Length - 1);
        //float cursor = -(totalSizeX * 0.5f) + buttonSizeX * 0.5f;
        //Vector2 centerBtnsPos = buttonsInWindow[0].transform.localPosition;
        //RectTransform rt = btn.GetComponent<RectTransform>();
        Vector2 centerBtnsPos = buttonsInWindow[0].GetComponent<RectTransform>().position;
        //print("centerBtnsPos:" + centerBtnsPos);
        //print("count btns:" + btnsText.Length);

        float startPosX = -totalSizeX * 0.5f + buttonSizeX * 0.5f + buttons.GetComponent<RectTransform>().position.x;
        //print("startPosX: " + startPosX);
        int windowCountBtns = buttonsInWindow.Length;
        for (int i = 0; i < btnsText.Length; i++){
            Button btn;
            if (windowCountBtns < btnsText.Length) {
                windowCountBtns++;
                var buttonT = Instantiate(buttons.GetChild(0));
                btn = buttonT.GetComponent<Button>();
                btn.transform.parent = buttons;
            } //else btn = buttons.GetChild(i).GetComponent<Button>();
            //var img = btn.GetComponent<Image>();
            //btn.transform.position = cursor
        }

        for (int i = 0; i < btnsText.Length; i++) {
            Button btn = buttons.GetChild(i).GetComponent<Button>();
            Text textBtn = btn.transform.FindChild("buttonText").GetComponent<Text>();
            //print("█ textBtn == " + textBtn.text);
            RectTransform rt = btn.GetComponent<RectTransform>();
            textBtn.text = btnsText[i];
            //print("█ textBtn == " + textBtn.text + " after change!");
            btn.transform.localScale = new Vector2(1.0f, 1.0f);
            //rt.rect = new Rect(rt.rect.x, rt.rect.y, buttonSizeX, rt.rect.height)
            rt.sizeDelta = new Vector2(380, buttonHeight);

            float cursor = startPosX + i * (buttonSizeX + indent);
            //print("█ cursor btn:'+" + btn.name + "+':" + cursor);
            //btn.transform.localPosition = new Vector2(cursor*10.0f, centerBtnsPos.y);
            rt.position = new Vector2(cursor, centerBtnsPos.y);
            //btn.transform.parent = buttons.transform;
            btn.transform.SetParent(buttons.transform);
            btn.onClick.AddListener(() => {
                //print(textBtn == null);
                Errors.onErrorButtonClick(this, textBtn.text);
            });
        }
        //alignButtons();
    }
    // по идеи ниже текста ошибки более мелким тестом должно было выводится техническая информация
    public void addDetails(string text) {
        // TODO details
    }
    public void setAction(int indexButton, UnityEngine.Events.UnityAction action) // ███ ДОБАВИТЬ действие указаной кнопке за индексом в масиве кнопок, по умолчанию каждая кнопка просто закрывает окно! Потому действия выполнятся после действия по умолчанию
    {
        getAllButtons()[indexButton].onClick.AddListener(action);
    }
    // установить действие указаной кнопке за именем в масиве кнопок, по умолчанию каждая кнопка просто закрывает окно
    public void setAction(string buttonCaption, UnityEngine.Events.UnityAction action) {
        getButtonByCaption(buttonCaption).onClick.AddListener(action);
    }
    // (реализовано в другой функции) выравнивание кнопок
    void alignButtons() {
        //var windBut = getAllButtons();
    }
    // █ (удаляет) спрятать окно
    public void hideWindow() {
        //MAIN.getMain.actualInputLayer = prevActualLayer;
        //transform.parent.gameObject.SetActive(false);
        Destroy(transform.parent.gameObject);
        ScenesController.updateGetActualInputLayer();
    }
}
