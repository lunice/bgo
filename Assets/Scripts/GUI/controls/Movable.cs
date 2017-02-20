using UnityEngine;
using System.Collections;

public class Movable : BaseController {
    public bool enableX = true;
    public bool enableY = true;
    bool isMoving = false;
    Vector2 diffPress;
    public string targetName = "";
    public Transform target = null; // если сюда ничего не указать двигающим объектом будет экземпляр этого класса
    float mouseCoef = 0.02f;
    //void Start () {}
    protected override void Awake()
    {
        base.Awake();
        if (target == null) { 
            if (targetName == "") target = transform;
            else {
                var t = GameObject.Find(targetName);
                if (t != null) target = t.transform;
                else target = transform;
            }
        }
    }

    public override bool onMouseDown()
    {
        diffPress = target.position - Input.mousePosition * mouseCoef;
        return base.onMouseDown();
    }

    protected override void onPress()
    {
        base.onPress();
        if ( enableX || enableY ) {
            target.position = new Vector2(
                //enableX ? Input.GetAxisRaw("Mouse X") * MAIN.mouseCoef : 0,
                //enableY ? Input.GetAxisRaw("Mouse Y") * MAIN.mouseCoef : 0);
                enableX ? Input.mousePosition.x * mouseCoef + diffPress.x : target.position.x,
                enableY ? Input.mousePosition.y * mouseCoef + diffPress.y : target.position.y);
        }
    }
    protected override void Update()
    {
        base.Update();
    }

}
