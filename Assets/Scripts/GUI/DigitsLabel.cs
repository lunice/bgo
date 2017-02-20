using UnityEngine;
using System.Collections;
////////////////////////////////////////////////////////////////
// Клас для вывода цифровых строк в виде спрайтов.
// █ Для установки спрайтов, нужно загрузить их в масив: spriteDigits заранее в префабе
// █ В силу некоторых сложностей с точным выравниванием строки, была введена переменная shiftDigits с помощью которой так же заранее в префабе выставляются нужные коректировки
//   Так же есть scalingIndent для регулировки отступа между символами строки.
//   И возможность устанавливать картинку в конце строки, которая автоматически подгоняется по размерам, но только по вертикали!
// █ Строка поддерживает символы '-', '+', '.' (первые два добавляются 11 и 12 элементом в масиве цифр) а точка помещается в отдельный спрайт.

public class DigitsLabel : MonoBehaviour {
    public enum Justification {
        CENTER,
        RIGHT,
        LEFT
    }
    public enum AdditionalPrefixSymbols {
        NONE,
        PLUS
    }
    public enum TypeLabel {
        INT = 0,
        FLOAT
    }

    public TypeLabel typeLabel = TypeLabel.INT;                 // текущий тип строки (интовое / дробовое)
    public Sprite[] spriteDigits = null;                        // ресурсы цирф а так же - и + (последнии два устанавливать не обязательно)
    public Sprite dot = null;                                   // ресурсы точки
    public Sprite picInTheEnd = null;   // картинка в конце     // ресурсы иконки в конце строки
    public Vector2 shiftDigits = Vector2.zero;                  // смещение позиции цифр
    public float indent = 0.0f;                                 // межсимвольный отступ
    public Justification justification = Justification.CENTER;  // выравнивание
    public int orderLayer;                                      // █ ордер устанавливается на каждый символ при их создании. Учитывайте при динамическом изменении ордера!
    public float scale = 1.0f;                                  // для установки маштаба цифр. Из-за того что к текущему объекту может прилаживаться подложка и нужно сохранить её маштаб, был введён дополнительно маштаб, который задаёт маштаб цифр
    AdditionalPrefixSymbols additionalPrefixSymbols = AdditionalPrefixSymbols.NONE; // используется при наличии символа '+'
    float scalingIndent;                                        // тот же indent только с поправкой на локальный маштаб
    float labelWidth;                                           // общая ширина строки на текущий момент (нужно для выравнивания строки)

    public int iValue = 0;                                      // значение строки (интовое)
    public float fValue = 0;                                    // значение строки (дробное)
    public string sValue = "";                                  // значение строки (в виде строки)

    void Awake() { scalingIndent = indent * transform.localScale.x; }
    void Start () {}
	void Update () {}
    // ширина цифровой картинки
    float getWidthDigit(int index) {
        var sr = transform.GetChild(index).GetComponent<SpriteRenderer>();
        return sr.sprite.texture.width;
    }
    // ниже функции выравнивания
    void align() { align(iValue.ToString()); }
    void align(string vS) {
        float cursorPos = 0;
        
        if (transform.childCount == 0) {
            Errors.showTest("Client Error! [align] faled init digit_label: childCount == 0. DetailInfo(name:" + this.name + " ,value:" + vS + ")");
            return;
        }
        float firstSW = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.rect.width;
        switch (justification) {
            case Justification.CENTER: cursorPos = -labelWidth * 0.5f; break;
            case Justification.LEFT: cursorPos = 0.0f; break;
            case Justification.RIGHT: cursorPos = -labelWidth; break;
        }
        //print("[align] pic width:" + labelWidth);
        //print("[align] cursorPos:" + cursorPos);
        //transform.localPosition = Vector3.zero;
        for (int i = 0; i < vS.Length; i++) {
            var t = transform.GetChild(i);
            var sr = t.GetComponent<SpriteRenderer>();
            float spriteWidth = 0.0f;
            if (sr.sprite != null) {
                spriteWidth = sr.sprite.rect.width * 0.01f;
                if (sr.sprite == picInTheEnd) cursorPos += 0.1f;
            }
            t.localPosition = new Vector3(cursorPos + shiftDigits.x, shiftDigits.y, 0.0f);
            cursorPos += spriteWidth + scalingIndent;
            //print(spriteWidth + scalingIndent);
            //print("[align] spriteWidth:" + spriteWidth);
            
            //print("sprite name:" + sr.sprite.name + ":" + i + ":" + spriteWidth + ": curPos = " + cursorPos + " t.localPosition:"+ t.localPosition+ " t.position"+ t.position);
        }
    }

