using UnityEngine;
using System.Collections;
// Сцена загрузки приложения
public class Loading : MonoBehaviour {
    Transform dragon;               // сам дркон
    public int loadLevelNum = 0;    // номер загружаемой сцены
    float porcentValue;             // процесс загрузки 0..100

    void Start () // инициализация загрузки. Здесь используется Unity система, которая позволяет асинхронно загружать ресурсы указанной сцены, при этом из неё можно вытащить прогресс загрузки... Что и сделано в функции StartCoroutine("loadGame") ниже
    {
        dragon = transform.FindChild("Dragon");
        for (int i = 0; i < dragon.childCount;i++)
            dragon.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        porcentValue = dragon.childCount / 100;
        StartCoroutine("loadGame");
	}

    IEnumerator loadGame() // события от загрузчика
    {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadLevelNum);
        while (async.isDone == false) {
            setProgress(async.progress * porcentValue);
            yield return true;
        }
    }

    int currentProgress = 0; // текущий рогресс
    void setProgress(float newProgress) // визуальное отображения прогресса загрузки на драконе
    {
        int newP = (int)newProgress;
        SpriteRenderer sr;
        do {
            sr = dragon.GetChild(currentProgress).GetComponent<SpriteRenderer>();
            sr.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        } while (currentProgress++ < newP);
        sr = dragon.GetChild(currentProgress).GetComponent<SpriteRenderer>();
        sr.color = new Color(1.0f, 1.0f, 1.0f, newProgress - newP );
    }
}
