using UnityEngine;
using System.Collections;

public class SButton : BaseController {
    //SpriteRenderer backGround;
    MAIN main = MAIN.getMain;

    protected override void setState(ControllerState newState) {
        if (backGroundSR)
            switch (newState) {
                case ControllerState.ENABLE: { backGroundSR.color = Color.white; } break;
                case ControllerState.DISABLE: { backGroundSR.color = Color.grey; } break;
                case ControllerState.PRESS: { backGroundSR.color = Color.green; } break;
            }
        else print("Error! [setState] backGround not defined!");
        base.setState( newState );
    }

    public override bool onClick() {

        //main.onButtonClick(this.name);
        return true;
    }
}
