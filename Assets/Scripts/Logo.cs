using UnityEngine;
using System.Collections;
// Класс стартовой сцены при запуске приложения, тут происходит несколько простых анимаций, по таймеру переход к сцене загрузки приложения
public class Logo : MonoBehaviour {
    public float pauseDelay = 5.0f;
    float startTime;
    public GameObject loadingPrefab;

    SpriteRenderer line1;
    SpriteRenderer line2;
    SpriteRenderer morda;
    SpriteRenderer label;

    Flying flyingM;
    void Start () {
        ///////////////////  test  //////////////////////
         //Errors.onServerError(Api.ServerErrors.E_VERSION_ERROR);
         //return;
        /////////////////////////////////////////////////
        line1 = transform.FindChild("L1").GetComponent<SpriteRenderer>();
        line2 = transform.FindChild("L2").GetComponent<SpriteRenderer>();
        morda = transform.FindChild("morda").GetComponent<SpriteRenderer>();
        var flying1 = line1.gameObject.AddComponent<Flying>();
        var flying2 = line2.gameObject.AddComponent<Flying>();
        flyingM = morda.gameObject.AddComponent<Flying>();
        flying1.init(new Vector2(0.0f, flying1.transform.position.y),0.05f);
        flying2.init(new Vector2(0.0f, flying2.transform.position.y),0.05f);
        
        flying1.destroyOnArrive = false;
        flying2.destroyOnArrive = false;
        flyingM.destroyOnArrive = false;
        label = transform.FindChild("label").GetComponent<SpriteRenderer>();
        morda.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        label.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        startTime = Time.time;
        s = 0.5f - startFrom;
        SoundsSystem.play(Sound.S_LOGO);
    }

    float s;
    float startFrom = 0.2f;
    
    void Update () {
        ///////////////////  test  //////////////////////
         //return;
        /////////////////////////////////////////////////
        float currentDelay = Time.time - startTime;
        if (currentDelay > pauseDelay) {
            gameObject.SetActive(false);
            Instantiate(loadingPrefab);
        } else if (currentDelay > pauseDelay * startFrom) {
            flyingM.init(new Vector2(0.0f, 0.98f), 0.025f);
            float alpha = ((currentDelay + s * pauseDelay) - (pauseDelay * 0.5f)) / (currentDelay * 0.5f);
            morda.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            label.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        }
	}
}
