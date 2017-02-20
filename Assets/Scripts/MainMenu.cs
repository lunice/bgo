using UnityEngine;
using System.Collections;
// Класс отвечающий за лобби, и кнопки на нём
public class MainMenu : MonoBehaviour {
    // Поиск кнопок и подписка на них
    void Start () {
        var playBtn = GameObject.Find("playBtn").GetComponent<BaseController>();
        var tutorialBtn = GameObject.Find("tutorialBtn").GetComponent<BaseController>();
        

        if (!MAIN.isTutorialEnable)
            tutorialBtn.gameObject.SetActive(false);

        playBtn.subscribeOnControllEvents(onPlayBtnEvent);          
        tutorialBtn.subscribeOnControllEvents(onTutorialRestart);
        alignAllChilds();
    }
    void update(){
        //alignAllChilds();
    }
    void alignAllChilds() // выравнивание кнопок в зависимости от пропорций экрана
    {
        float xCoef = HUD.getProportionScreen();
        for (int i=0;i<transform.childCount; i++) {
            var btn = transform.GetChild(i);
            btn.position = new Vector2(btn.position.x * xCoef, btn.position.y);
        }
    }
    // на этот калбэк подписываются кнопки ведущие в румы 
    void onPlayBtnEvent(BaseController bt, BaseController.TypeEvent e) {
        if (e == BaseController.TypeEvent.ON_MOUSE_CLICK) {
            if ( Rooms.chooseRoom( bt.name ) )
                ScenesController.loadScene(GameScene.BUY_TICKETS);
            //ScenesController.hideModule("MainMenu");
        }
    }
    // при нажатию на кнопку "обучение", перезапускает туториал
    void onTutorialRestart(BaseController bt, BaseController.TypeEvent e) {
        if (e == BaseController.TypeEvent.ON_MOUSE_CLICK) Tutorial.restart();
    }
    // для туториала ( если туториал не был показан, показывает обучающее сообщение )
    void OnEnable() { Tutorial.show(TutorialSubject.TS_ENTER_IN_ROOM); }
}
