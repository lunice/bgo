using UnityEngine;
using System.Collections;
// Кнопка нажимная плита (вглубь экрана)
public class PushedStonePlate : BaseController {
    float animTimePlay = 0.15f;         // время проигрования анимация
    public float defaultScale = 1.0f;   // матаб по умолчанию (до анимации маштабирования)
    float pressScale = 0.85f;           // задержка нажатия отжатия кнопки
    //public Sprite pressedBackground;    // (недоделано) установить другой фон кнопки, при её нажатии
    //public Color defaultColor;
    //public Color pressColor = new Color();

    float animTimePlayStart;            // начало проигрования анимации
    bool isPlayAnim = false;            // проигрывается ли анимация
    float diffScalse;                   // разница между дефотным матабом и тем к которому нужно проиграть анимацию
    SpriteRenderer sr;                  // рабочий SpriteRenderer текущего фона
    SpriteRenderer srP;                 // SpriteRenderer к цвету которого нужно привести рабочий, при проигровании анимации
    //float diffAlfa;

    protected override void Start() {
        base.Start();
        defaultScale *= transform.localScale.x;
        pressScale *= transform.localScale.x;
        diffScalse = defaultScale - pressScale;

        /*if ( pressedBackground != null ) {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            srP = go.AddComponent<SpriteRenderer>();
            srP.sprite = backGroundSprite;
            srP.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }*/
        sr = this.gameObject.GetComponent<SpriteRenderer>();
    }
    protected override void setState(ControllerState newState) {
        if (backGroundSR)
            switch (newState) {
                case ControllerState.ENABLE: { backGroundSR.color = Color.white;
                        //startAnim();
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
    public override bool onClick() {
        //MAIN.getMain.onButtonClick(this.name);
        return true;
    }
    public override bool onMouseDown(){
        //AudioSource.PlayClipAtPoint(MAIN.getMain.getResources().buttonPushSound, transform.position);
        SoundsSystem.play(Sound.S_BUTTON);
        return base.onMouseDown();
    }
    void startAnim() {
        if (!isPlayAnim) {
            isPlayAnim = true;
            animTimePlayStart = Time.time;
        } else {
            float lastTime = 1 - (Time.time - animTimePlayStart);
            animTimePlayStart = Time.time - lastTime;
        }
    }
    void stopAnim() {
        isPlayAnim = false;
        animTimePlayStart = Time.time;
    }
    protected override void playAnim() {
        if (!isPlayAnim || state == ControllerState.DISABLE)
            return;
        float passTime = Time.time - animTimePlayStart;
        if (passTime == 0) return;
        if (passTime >= animTimePlay)
            passTime = animTimePlay;

        float coef = passTime / animTimePlay;
        if (coef > 1.0f) coef = 1.0f;
        bool isUnpress = state == ControllerState.UNPRESSING;
        float newScale = diffScalse * coef;
        newScale = (isUnpress) ? pressScale + newScale : defaultScale - newScale;
        transform.localScale = new Vector3(newScale, newScale);
        if ( srP != null ) {
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
}
