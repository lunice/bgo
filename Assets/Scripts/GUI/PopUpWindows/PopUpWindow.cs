using UnityEngine;
using System.Collections;
// Класс всплывающее окно для покупки рубинов, обмена золота, и на строек, имеет общий базовый класс описаный ниже
public class PopUpWindow : MonoBehaviour {
    enum PopUpWindState // основные состояния окна
    {
        HIDE,   // окно спрятано
        HIDING, // окно в процессе сокрытия
        SHOWING,// окно в процессе появления
        SHOW    // окно отображется
    }
    PopUpWindState state = PopUpWindState.HIDE;
    public Vector2 hidePosition = new Vector2(0.0f, -4.8f); // захаркодженая позиция сокрытого окна, куда оно стремится при сокрытии
    public Vector2 showPosition = new Vector2(0.0f, 0.5f);  // захаркодженая позиция отображаемого окна (вблизи центра)
    GameObject _content;            // контент окна
    public float speedMove = 0.15f; // скорость анимации отображения/сокрытия
    Flying flyTo;                   // эффект для удобного доступа, с помощьу которого работает анимация.

    virtual public GameObject createContent() // должно переопредлятся в унаследованных классах
    {
        return null;
    }

    public bool isActive() // более удобная функция, возвращаяющаяя состояние окнаы
    {
        return state == PopUpWindState.SHOW || state == PopUpWindState.SHOWING;
    }

    // конткент так же должен конкретизироваться в унаследованных классах
    public GameObject content { get { return _content; } set {
            _content = value;
            _content.transform.parent = transform;
            _content.transform.localPosition = _content.transform.localPosition + transform.localPosition;
        }
    }

    void Start () {
        var bc = transform.FindChild("closeBtn").GetComponent<BaseController>();
        bc.subscribeOnControllEvents(onCloseBtn);
        flyTo = transform.GetComponent<Flying>();
        flyTo.subscribe(onFlyTo);
        flyTo.destroyOnArrive = false;
        float s = HUD.isVisibleLeftRight ? 1.0f : 0.8f;
        transform.localScale = new Vector2(s, s);
    }
	
    public void show() // показать окно (начать анимацию)
    {
        ScenesController.updateGetActualInputLayer();
        gameObject.SetActive(true);
        if (state == PopUpWindState.HIDE || state == PopUpWindState.HIDING) {
            state = PopUpWindState.SHOWING;
            if (flyTo == null) Start();
            flyTo.init(showPosition, speedMove);
            WindowController.onWindow(this, WindowController.PopUpWindowEventType.PW_SHOW);
        }
    }
    public void hide() // сокрыть окно (начать анимацию)
    {
        //print("hide");
        if (state == PopUpWindState.SHOW || state == PopUpWindState.SHOWING) {
            state = PopUpWindState.HIDING;
            flyTo.init(hidePosition, speedMove);
            WindowController.onWindow(this, WindowController.PopUpWindowEventType.PW_HIDE);
        }
    }

    void onFlyTo(GameObject go) // при окончании анимации переключение в соответсвенное состояние
    {
        if (state == PopUpWindState.SHOWING) state = PopUpWindState.SHOW;
        else {
            state = PopUpWindState.HIDE;
            gameObject.SetActive(false);
        }
    }

    void onCloseBtn(BaseController btn, BaseController.TypeEvent type) {
        if (type == BaseController.TypeEvent.ON_MOUSE_CLICK) WindowController.hideCurrentWindow();
    }
}
