using UnityEngine;
using System.Collections;
// █ некоторые из параметров данного эфекта, должны устанавливаться только из префабов
// Класс был описан специально для челюсти дракона, он осуществляет ряд изменений в основном в классе Transform, а именно меняет угол положения в пространстве и положение, так же может менять маштаб, и предполагалось сделать изменение цвета
// все анимации установленные в класс могут иметь промежуточные значения (состояния), количество которых так же задаются в этот еффект, и равномерно разбиваются на протяжении всей анимации. Например. установленое число 5. Породит 5 позиций приоткрытия рта дракона
public class AnimationStates : MonoBehaviour {
    struct animSet {
        //public animSet() {}
        public animSet(Vector2 difPos, float difRotate, Color difColor) {
            m_difPos = difPos;
            m_difRotate = difRotate;
            m_difColor = difColor;
        }
        public Vector2 m_difPos;
        public float m_difRotate;
        public Color m_difColor;
    }
    // события смены состояния анимации
    public delegate void animStateEvent(GameObject go, float value);
    protected animStateEvent callBack;
    public void subscribe(animStateEvent newCallBack) { callBack = newCallBack; }

    float m_animCurrentProgress = 0.0f; // текущее состояние анимации от состояния х до состояния у
    float m_nextProgress = 0.0f;    // вычисленная граничная велечина прогресса перехода в следующее состояние

    public Vector2 startPos;        // начальная позиция обхъекта
    public Vector2 finishPos;       // конечная позиция объекта
    public float startRotate = 0;   // стартовый угол поворота
    public float finishRotate = 0;  // конечный угол поворота
    public Color startColor;        // стартовая скорость
    public Color finishColor;       // конечная скорость
    public float speedAnim;         // скорость проигрования анимации
    // из выше перечисленных начальных и конечных параметров, ниже, указывается количество состояний, по которым разбиваются равноемерные, промежуточные состояния от начального до конечного
    public int countStates = 5; // ███ #V количество приоткрытий челюсти на дополнительные шары должно соответствовать максимальному количеству дополнительных шаров на один розыгрышь
    public int curState = 0;    // текущее(начальное состояние)
    

    animSet globalDif;
    SpriteRenderer m_spriteRenderer = null;

    public static AnimationStates set(GameObject target, float newState, float speedAnim = 0.01f){
        AnimationStates animStates = target.GetComponent<AnimationStates>();
        if (animStates == null){
            animStates = target.AddComponent<AnimationStates>();
        }
        animStates.setNewState(newState);
        animStates.speedAnim = speedAnim;
        return animStates;
    }

    void Start () {
        globalDif = new animSet();
        if (startPos != null && finishPos != null ) globalDif.m_difPos = finishPos - startPos;
        globalDif.m_difRotate = finishRotate - startRotate;
        globalDif.m_difColor = finishColor - startColor;
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        //m_nextProgress = 1.0f;
        //print("globalDif.m_difPos == " + globalDif.m_difPos);
        //print("globalDif.m_difRotate == " + globalDif.m_difRotate);
        ///print("globalDif.m_difColor == " + globalDif.m_difColor);
    }
	//void Update () {}
    void FixedUpdate(){
        if (m_animCurrentProgress != m_nextProgress) {
            float val = Time.deltaTime * speedAnim;
            if (m_animCurrentProgress < m_nextProgress) {
                m_animCurrentProgress += val;
                if (m_animCurrentProgress > m_nextProgress) onAnimFinish();
            } else {
                m_animCurrentProgress -= val;
                if (m_animCurrentProgress < m_nextProgress) onAnimFinish();
            }
            //print(m_animCurrentProgress);
            updateValues(m_animCurrentProgress);
        }
    }
    void onAnimFinish() // окончание анимации от состояния х до состояни y и передаётся в событие новое текущее состояние y
    {
        m_animCurrentProgress = m_nextProgress;
        if (callBack != null) callBack(this.gameObject, m_animCurrentProgress);
    }
    
    void updateValues(float coef)   // проигрывает текущую анимацию от положения(состояния) х до положения(состояния) у
    {
        if (globalDif.m_difPos != null)
            transform.localPosition = startPos + globalDif.m_difPos * coef;
        if (globalDif.m_difRotate != 0) {
            //transform.localRotation.Set( 0.0f, 0.0f, startRotate + globalDif.m_difRotate * coef, 1.0f);
            //transform.localRotation.z = startRotate + globalDif.m_difRotate * coef;
            transform.eulerAngles = new Vector3(0.0f, 0.0f, startRotate + globalDif.m_difRotate * coef);
        }
        if (globalDif.m_difColor != null && m_spriteRenderer != null && globalDif.m_difColor != new Color(0.0f, 0.0f, 0.0f, 0.0f) ) { 
            m_spriteRenderer.color = startColor + globalDif.m_difColor * coef;
        }
    }

    public void setNextState() // установить следующее состояние (возможно уже последнее) опираясь на внутренний каунтер состояний
    {
        m_nextProgress = (float)++curState / countStates;
        /*print("█ curState:" + curState);
        print("█ m_nextProgress:" + m_nextProgress);
        print("█ m_animCurrentProgress:" + m_animCurrentProgress);
        print("█ countStates:" + countStates);*/
    }
    public void setLastState() // установить последнее состояние. (проиграть анимацию до последнего состояния
    { m_nextProgress = 1.0f; curState = countStates; }
    public void setNewState(int newState) { m_nextProgress = countStates / countStates; }
    public void setNewState(float newState){ m_nextProgress = newState; }
    public void setFirstState(bool immediately = false) // установить первое состояние, немедленно - true, фактически сбрасывает анимацию в начало
    {
        m_nextProgress = 0.0f;
        curState = 0;
            if (immediately) {
            m_animCurrentProgress = 0.0f;
            updateValues(0.0f);
        }
    }
}
