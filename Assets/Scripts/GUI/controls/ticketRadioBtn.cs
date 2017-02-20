using UnityEngine;
using System.Collections;
// класс радио кноппка, для выбора количества билетов
public class ticketRadioBtn : MonoBehaviour {
    bool IAmSelected;           // выбранная ли
    float shiftValue = 0.06f;   // значение смещения по вертикале при выборе

	void Start ()
    {
        DigitsLabel label = transform.Find("Label").GetComponent<DigitsLabel>();
        label.setValue( int.Parse(name) );
        RadioButtons rb = transform.parent.GetComponent<RadioButtons>();
        var selectedButton = rb.getSelectedButton();
        IAmSelected = selectedButton != null && gameObject == rb.getSelectedButton().gameObject;
        rb.subscribeOnRadioBtnSelected(OnNewSelected);
        if (IAmSelected) {
            select(true);
            Rooms.countTickets = int.Parse(this.gameObject.GetComponent<BaseController>().name);
        }
    }

    public void select(bool val) {
        float shift = val ? -shiftValue : shiftValue;
        DigitsLabel label = transform.Find("Label").GetComponent<DigitsLabel>();
        var p = label.transform.localPosition;
        label.transform.localPosition = new Vector3(p.x, p.y + shift, p.z);
    }

    void OnNewSelected( BaseController btn ){
        if (IAmSelected) {
            if (btn.gameObject != this.gameObject) select(false);
        } else if (!IAmSelected && btn.gameObject == this.gameObject) {
            select(true);
            Rooms.countTickets = int.Parse(btn.name);
            // print(MAIN.getMain.countTickets);
        }
        IAmSelected = btn.gameObject == this.gameObject;
    }
}
