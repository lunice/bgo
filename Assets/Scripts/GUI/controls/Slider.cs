using UnityEngine;
using System.Collections;

public class Slider : BaseController {
    MAIN main = MAIN.getMain;
    //const string handleName = "Handle";

    public ControllerDirection direction;
    public Vector2 minMaxValues;
    public Vector2 multCoefMinMaxBorders = new Vector2(1.0f, 1.0f);
    public float curValue;
    Sprite handleSprite;
    //public Transform handle;
    float lastTimeUpdate;

    // elements:
    public Transform handle;
    public Transform fillingBar;
    Vector2 minMaxPos;
    float minPos, maxPos, coef, halfDifPos;

    protected override void Awake() {
        base.Awake();
        //handle = transform.FindChild(handleName);
        if ( handle != null ) {
            handleSprite = handle.GetComponent<SpriteRenderer>().sprite;
            calculateMinMaxBackGroundPos();
        } //else print("Error! [Awake] handle not defined!");
    }

    Vector2 fillingBarScaleCoef = new Vector2(1.0f,1.0f);
    void calculateMinMaxBackGroundPos() {
        print("[calculateMinMaxBackGroundPos]");
        Vector2 bgPos = backGround.localPosition;
        if (direction == ControllerDirection.HORIZONTAL) {
            float halfHandleSize = handleSprite.rect.width * 0.005f;
            print("backGroundTextureSize:" + backGroundTextureSize);
            float halfBackGroundSize = backGroundTextureSize.x * 0.5f;
            minPos = (-halfBackGroundSize + halfHandleSize) * transform.localScale.x * multCoefMinMaxBorders.x;
            maxPos = (halfBackGroundSize - halfHandleSize) * transform.localScale.x * multCoefMinMaxBorders.x;
            print("minPos:" + minPos + " maxPos:" + maxPos);
        } else if (direction == ControllerDirection.VERTICAL) {
            float halfHandleSize = handleSprite.texture.height * MAIN.coordSystemCoef * 0.5f;
            minPos = (- backGroundTextureSize.y * 0.5f + halfHandleSize) * transform.localScale.y * multCoefMinMaxBorders.y;
            maxPos = (backGroundTextureSize.y * 0.5f - halfHandleSize) * transform.localScale.y * multCoefMinMaxBorders.y;
        }
        coef = ( maxPos - minPos ) / ( minMaxValues.y - minMaxValues.x );
        halfDifPos = (maxPos - minPos) * 0.5f;

        if (fillingBar != null) {
            Sprite fSprite = fillingBar.GetComponent<SpriteRenderer>().sprite;
            //Vector2 fSpriteSize = new Vector2(fSprite.rect.width, fSprite.rect.height) * 0.01f;
            if (direction == ControllerDirection.HORIZONTAL)
                fillingBarScaleCoef = new Vector2(fSprite.rect.width / backGroundSR.sprite.rect.width, 1.0f);
            else
                fillingBarScaleCoef = new Vector2(1.0f,fSprite.rect.height / backGroundSR.sprite.rect.height);
        }

        //print("minPos:" + minPos + " ,maxPos:" + maxPos);
    }

    /*void setHandle(Sprite sprite) {
        handleSprite = sprite;
        SpriteRenderer sr;
        if (!handle) {
            handle = new GameObject().transform;
            //Instantiate(handle);
            //handle.name = handleName;
            handle.parent = this.transform;
            handle.localPosition = Vector2.zero;
            handle.localScale = transform.localScale;
            sr = handle.gameObject.AddComponent<SpriteRenderer>();
        }
        sr = handle.gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        calculateMinMaxBackGroundPos();
        setValue(main.timeDelayFilingBalls);
    }*/

    Vector3 _getMinMaxBoundsByHalfValue(Vector2 size, Vector3 pos, float value) {
        return new Vector3(size.x * value + pos.x,size.y * value + pos.y,0.0f);
    }

    protected override void Start () {
        updatePosHandle();
    }
	
    public void setValue(float newValue) {
        if (newValue > minMaxValues.y)
            newValue = minMaxValues.y;
        else if (newValue < minMaxValues.x)
            newValue = minMaxValues.x;
        else curValue = newValue;
        updatePosHandle();
    }

    void updatePosHandle() {
        if (!handle) return;
        //print(111 + " "+ curValue);
        float addPosToMin = (curValue - minMaxValues.x) * coef;
        //print(111 + " " + addPosToMin);
        if (direction == ControllerDirection.VERTICAL) {
            handle.localPosition = new Vector2(handle.localPosition.x, addPosToMin + minPos);
        } else if (direction == ControllerDirection.HORIZONTAL) {
            handle.localPosition = new Vector2(addPosToMin + minPos, handle.localPosition.y);
        } else print("Error![updatePosHandle] unknown ControllerDirection!");
    }

    float _GetCorrectNewPos(float newPos) {
        if (newPos < minPos)
            return minPos; 
        else if (newPos > maxPos )
            return maxPos;
        return newPos;
    }

    public override bool onMouseUp(bool tryClick = true) {
        if (state != ControllerState.DISABLE)
            state = ControllerState.ENABLE;
        return true;
    }

    void updateValueByCurrentPos() {
        float cP = 0;
        if (direction == ControllerDirection.HORIZONTAL)
            cP = handle.localPosition.x;
        else if (direction == ControllerDirection.VERTICAL)
            cP = handle.localPosition.y;
        float newPos = (cP + halfDifPos ) / coef;
        //print("handle.localPosition == " + handle.localPosition);
        //print("newPos == " + newPos);
        
        if (curValue != newPos) {
            float newFPos = curValue / (minMaxValues.y - minMaxValues.x);
            curValue = newPos + minMaxValues.x;
            //onChangeValue();
            main.onSliderValueUpdate(name, curValue);
        }
        
        if (fillingBar != null) {
            Vector2 newScale = 
            fillingBar.transform.localScale = fillingBarScaleCoef;
        }
    }

    //public void onChangeValue() {}

    protected override void onPress() {
        base.onPress();
        switch (direction){
            case ControllerDirection.HORIZONTAL: {
                    float mPosX = Input.GetAxisRaw("Mouse X") * MAIN.mouseCoef; //* (Time.time - lastTimeUpdate);
                    float newPos = _GetCorrectNewPos(handle.localPosition.x + mPosX);
                    //print("newPos:"+ newPos);
                    handle.localPosition = new Vector3(newPos, handle.localPosition.y, 0.0f);
                    //print("handle.localPosition:"+handle.localPosition);
                } break;
            case ControllerDirection.VERTICAL: {
                    float mPosY = Input.GetAxisRaw("Mouse Y") * MAIN.mouseCoef;
                    float newPos = _GetCorrectNewPos(handle.localPosition.y + mPosY);
                    handle.localPosition = new Vector2(handle.localPosition.x, Input.mousePosition.y + mPosY);
                } break;
                //default: print("Error! [Update] undefined controll direction:" + direction);
        }
        updateValueByCurrentPos();
        lastTimeUpdate = Time.time;
    }

    protected override void Update() {
        base.Update();
        /*Camera camera = Camera.main;
        if (camera) { 
            Debug.DrawLine(new Vector3(1.0f,1.0f,0.0f),//camera.transform.position - new Vector3(Screen.width / 2, Screen.height / 2, 0),// вывод линии на экран 
                       new Vector3(100.0f, 100.0f, 0.0f),Color.white);//camera.transform.position + new Vector3(Screen.width / 2, Screen.height / 2, 0));
            print("succes draw line!");
        }
        else print("camera == null");*/
    }
}
