using UnityEngine;
using System.Collections;
// Класс работающий с системным временем, его, плавное замедление, и восстановление
public class GameSystem : MonoBehaviour {
    float targetTimeScale;              // маштаб времени к которому должно прийти системное
    float delayChangeTimeScale = 0.1f;  // время плавного перехода (█ искажается самим же временем)
    float startChangeTime = -1;         // -1 (означает отключено) иначе это точка старта отчёта времени, с которого происходит плавное изменение времени
    public TestConsole testConsole;

    void Awake() {
        testConsole = GetComponent<TestConsole>(); //gameObject.AddComponent<TestConsole>();
    }
    void Start () { targetTimeScale = Time.timeScale; }
    public void setGameTimeSpeed(float newTimeScale, float delay) // установка нового маштаба, с указанным промежутком (плавностью перехода)
    {
        targetTimeScale = newTimeScale;
        delayChangeTimeScale = 0.1f;
        startChangeTime = Time.time;
    }

    public static void showHideConsole() {
        var go = GameObject.Find("System");
        if (go!=null) {
            TestConsole tc = go.GetComponent<GameSystem>().testConsole;
            tc.ShowHideConsole();
        }
    }

    public void restoreTime() // █ мгновенное востановление, пока только мгновенное
    {
        startChangeTime = -1;
        Time.timeScale = 1.0f;
    }

    void FixedUpdate() {
        if (startChangeTime < 0) return;
        
        float dif = Time.time - startChangeTime;
        if ( dif < delayChangeTimeScale) {
            Time.timeScale = 1.0f / targetTimeScale * (dif / delayChangeTimeScale);
        } else {
            Time.timeScale = targetTimeScale;
            startChangeTime = -1;
        }
    }
}
