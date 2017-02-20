using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// █ Базовый класс всех тач-реагирующих объектов (кнопок, ползунков, чек-боксов, радиобатанов, и т.п.)
public class BaseController : MonoBehaviour {
    public enum ControllerDirection     // (█ не проверялось) Данное перечисление актуально только для ползунка и радиобатанов...
    { 
        HORIZONTAL,     // горизонтальное положение
        VERTICAL        // вертикальное положение
    }
    public enum ControllerState         // состояние контроллера (состояния не для всех видов)
    {
        DISABLE,    // отключен
        ENABLE,     // включен (и в состоянии отжат)
        PRESSING,   // нажимается (процесс анимированого нажатия)
        PRESS,      // нажат
        UNPRESSING  // отжимается (процесс анимированого отжатия)
    }
    public const string backGroundName = "BackGround";
    protected ControllerState state = ControllerState.ENABLE; // █ состояние контрола
    public bool unPressOnMouseLeave;    // █ отжимать ли кнопку, при покидании курсора/пальца, области реагирования кнопки 
    public bool enableClickWhenMouseLeave = false; // █ засчитывать ли отжатие кнопки если отпуск был вне кнопки
    Sprite backGroundSprite;            // фоновая картинка контрола
    protected SpriteRenderer backGroundSR; //SpriteRenderer фоновой картинки контрола
    protected Transform backGround;     // transform фоновой картинки контрола
    protected Vector2 backGroundTextureSize; // размер фоновой картинки контрола
    //int ignorelayerMask = 1 << 8;     // реакция под указаные маски
    public enum TypeEvent // тип обрабатываемого события 
    {
        ON_MOUSE_UP,                    // кнопка отжалась
        ON_MOUSE_DOWN,                  // кнопка нажалась
        ON_MOUSE_CLICK                  // кнопка нажалась и отжалась
    }
    public delegate void ControllEvent( BaseController btn, TypeEvent typeEvent ); // описание событие кнопки (несёт в себе кнопку и тип события)
    protected List<ControllEvent> callBacks = new List<ControllEvent>(); // Список калбеков, который при нажатиях на контролл, разсылает соответственные события всем подписчикам
    public void subscribeOnControllEvents( ControllEvent newCallBack ) // подписаться на события контролла
    {
        callBacks.Add(newCallBack);
    }
    public void unSubscribeOnControllEvents(ControllEvent callBack ) // отписаться от события контролла
    {
        if (callBacks.Contains(callBack)) callBacks.Remove(callBack);
    }

    protected virtual void Awake() // инициализация картинок при старте, поиск ресурсов из разных источников
    {
        backGround = transform.FindChild(backGroundName);
        if (!backGround && backGroundSprite) {
            setBackGround(backGroundSprite);
        } else { 
            backGroundSR = GetComponent<SpriteRenderer>();
            if (backGroundSR) {
                backGroundSprite = backGroundSR.sprite;
                backGround = backGroundSR.transform;
                backGroundTextureSize = new Vector2(backGroundSprite.rect.width, backGroundSprite.rect.height) * MAIN.coordSystemCoef;
            } //else print("Error! [Awake] backGround not defined!");
        }
    }
    protected void setBackGround(Sprite sprite) // установка фоновой картинки
    {
        backGroundSprite = sprite;
        if (!backGround) {
            backGround = new GameObject().transform;
            //Instantiate(backGround);
            backGround.name = backGroundName;
            backGround.parent = this.transform;
            backGround.localPosition = Vector2.zero;
            backGround.localScale = transform.localScale;
            GameObject goBG = backGround.gameObject;
            backGroundSR = goBG.AddComponent<SpriteRenderer>();
        }
        backGroundSR.sprite = sprite;
        backGroundTextureSize = new Vector2(sprite.rect.width * MAIN.coordSystemCoef, sprite.rect.height * MAIN.coordSystemCoef);
        Vector3 v3 = new Vector3(backGroundTextureSize.x, backGroundTextureSize.y, 0.0f);

        BoxCollider2D collider = backGround.gameObject.GetComponent<BoxCollider2D>();
        if (collider) {
            DestroyImmediate(collider);
        }
        collider = backGround.gameObject.AddComponent<BoxCollider2D>();
        //print("backGroundTextureSize == " + v3);
    }
    protected virtual void Start () {}
    public void setEnable(bool val = true) // включение / отключение контрола
    {
        //print("[setEnable] val == " + val);
        if ( val ) { 
            if (state == ControllerState.DISABLE) setState(ControllerState.ENABLE);
        } else { 
            if (state != ControllerState.DISABLE) setState(ControllerState.DISABLE);
        }
    }
    protected virtual void setState(ControllerState newState) // изменение состояния
    { 
        state = newState;
    }
    public virtual bool onMouseDown() // на нажатие, на котнол...
    {
        /*if (GameInput.getObjectUnderMouse() != this.gameObject)
            return false;*/
        //print("[onMouseDown] controll name:" + this.name);
        if (state != ControllerState.DISABLE) {
            setState(ControllerState.PRESSING);
            //if (callBack != null) callBack(this, TypeEvent.ON_MOUSE_DOWN);
            for (int i = 0; i < callBacks.Count; i++) callBacks[i](this, TypeEvent.ON_MOUSE_DOWN);
            return true;
        }
        return false;
    }
    /*GameObject getObjectUnderMouse() // Если есть что-то что нужно перетаскивать {
        //RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        print(" "+backGround.gameObject.layer);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 11.0f,MAIN.getMain.actualInputLayer);
        //Physics2D.ra
        if (hit) {
            print(" hit!");
            return hit.collider.gameObject;
        }
        return null;
    }*/
    public virtual bool onMouseUp(bool tryClick = true) // на отжатие, контрола...
    {
        if (state == ControllerState.PRESS || state == ControllerState.PRESSING) {
            setState(ControllerState.UNPRESSING);
            if (tryClick || enableClickWhenMouseLeave /*&& GameInput.getObjectUnderMouse() == this.gameObject*/) {
                //if (callBack != null) callBack(this, TypeEvent.ON_MOUSE_CLICK);
                for (int i = 0; i < callBacks.Count; i++) { 
                    callBacks[i](this, TypeEvent.ON_MOUSE_UP);
                    if (tryClick) callBacks[i](this, TypeEvent.ON_MOUSE_CLICK);
                }
                onClick();
                return true;
            }
        }
        return false;
    }
    protected virtual void playAnim() // проигрывается анимация нажатия/отжатия
    {
        if (state == ControllerState.PRESSING) setState(ControllerState.PRESS);
        else setState(ControllerState.ENABLE);
    }
    protected virtual void onPress() {}
    public virtual bool onClick() { return true; }
    protected virtual void Update(){
        switch (state) {
            case ControllerState.PRESSING: playAnim(); break;
            case ControllerState.UNPRESSING: playAnim(); break;
            case ControllerState.PRESS: {
                    if (unPressOnMouseLeave && GameInput.getObjectUnderMouse() != this.gameObject ) {
                        //if (callBack != null) callBack(this, TypeEvent.ON_MOUSE_UP);
                        for (int i = 0; i < callBacks.Count; i++) callBacks[i](this, TypeEvent.ON_MOUSE_UP);
                        onMouseUp(false);
                    } else onPress(); } break;
        }
    }
}
