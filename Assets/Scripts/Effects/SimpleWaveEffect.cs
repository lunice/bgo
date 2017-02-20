using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Эффект волна задаётся объект или позиция эпицентра, радиус и время прохождение по нему, список объектов и калбек который вызовется в них при прохождении волны через их позиции, количество проходящих волн, и нужно ли удалить епицентровый объект по окончанию эффекта(если он был задан)
public class SimpleWaveEffect : BaseEffect {
    // оповещения о прохождении волны через позиции подписаных объектов
    public delegate void onWaveEvent(GameObject waving);
    onWaveEvent m_callBack = null;
    List<GameObject> m_waving = new List<GameObject>();
    List<GameObject> tL;// = new List<GameObject>();
    bool destroyOnFinish;   // удалять ли таргет по окончанию работы эффекта
    float m_radius;         // радиус расспостранения
    float m_sqrRadius;      // квадрат радиуса расспостранения
    
    public SimpleWaveEffect(Transform target, GameObject[] waving, onWaveEvent callBack, float period, float radius, int cycles, bool destroy)  // Конструктор, в который передаются почти все необходимые параметры
        : base(target, period, cycles) 
    {
        m_callBack = callBack;
        for (int i = 0; i < waving.Length; i++) m_waving.Add(waving[i]);
        tL = m_waving;
        //m_waving = waving;
        m_radius = radius;
        m_sqrRadius = radius * radius;
        destroyOnFinish = destroy;

    }
    public override void play(float coef)   // страт волны
    {
        Vector2 pos = target.position;
        //float curDist = m_radius * coef;
        float curSqrtDist = m_sqrRadius * coef;
        for (int i = 0; i < tL.Count; i++){
            //Debug.Log(coef);
            if (tL[i] != null){
                Vector2 tPos = tL[i].transform.position;
                float difX = tPos.x - pos.x;
                float difY = tPos.y - pos.y;
                float d = difX * difX + difY * difY;
                if (curSqrtDist >= d){
                    m_callBack(tL[i]);
                    tL.Remove(tL[i]);
                }
            }
        }
        //base.play(coef);
    }
    public override void onPeriodFinish()   // окончание волны
    {
        base.onPeriodFinish();
        tL = m_waving;
    }
    public override void onCyclesFinish()   // окончание циклов волн
    {
        base.onCyclesFinish();
        if (destroyOnFinish) {
            if (target != null) GameObject.Destroy(target.gameObject);
            else Debug.Log("[onCyclesFinish] target in SimpleWaveEffect is null");
        }
    }
}