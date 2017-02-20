using UnityEngine;
using System.Collections;
// Класс настройки, отвечает за окно настроек в игре
public class Settings : PopUpWindow {
    // Инициализация, и привязка всех кнопок к соответственным игровым модулям
    void Start () {
        // проигрования музыки
        CheckButton musickCB = transform.FindChild("gameMusicBtn").GetComponent<CheckButton>();
        musickCB.value = !SoundsSystem.musikOn;
        musickCB.subscribeOnControllEvents(SoundsSystem.onCheckBoxValueChange);
        // проигрование звуков
        CheckButton soundsCB = transform.FindChild("gameSoundBtn").GetComponent<CheckButton>();
        soundsCB.value = !SoundsSystem.soundOn;
        soundsCB.subscribeOnControllEvents(SoundsSystem.onCheckBoxValueChange);
        // проигрования звуков шаров
        CheckButton ballsSoundsCB = transform.FindChild("ballsSoundBtn").GetComponent<CheckButton>();
        ballsSoundsCB.value = !SoundsSystem.ballsSoundOn;
        ballsSoundsCB.subscribeOnControllEvents(SoundsSystem.onCheckBoxValueChange);
        // ползунок скорости подачи шаров
        Slider speedBallsSlider = transform.FindChild("speedBallsSlider").GetComponent<Slider>();
        speedBallsSlider.setValue(MAIN.getMain.timeDelayFilingBalls);
        // Кнопка закрытия окна. (Красный крестик) в его вверхнем правом углу
        SButton closeBtn = transform.FindChild("closeBtn").GetComponent<SButton>();
        if (closeBtn != null) closeBtn.subscribeOnControllEvents(onCloseClick);
        else print("Error! closeBtn != null");
        // █ Логика для тестирования! Всё что ниже в релиз не должно попасть
        var debugT = transform.Find("debug");
        //var sessionBtn = transform.Find("sessionBtn");
        var textGO = GameObject.Find("sessionID");
        if (MAIN.IS_TEST) { 
            debugT.GetComponent<BaseController>().subscribeOnControllEvents(testPrint);
            //sessionBtn.GetComponent<BaseController>().subscribeOnControllEvents(showSessionID);
            textGO.GetComponent<UnityEngine.UI.Text>().text = "Session ID: " + MAIN.getMain.sessionID;
        } else {
            debugT.gameObject.SetActive(false);
            //sessionBtn.gameObject.SetActive(false);
            textGO.SetActive(false);
        }
    }
    /*void showSessionID(BaseController btn, BaseController.TypeEvent typeEvent) {
        if (typeEvent == BaseController.TypeEvent.ON_MOUSE_CLICK){
            var sessionIdGO = GameObject.Find("sessionID");
            if ( sessionIdGO == null) {
                
            }
        }
    }*/
    // Событие нажатии тестовой кнопки. Только при тестовом режиме, отображается кнопка выведение логов на экран.
    void testPrint(BaseController btn, BaseController.TypeEvent typeEvent){
        if (typeEvent == BaseController.TypeEvent.ON_MOUSE_CLICK){
            GameSystem.showHideConsole();
            //var raf = MAIN.getMain.raffle;
            //raf.testPrint();
            //raf.setState(RaffleState.FINISH);
        }
    }
    // При нажатии на кнопку закрытия данного окна. (Красный крестик) в его вверхнем правом углу
    void onCloseClick(BaseController btn, BaseController.TypeEvent typeEvent){
        //MAIN.getMain.money.setValue(5);
        if (typeEvent == BaseController.TypeEvent.ON_MOUSE_CLICK){
            WindowController.hideCurrentWindow();
        }
    }
}
