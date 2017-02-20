using UnityEngine;
using System.Collections;
// Класс отвечающи за сцену покупки билетов
public class BuyTickets : MonoBehaviour {
    DigitsLabel costTicketsLabel;       // лейбел отображения цены за билеты
    //float startTimeBeforeShowTutorial;  // задержка перед отображением обучающих сообщений
	
	void Start () // внутри происходит инициализация радиобутонов
    {
        print("Rooms.currentRoom.TicketMin:" + Rooms.currentRoom.TicketMin);
        print("Rooms.currentRoom.TicketMax:" + Rooms.currentRoom.TicketMax);
        var t = transform.Find("BuyTicketsWindow").transform;
        var rb = t.Find("CountTicketsButtons").GetComponent<RadioButtons>();
        costTicketsLabel = t.Find("CostLabel").GetComponent<DigitsLabel>(); 
        rb.subscribeOnRadioBtnSelected(OnNewCountTicketsSelected);
        var buyTicketsBtn = GameObject.Find("buyTicketsBtn").GetComponent<BaseController>();
        var costLabel = GameObject.Find("CostLabel").GetComponent<BaseController>();
        buyTicketsBtn.subscribeOnControllEvents(onBuyTicketsBtnEvent);
        costLabel.subscribeOnControllEvents(onBuyTicketsBtnEvent);
        /*if ( Tutorial.wasShowed(TutorialSubject.TS_CHOOSE_COUNT_TICKETS) ) {
            rb.selectBtn(3); // выбираем по умолчанию 4-й элемент
        }*/
    }
    void onBuyTicketsBtnEvent(BaseController btn, BaseController.TypeEvent e) // событие на нажатие кнопки радиобутона
    {
        var t = transform.Find("BuyTicketsWindow").transform;
        var rb = t.Find("CountTicketsButtons").GetComponent<RadioButtons>();
        if ( e == BaseController.TypeEvent.ON_MOUSE_CLICK) {
            if (rb.getSelectedButton() == null) {
                Errors.show("Выбирете количество билетов");
                return;
            }
            int numButton = int.Parse(rb.getSelectedButton().name);
            //print(rb.getSelectedButton().name);
            if (MAIN.getMain.money.getValue() >= numButton * Rooms.currentRoom.TicketPrice) {
                /*if (numButton == 4)*/ ScenesController.loadScene(GameScene.RAFFLE);
                //else Errors.show("Извините в этой версии пока доступно только 4 билета", "ну ладно");
            } else HUD.playAnimNeedMoreMoney();
        }
    }
    void OnNewCountTicketsSelected(BaseController btn) // установка цены в зависимости от выбранной кнопки
    {
        costTicketsLabel.setValue( int.Parse(btn.name ) * Rooms.currentRoom.TicketPrice );
    }

    void OnEnable() // установка задержки для обучения 
    {
        //startTimeBeforeShowTutorial = Time.time;
    }
    void Update ()
    {
	}
}
