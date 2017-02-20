using UnityEngine;
using System.Collections;
// GUI компонент, который реагирует на нажатие и меняет своё состояние true/false
// По правильному нужно было назвать класс CheckBox
public class CheckButton : BaseController{
    GameObject no = null;       // Объект на котором спрайт, что обозначает отрицательное значение
    public Vector2 shiftNo;     // Смещенния объекта описаного выше
    // Значение задаётся и получается через этот интерфейс
    public bool value {
        get { return (no != null && no.activeSelf); }
        set {
            if (value) {
                if (no == null) createNo();
                else no.SetActive(true);
            } else if (no != null) no.SetActive(false);
        }
    }
    // При отрицательном значении, на иконке кнопки, рисуется перечёркнутый круг. Здесь он и создаётся.
    void createNo() {
        no = new GameObject("noPic");
        no.transform.parent = transform;
        no.transform.localPosition = shiftNo;
        var sr = no.AddComponent<SpriteRenderer>();
        var parentSR = GetComponent<SpriteRenderer>();
        sr.sortingOrder = parentSR.sortingOrder + 1;
        var resGO = GameObject.Find("RESOURCES");
        if (resGO == null) {
            Errors.showTest("ресурсы не найдены! (При попытке инициализации CheckBox)");
            return;
        }
        RESOURCES res = resGO.GetComponent<RESOURCES>();
        sr.sprite = res.checkBoxFalse;
        
    }
    // Событие нажатия мыши
    public override bool onMouseDown(){
        bool onConroller = base.onMouseDown();
        if (onConroller) {
            value = !value;
            SoundsSystem.play(Sound.S_CHECK_BUTTON);
            //for (int i = 0; i < callBacks.Count; i++)
            //    callBacks[i](this, TypeEvent.ON_MOUSE_DOWN);
        }
        return onConroller;
    }
}
