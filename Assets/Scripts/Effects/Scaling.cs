using UnityEngine;
using System.Collections;
// Эффект маштабирования устанавливает маштаб до указанного размера и возвращает обратно, в указаных рамках времени. С возможностью цикличного повторения
public class Scaling : MonoBehaviour {
    //public Vector3 scalingValue;
    public float scalingPeriod = 1.0f;
    public int scalingLimitCycles = -1;
    public bool destroyOnFinish = true;
    bool WithOutReturedAnim = false;
    public bool isUnScalingTime = false;

    public static Scaling set(GameObject target, Vector3 value, float period, int limitCycles = -1){
        Scaling scaling = target.AddComponent<Scaling>();
        //scaling.scalingValue = value;
        scaling.scalingPeriod = period;
        scaling.scalingLimitCycles = limitCycles;
        scaling.defaultScale = target.transform.localScale;
        scaling.difValue = value - scaling.defaultScale;
        return scaling;
    }
    public static Scaling set(GameObject target, float value, float period, int limitCycles = -1) {
        return set(target, target.transform.localScale * value, period, limitCycles);
    }

    float startScalingTime = -1;
    Vector3 defaultScale;
    Vector3 difValue;

    public delegate void ScalingFinish(GameObject target);
    protected ScalingFinish m_callBack = null;
    public void subscribeOnScalingFinish(ScalingFinish callBack) { m_callBack = callBack; }
    public void setWithOutReturedAnim(bool val = true){ WithOutReturedAnim = val; }

    void Start () { startScalingTime = Time.time; }
	void Update () {
        float dif = (Time.time - startScalingTime);
        if (isUnScalingTime) dif *= (1.0f / Time.timeScale);
        if (startScalingTime > 0 && dif < scalingPeriod){
            float coef = dif / scalingPeriod;
            setScale((coef < 0.5f || WithOutReturedAnim ? coef : (0.5f - (coef - 0.5f))) * 2);
            
        } else onFinishPeriod();
    }
    void setScale(float coef) {
        transform.localScale = defaultScale + difValue * coef;
    }
    public void stopScaling(bool immediately = false) {
        scalingLimitCycles = 0;
        if (immediately) onFinishPeriod();
    }
    public void onFinishPeriod() {
        startScalingTime = Time.time;
        if (scalingLimitCycles != -1)
            if ( scalingLimitCycles-- <= 1 ) { 
                if (!WithOutReturedAnim) setScale(0.0f);
                if (m_callBack != null) m_callBack(gameObject);
                if (destroyOnFinish) Destroy(this);
                else startScalingTime = -1;
            }
    }
}
