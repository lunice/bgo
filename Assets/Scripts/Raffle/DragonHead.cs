using UnityEngine;
using System.Collections;
// Класс отвечающий за декоративную голову дракона в розыгрыше. Открытие её челюсти и в седствии этого высыпания шаров. Так как открывается физический колайдер что их сдерживает
public class DragonHead : MonoBehaviour {
    static DragonHead m_dragonHead; // тут хранится сама голова, ниже реализация доступа на подобье синглтона
    public static DragonHead dragonHead{
        get {
            if (m_dragonHead == null ) { 
                m_dragonHead = GameObject.Find("DecorHead").GetComponent<DragonHead>();
                m_dragonHead.init();
            }
            return m_dragonHead;
        }
    }
    public AnimationStates jaw;     // "Позиционная анимация" челюсти дракона
    public AnimationStates eye;     // "Позиционная анимация" глаза дракона 
    public EdgeCollider2D collider; // сдерживающий шары колайдер

    public static bool isFullOpen() { return !dragonHead.collider.enabled; } // открыта ли полностью челюсть?

    public static void openMore() // ███ приоткрыть челюсть сильнее ( с каждым докупленным дополнительным шаром, позиции приоткрывания равномерно разбиваются от максимально доступного количества шаров, указанных в MAIN ) потому менняя их количество на сервере нужно всегда менять их и там
    {
        dragonHead.jaw.setNextState();
        Flickering.set(dragonHead.eye.gameObject, 0.35f, 1); // V# мигание глаза при докупке шара
        SoundsSystem.play(Sound.S_DRAGON, dragonHead.transform.position);
    }
    public static void close(bool immeadiatly = false) // закрытие пасти (сделано только для мгновенного закрытия, вне видимости пользователя)
    {
        dragonHead.jaw.setFirstState(immeadiatly);
        dragonHead.collider.enabled = true;
    }
    public static void openFull() // открыть челюсть полностью ( шары начнут сыпаться )
    {
        dragonHead.jaw.setLastState();
        Flickering.set(dragonHead.eye.gameObject, 0.5f, 11); // V# мигание глаза при выплёвывании шаров
        SoundsSystem.play(Sound.S_DRAGON_FULL, dragonHead.transform.position);
    }

    bool isInit = false;
    void init() {
        jaw = transform.FindChild("Jaw").GetComponent<AnimationStates>();
        jaw.subscribe(onJawAnimDone);
        eye = transform.FindChild("Eye").GetComponent<AnimationStates>();
        collider = GetComponent<EdgeCollider2D>();
        isInit = true;
    }
    void Start () {
        init();
    }
	void Update () {}
    void onJawAnimDone(GameObject go, float val) {
        if (val == 1.0f) {
            collider.enabled = false;
        }
    }
}
