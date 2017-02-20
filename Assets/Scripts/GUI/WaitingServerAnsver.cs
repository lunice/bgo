using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
// класс ожидания ответа от сервера, отображающий шестирёнки, и блокирующий любые нажатия клиента
public class WaitingServerAnsver : MonoBehaviour {
	public RectTransform gear1; // большая шестерёнка
    public RectTransform gear2; // маленькая шестерёнка
    public float rotateGear1;   // скорость вращение больш. шестр.
    public float rotateGear2;   // скорость вращение малой  шестр.
    static float showThroughTime = 1.0f;    // █ плавность/длительность появления

    float defaultBackgroundAlpha;   // значение альфы к которой затемняется фон (берется из префаба)
    void Start () {
        defaultBackgroundAlpha = this.GetComponent<Image>().color.a; // запоминается из префаба
    }
	void Update () {
        if (countRequests == 0) return;
        // для плавного появления при начале отображения
        if (Time.time - callShowTime <= showThroughTime)
            setAlpha( (Time.time - callShowTime) / showThroughTime );
        // вращение шестерёнок
        gear1.Rotate(new Vector3(0.0f, 0.0f, rotateGear1));
        gear2.Rotate(new Vector3(0.0f, 0.0f, rotateGear2));
    }
    
    //Dictionary<string, float> defaultAlpha;
    // установка прозрачности, для фона И шестерёнок
    void setAlpha(float alpha) {
        Image backGround = GetComponent<Image>();
        backGround.color = new Color(0.0f, 0.0f, 0.0f, defaultBackgroundAlpha * alpha);
        for(int i=0;i<transform.childCount;i++){
            var img = transform.GetChild(i).GetComponent<Image>();
            if (img != null) img.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        }
    }
    public static Object waitingServerAnsverPrefab = null; // префаб фрейма ожидания ответа от сервера
    public static WaitingServerAnsver waitingServerAnsver = null; // сам фрейм ожидания
    // отображается ли
    public static bool isShowing() { return waitingServerAnsver.gameObject.activeInHierarchy; }
    // █ в проекте все запросы должны быть последовательны, но счётчик ниже поддерживает и паралельное их выполнение, а так же выдаёт ошибку в режиме тестирования сообщая о том что есть паралельные 
    static int countRequests = 0;   // █ количество запросов использовалось для контроля количества одновременных запросов и ответов, при появении новых запросов, до ответа предведущих, счётчик увеличивает своё значение, при ответе уменьшает, когда он равен нулю, окно прячется
    static float callShowTime = 0;  // фиксация времени когда начался показ фрейма, (для вычисления прошедшего времени)
    public int prevInputLayer = 0;  // █ предведущий слой/маска тачей запоминается для востановления предведущего слоя после сокрытия фрейма
    // показать фрейм
    public static void show(string requestType = "") {
        if (countRequests < 0){
            Errors.showTest("Предупреждение счётчик ожиданий ответов от сервера меньше нуля!");
            countRequests = 0;
        }
        countRequests++; // увеличиваем счётчик запросов от клиента
        if (waitingServerAnsver == null) {
            GameObject waitingServerAnsverGO = GameObject.Find("WaitingServerAnsver");
            if (waitingServerAnsverGO == null) { 
                if (waitingServerAnsverPrefab == null) 
                    waitingServerAnsverPrefab = RESOURCES.getPrefab("WaitingServerAnsver");
                waitingServerAnsverGO = GameObject.Instantiate(waitingServerAnsverPrefab) as GameObject;
                waitingServerAnsverGO.name = "WaitingServerAnsver";
                DontDestroyOnLoad(waitingServerAnsverGO);
            }
            waitingServerAnsver = waitingServerAnsverGO.GetComponent<WaitingServerAnsver>();
            waitingServerAnsver.gameObject.SetActive(false);
        }
        
        if (callShowTime == 0) callShowTime = Time.time;
        /*if (throughTime > 0 && showThroughTime == 0) {
            showThroughTime = throughTime;
            callShowTime = Time.time;
            return;
        }*/
        waitingServerAnsver.gameObject.SetActive(true);
        // █ изменение слоя
        /*if (MAIN.getMain.actualInputLayer != 0) { 
            waitingServerAnsver.prevInputLayer = MAIN.getMain.actualInputLayer;
            MAIN.getMain.actualInputLayer = 0;
        }*/
        ScenesController.updateGetActualInputLayer();
        // █ проверка на нарушение последовательсноти запросов
        if (countRequests > 1) Errors.showTest("Количество запросов больше 1!(второй запрос:" + requestType + "). сообщите разработчикам. Приложение должно подвиснуть");
    }
    // спрятать фрейм
    public static void hide(bool all = false){
        if (waitingServerAnsver != null) {
            if (!waitingServerAnsver.gameObject.activeSelf)
                callShowTime = 0;
            else if (countRequests-- <= 1 || all) {
                waitingServerAnsver.gameObject.SetActive(false);
                callShowTime = 0;
                //MAIN.getMain.actualInputLayer = waitingServerAnsver.prevInputLayer;
                ScenesController.updateGetActualInputLayer();
            }
        } else print("warning waitingServerAnsver frame already hided");
    }
}
