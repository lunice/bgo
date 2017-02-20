using UnityEngine;
using System.Collections;
// █ эффект - полёт. Используется при полётах монет, звёзд, всплывающих окон и нажимающихся вниз кнопок
// █ принцип работы довольно прост. Берётся позиция старта и позиция конца движения, вычисляется разница (вектор движения) и добавляется указанная скорость по этому вектору █но!
// так как у объекта есть собственная инерция, движения, то вычисления вектора движения к цели постоянно обновляется, а скорость к цели возрастает на некий постоянный кофициент. Таким образом одна скорость преобладает над другой и объекты всегда достигают цели... (но апсолютно не известно через какое время и по какой траэктории)
public class Flying : MonoBehaviour {
    // █ два вида подписи, упрощённая, где возвращается сам летящий объект и усложнённая, где передаётся ещё и цель к которой он летел (нужно было для билетов, что бы знать какомму именно билетку пренадлежит клетка к которому прилетел шар) по номеру шара узнать было невозможно
    public delegate void OnGameObjectArrived(GameObject obj);
    public delegate void OnGameObjectArrivedToTarget(GameObject obj, GameObject target);
    protected OnGameObjectArrived callbackFct;
    protected OnGameObjectArrivedToTarget callbackFctWithTarget;
    public void subscribe(OnGameObjectArrived NewCallBackFunction) { callbackFct = NewCallBackFunction;}
    public void subscribe(OnGameObjectArrivedToTarget NewCallBackFunction, GameObject target, GameObject fromGO = null) {
        callbackFctWithTarget = NewCallBackFunction;
        _target = target;
        _fromGO = fromGO;
    }
    GameObject _target; // цель к которой нужно прилететь
    GameObject _fromGO; // цель от которой начинается полёт
    public Vector2 fFrom;   // поцизия от которой начинается полёт
    public Vector2 fTo;     // позиция к которой нужно прилететь █ (обновляется от указанного таргета или нет уже не помню...)
    public bool destroyOnArrive = true; // уничтожать ли объект по прибитыию
    public bool trackTarget = false; // отслеживание цели, при наличии таргета, обновляет его местоположение, и, при его движении летит на обновлённую позицию
    public GameObject getStartFlyFromGameObject() { return _fromGO; } 

    float slowdownAtDist = 0, slowdownToSpeed, slowdownBrakingRate; // усложнённые параметры, для звёзд - притормаживание перед прибытием, с какой дистанции начиать торможение, к какой скорости и с какой силой тормозить
    public void slowdown(float atDist, float toSpeed, float brakingRate) // инициализация усложнённого полёта
    {
        slowdownAtDist = atDist; slowdownToSpeed = toSpeed; slowdownBrakingRate = brakingRate;
    }
    float fSpeed;       // текущая скорость
    Vector2 fVelocity;  // текущая скорость
    bool isInit = false;
    public void init(Vector2 flyTo, float speed){ init(flyTo, speed, Vector2.zero); } 
    public void init(Vector2 flyTo, float speed, Vector2 velocity){
        fFrom = transform.localPosition;
        fTo = flyTo;
        fSpeed = speed;
        fVelocity = velocity;
        isInit = true;
        updateAddS();
    }

    Vector2 addS; // прибавляемое значение скорости, на новом тике
    void Start () { }
	
    void updateAddS() // █ здесь вычисляется вектор движения к цели, и новая позиция на каждый тик
    {
        if (trackTarget && _target != null) fTo = _target.transform.position;
        addS = fTo - fFrom;
        float aX = Mathf.Abs(addS.x);
        float aY = Mathf.Abs(addS.y);
        if (aY < aX) aY = aX;
        addS = fSpeed * addS / aY;
    }

	void FixedUpdate () 
    {
        if (!isInit) return;
        // здесь проверяется прибыл ли объект к цели или ещё нет
        Vector3 v = transform.localPosition;
        float dx = fTo.x - v.x;
        float dy = fTo.y - v.y;
        float leftDist = dx * dx + dy * dy; // квадрат расстояния к цели
        bool isStop = fSpeed > leftDist;    // █ проверка на прибитие (минимальное расстояние считается расстоянием которое преодолевается за один тик, т.е. значение скорости)
        if (isStop) {           // остановка
            if (callbackFct != null) callbackFct(gameObject);
            else if (callbackFctWithTarget != null) callbackFctWithTarget(this.gameObject, _target);
            if (destroyOnArrive) Destroy(this.gameObject);
            else {
                isInit = false;
                transform.localPosition = fTo;
            }
            return;
        }
        // █ объект не прибыл устанавливаются новые позиции
        transform.localPosition = new Vector3(v.x + addS.x + fVelocity.x, v.y + addS.y + fVelocity.y, 0.0f );
        ///fVelocity *= 0.999f;
        // расчёт позиций на следующий тик ( для следующей проверки )
        fFrom = v;
        fSpeed *= 1.005f;   // █ возграстание скорости к цели
        // если указана дистанция с которой нужно начать тормозить, начинают пропорционально гасится скоростя
        if (slowdownAtDist > 0 && leftDist < slowdownAtDist && slowdownToSpeed < fSpeed){
            //print("█");
            fSpeed *= slowdownBrakingRate;
            fVelocity *= slowdownBrakingRate;
        } 
        updateAddS();   // вычисление нового направления
    }
}
