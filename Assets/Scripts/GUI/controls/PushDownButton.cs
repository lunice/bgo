using UnityEngine;
using System.Collections;
// Кнопка каменная плита, нажимающаяся вниз. Это все кнопки на нижнем меню
// у них расширеный функционал их анимации
public class PushDownButton : BaseController {

    float animTimePlay = 0.15f;
    public float defaultScale = 1.0f;
    float pressScale = 0.85f;
    public Sprite pressedBackground;
    public Vector2 ShowPos;
    //public Color defaultColor;
    //public Color pressColor = new Color();

    float animTimePlayStart;
    bool isPlayAnim = false;
    float diffScalse;
    SpriteRenderer sr;
    SpriteRenderer srP;
    //Vector3 startPos;
    //float diffAlfa;
    //Vector2 hideShift();

    protected override void Start()
    {
        base.Start();
        defaultScale *= transform.localScale.x;
        pressScale *= transform.localScale.x;
        diffScalse = defaultScale - pressScale;
        sr = this.gameObject.GetComponent<SpriteRenderer>();
    }

    protected override void setState(ControllerState newState){
        //print(newState);
        if (backGroundSR)
            switch (newState) {
                case ControllerState.ENABLE: { backGroundSR.color = Color.white;
                        //onClick();
                    } break;
                case ControllerState.DISABLE: { backGroundSR.color = Color.gray;
                        stopAnim();
                    } break;
                //case ControllerState.PRESS: { backGroundSR.color = Color.green; } break;
                case ControllerState.PRESSING: { startAnim(); } break;
                case ControllerState.UNPRESSING: { startAnim(); } break;
            }
        else print("Error! [setState] backGround not defined!");
        base.setState(newState);
    }

    public override bool onMouseDown() {
        SoundsSystem.play(Sound.S_BUTTON, transform.position);
        return base.onMouseDown();
    }

    void stopAnim() {
        isPlayAnim = false;
        animTimePlayStart = Time.time;
    }

    public override bool onClick()
    {
        //MAIN.getMain.onButtonClick(this.name);
        return true;
    }

    void startAnim()
    {
        if (!isPlayAnim) {
            isPlayAnim = true;
            animTimePlayStart = Time.time;
        }
        else
        {
            float lastTime = 1 - (Time.time - animTimePlayStart);
            animTimePlayStart = Time.time - lastTime;
        }
    }

    protected override void playAnim() // переопределение анимации нажимания внутрь, на нажимание вниз
    {
        if (!isPlayAnim )
            return;
        float passTime = Time.time - animTimePlayStart;
        if (passTime == 0) return;
        if (passTime >= animTimePlay)
            passTime = animTimePlay;

        float coef = passTime / animTimePlay;
        bool isUnpress = state == ControllerState.UNPRESSING;
        float newScale = diffScalse * coef;
        newScale = (isUnpress) ? pressScale + newScale : defaultScale - newScale;
        transform.localScale = new Vector3(newScale, newScale);
        //transform.localPosition.Set(startLocalPos.x,startLocalPos.y + newScale,0.0f);
        if (srP != null)
        {
            print(coef);
            coef = isUnpress ? coef : -coef;
            srP.color = new Color(1.0f, 1.0f, 1.0f, coef);
            sr.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - coef);
        }

        if (passTime == animTimePlay) {
            setState(state == ControllerState.PRESSING ? ControllerState.PRESS : ControllerState.ENABLE);
            isPlayAnim = false;
        }
    }

    //=======================[hide/show system]=========================

    
    void initFly(Vector2 to) {
        Flying flying = this.gameObject.GetComponent<Flying>();
        if (flying == null) { 
            flying = this.gameObject.AddComponent<Flying>();
            flying.subscribe(onButtonArrived);
            flying.destroyOnArrive = false;
        }
        flying.init(to, 0.03f);
    }
    void onButtonArrived(GameObject btn) {
        //print("[onButtonArrived] curPos =" + transform.position);
    }
    private Vector2 shiftForHidePos = new Vector2(0.0f,-1.7f);
    public void show() {
        //if (!this.gameObject.activeSelf) {
        initFly(ShowPos);
        //}
    }
    public void show(Vector2 onPos, bool rewriteStartPos = false) {
        if (rewriteStartPos) ShowPos = onPos;
        initFly(onPos);
    }
    public void hide() {
        initFly((Vector2)ShowPos + shiftForHidePos );
    }
}