    // интерфейсные функции
    public void setJustification(Justification j) { justification = j; }
    public int getValue() { return iValue; }
    public float getFloatValue() { return fValue; }
    public void addFloatValue(float v) { setFloatValue(fValue += v); }
    public void addValue(int v) {
        int res = iValue += v;
        setValue(res);
    }
    public bool checkOnNegativeBalanceAndShow(float val)
    {
        bool res = val >= 0;
        SpriteRenderer msr = GetComponent<SpriteRenderer>();
        if (msr != null) msr.color = res ? Color.white : Color.red;
        return res;
    }
    public void setFloatValue(float v, AdditionalPrefixSymbols addPrefixSymbols = AdditionalPrefixSymbols.NONE) {
        additionalPrefixSymbols = addPrefixSymbols;
        labelWidth = 0;
        if (!checkOnNegativeBalanceAndShow(v)) print("Error! Gold value is negative");
        fValue = v;
        typeLabel = TypeLabel.FLOAT;
        setValue(v.ToString(), fValue);
    }
    public void setValue(int v, AdditionalPrefixSymbols addPrefixSymbols = AdditionalPrefixSymbols.NONE) {
        additionalPrefixSymbols = addPrefixSymbols;
        labelWidth = 0;
        //if (!checkOnnegativeBalanceAndShow(v) ) print("Error! Gold value is negative");
        iValue = v;
        typeLabel = TypeLabel.INT;
        setValue(v.ToString(), iValue);
    }
    // основновной функционал задания числа
    void setValue(string vS, float v){
        labelWidth = 0;
        if (additionalPrefixSymbols == AdditionalPrefixSymbols.PLUS) vS = '+' + vS;
        if (picInTheEnd != null) vS += '█';             // этот символ означает наличия картинки
        float scalePicTo = 1.0f;                        // для подгонки размера картинки
        Sprite curSprite = null;                        // текущий рисуемый спрайт
        ////////////////////////////////////////////////// основной цикл рисования:
        for (int i = 0; i < vS.Length; i++) { 
            GameObject go;
            SpriteRenderer sr;
            if (i == 0) {
                if (v < 0) { // проверка на наличие символа '-' и его отрисовка если значение отрицательное
                    if (spriteDigits.Length >= 11) curSprite = spriteDigits[10]; 
                    else { /*print("Error! [setValue] additiona simbol '-' not supported");*/ i++; }
                } else if (vS[i] == '+') {  
                    if (spriteDigits.Length >= 12) curSprite = spriteDigits[11]; // отрисовка плюса если есть карстинка в ресурсах
                    else { print("Error! [setValue] additiona simbol '+' not supported"); i++; }
                } else {
                    //print("i == " + i);
                    //print("spriteDigits.Length == "+spriteDigits.Length);
                    curSprite = spriteDigits[vS[i] - 48];       // задаётся само число по символьно
                }
            } else if (vS[i] == '.') { curSprite = dot;         // рисует току
            } else if (vS[i] == '█') {                          // рисует картинку
                scalePicTo = spriteDigits[0].rect.height / picInTheEnd.rect.height; // подгонка размера картинки
                curSprite = picInTheEnd;
            } else curSprite = spriteDigits[vS[i] - 48];
            //////////////////////////////////////////////////////////////////////////////////////////////
            // █ ниже идёт проверка на существующие символы если они есть меняется их картинка, если нет, создаются, а если есть лишние удаляются
            if (transform.childCount > i) {
                go = transform.GetChild(i).gameObject;
                go.transform.localScale = new Vector3(scale, scale, scale);
                sr = go.GetComponent<SpriteRenderer>();
            } else {
                go = new GameObject();
                go.name = "digit" + i;
                //print(" go.transform.parent:\"" + go.transform.parent + "\" transform:\""+ transform + "\" nameLabel:\""+name+ "\" value:\""+vS+"\" i=="+i);

                go.transform.parent = transform;
                go.transform.localScale = new Vector3(scale, scale, scale);
                sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = orderLayer;
            }
            sr.sprite = curSprite;
            if (vS[i] == '█') sr.transform.localScale = new Vector3(scalePicTo, scalePicTo, scalePicTo);
            /*if (MAIN.getMain.testPrintDigits) {
                print(" set digit:\"" + go.name + "\"  value:" + vS[i] + "  order in layer:" + sr.sortingOrder + "  pos:" + go.transform.position+ "  localPos:"+go.transform.localPosition);
            }*/
            if (sr.sprite != null) labelWidth += sr.sprite.rect.width * 0.01f;
        }
        // удаление лишних символов
        if (transform.childCount > vS.Length)
            for (int i = vS.Length; i < transform.childCount; i++)
                Destroy(transform.GetChild(i).gameObject);
        //////////////////////////////////////////////////////////////////////////////////////////////
        labelWidth += (vS.Length - 1) * scalingIndent;  // подсчёт длины строки
        align(vS);                                      // выравнивание символов по окончанию
    }
}
