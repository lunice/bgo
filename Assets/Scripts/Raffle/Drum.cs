using UnityEngine;
using System.Collections;
// Класс отвечающий за работу барабана, практически полностью декоративный. Он притормаживает начало розыгрыша, поскольку должне разогнаться до некой номинальной скорости. После чего начнут сыпаться шары
public class Drum : MonoBehaviour {
    public enum State {
        NONE = 0,   // не инициализирован, какая-то ошибка (не обрабабатывается, но и быть такого не может)
        START,      // разгон барабна (шары ожидают)
        ROLL,       // вращения барабана с его номинальной скоростью (шары начинают сыпаться)
        STOPING,    // барабан останавливается (свободные шары(30 шт) закончились сыпатсья)
        STOP        // барабан полностью остановился)
        //DRUG,
    }
    public float speed = 0.0f;                  // текущая скорость вращения
    public State state = State.NONE;            // текущиее состояние
    public float startAccseleration = 0.1f;     // ускорение при разгоне
    public float stopAccseleration = 0.001f;    // торможение в конце розыгрыша

    // подписка на события барабана, это интересно розыгрышу, от его стартового состояния он начинает высыпать шары
    public delegate void OnChangeState(State state);
    private OnChangeState callBack;
    public void subsribeOnDrumState(OnChangeState newCallBack) { callBack = newCallBack; }

    MAIN main = MAIN.getMain;
    Light light;                    // освещение
    public float minLight = 0.5f;   // минимальное освещение, в состоянии покоя
    public float maxLight = 1.3f;   // максимальное освещение, при максимальной/номинальной скорости вращения барабана
    void Start () {
        //rigidbody2D
        //speed = 100.0f / MAIN.getMain.timeDelayFilingBalls;
        light = GameObject.Find("drugLight").GetComponent<Light>();
    }

    const float nominalCoef = 40.0f;    // коэфициент для расчёта вращения барабана. █ Также зависит от скорости подачиш шаров!
    void Update () // тут вся его работа
    {
        switch ( state ) {
            case State.NONE: return;// break;
            case State.START:
                float nominal = nominalCoef / main.timeDelayFilingBalls;
                if (speed >= nominal){
                    state = State.ROLL;
                    speed = nominal;
                    if (callBack != null) callBack(state);
                }else{
                    light.intensity = maxLight * speed / nominal;
                    if (light.intensity < minLight) light.intensity = minLight;
                    speed += (startAccseleration / main.timeDelayFilingBalls);
                }
                    break;
            case State.ROLL: { }  break;
            case State.STOPING : if (speed <= 0) state = State.STOP;
                else {
                    speed *= 1 - stopAccseleration;
                    light.intensity = maxLight * speed / (nominalCoef / main.timeDelayFilingBalls);
                    if (light.intensity < minLight) light.intensity = minLight;
                }
                break;
            case State.STOP: return;
        }
        transform.Rotate(new Vector3(0f, 0f, speed * Time.deltaTime));
    }
}
