using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WarningWindow : MonoBehaviour {
    Button closeBtn;
    List< UnityEngine.Events.UnityAction> actions = new List<UnityEngine.Events.UnityAction>();

    void Awake(){
        closeBtn = transform.FindChild("exitBtn").GetComponent<Button>();
        MAIN.getMain.isShowWarningWindow = false;
    }

    public void toggleChanged(bool newValue){
        MAIN.getMain.isShowWarningWindow = !newValue;
    }

    public void onClose(){
        PlayerPrefs.SetInt("isShowWarningWindow", MAIN.getMain.isShowWarningWindow ? 1 : 0);
        Destroy(transform.parent.gameObject);
        Errors.warningWindow = null;
        ScenesController.updateGetActualInputLayer();
        for(int i = 0; i < actions.Count; i++) actions[i].Invoke();
    }
    public void setAction(int indexButton, UnityEngine.Events.UnityAction action) // ███ ДОБАВИТЬ действие указаной кнопке за индексом в масиве кнопок, по умолчанию каждая кнопка просто закрывает окно! Потому действия выполнятся после действия по умолчанию
    {
        //closeBtn.onClick.AddListener(action);
        actions.Add(action);
    }
}
