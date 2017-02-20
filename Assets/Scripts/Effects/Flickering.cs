using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Класс можно значительно уменьшить внеся его в единую систему эффектов
// Эффект мигание
public class Flickering : MonoBehaviour {
    public float periodFlickering = 1.0f;   // период мигания
    float startTimeFlickering = -1.0f;      // фиксация времени с начала мигания (периода)
    public int m_limitCycles = -1;          // количество циклов которые эффект отработает и удалиться из объекта (-1 обозначает бессконечно)
    //SpriteRenderer[] flickeringObjects;
    Dictionary<SpriteRenderer,Color> flickeringObjectsD = new Dictionary<SpriteRenderer, Color>(); // █ Во всех дочерних объектах которые имеют SpriteRenderer, заносятся в этот список, и вних происходит подмена стандартного шейдера на способный "мигать"
    // (недоделано) мигание пока только белым цветом ниже подготовка для задания разного цвета
    Color _color;                           
    Color _difColor;                        
    public Color flickeringColor{           
        get { return _color; }
        set { _color = value; }
    }
    BaseController myBaseController = null; // сюда задаётся контрол, по нажатию на который произойдёт прекращение мигания, и отписание от событий этой кнопки
    // установка мигания по указаной цели, с параметрами: период мигания, количество циклов (-1 бессконечно)
    public static Flickering set(GameObject target, float period, int limitCycles = -1) {
        var res = set(target, period, Color.white);
        res.m_limitCycles = limitCycles;
        return res;
    }
    public static Flickering set( GameObject target, float period, Color color ) {
        Flickering flickering = target.GetComponent<Flickering>();
        if (flickering != null) flickering.stopFlickering(true);
        else flickering = target.AddComponent<Flickering>();
        flickering.periodFlickering = period;
        flickering.flickeringColor = color;
        return flickering;
    }
    // остановка мигания, immediately по умолчанию, обозначает, что доотработает последний цикл мигания
    public static void stop(GameObject target, bool immediately = false) {
        var f = target.GetComponent<Flickering>();
        if (f != null) f.stopFlickering(immediately);
        else Debug.Log("█ Warning! can not STOPING! Flickering == null in target:"+ target.name);
    }
    
    // вызвав эту функцию, эффект сам попытается найти контрол и подписаться на него
    // (две функции возможно не уместны в этом классе, но так было удобно)
    public bool setFlickeringUntilPress() {
        var myBaseController = Utils.findBaseControllIn(this.transform);
        if (myBaseController == null) {
            if (MAIN.IS_TEST) Errors.showTest("[setFlickeringUntilPress] В мигающем объекте BaseController не найден");
            return false;
        }
        myBaseController.subscribeOnControllEvents(onClick);
        //isFlickeringUntilPress = true;
        return true;
    }
    // на нажатие остановка
    void onClick(BaseController bt, BaseController.TypeEvent e) {
        if (e == BaseController.TypeEvent.ON_MOUSE_CLICK) stopFlickering(true);
    }
    
    Shader nativeShader; // █ родной шейдер, помещается сюда, перед замещеннием его на способный мигать
    // Старт мигания
    public void startFlickering() {
        setFlickeringShader();
        startTimeFlickering = Time.time;
    }
    // █ Установка самого шейдера, по указанному объекту, и по всем дочерним, имеющим SpriteRenderer
    bool setFlickeringShader(bool unset = false) {
        foreach (var key in flickeringObjectsD.Keys) {
            if (key != null) { 
                Shader shader = Resources.Load<Shader>("Shaders/flickering2");
                if (shader == null) {
                    Errors.showTest("шейдер: Shaders/flickering не найден...");
                    return false;
                }
                if (unset) key.material.shader = nativeShader;
                else { 
                    nativeShader = key.material.shader;
                    key.material.shader = shader;
                }
            } //else flickeringObjectsD.Remove(key);
        }
        return true;
    }
    bool stoping = false; // █ эта переменная возможно лишняя, что мигание должно остановится но не немедленно, а отработаный цикл будет последним, можно было бы просто установить m_limitCycles = 1
    // остановка мигания
    public void stopFlickering(bool immediately = false) {
        stoping = true;
        if (immediately) onFinishPeriod();
    }
    // По окончанию периода. Проверка на повторный период или окончания работы эффекта
    void onFinishPeriod() {
        //print("█ limitCycles:"+m_limitCycles);
        if (m_limitCycles != -1) m_limitCycles--;
        if (!stoping && m_limitCycles != 0 ) startTimeFlickering = Time.time;
        else {
            setFlickeringColorByCoef(1.0f);
            setFlickeringShader(true);
            if (myBaseController != null) myBaseController.unSubscribeOnControllEvents(onClick);
            Destroy(this);
        }
    }
    // █ здесь и устанавливается значение свечения объекта, в зависимости от коэфициэнта ( 0.0f - 1.0f )
    void setFlickeringColorByCoef(float coef) {
        //print("█ set coef:" + coef);
        SpriteRenderer needRemove = null;
        foreach (var key in flickeringObjectsD.Keys) {
            float c = 0.75f + 0.35f * coef;//(0.5f + coef * 0.25f);
            Color col = flickeringObjectsD[key];
            if (key != null)
                key.color = new Color(col.r * c, col.g * c, col.b * c);
            else needRemove = key;
        }
        if (needRemove != null) flickeringObjectsD.Remove(needRemove);
    }
    // при старте сразу получаем список объектов имеющих spriteRenderer для подмены шейдера
    void Awake() {
        flickeringObjectsD = Utils.getSpriteRendererColor(transform);
    }
	void Start () { startFlickering(); /*print("█ startFlickering...");*/ }
    // здесь происходят вычисления по времени, коэфициента мигания, на текущий период, и проерка на его истичение (периода)
	void Update () {
        if (startTimeFlickering < 0) return;
        float dif = Time.time - startTimeFlickering;
        if (dif < periodFlickering){
            float coef = dif / periodFlickering;
            setFlickeringColorByCoef( ( coef < 0.5f ? coef : (0.5f - (coef - 0.5f)) ) * 2 );
        } else onFinishPeriod();
	}
}
