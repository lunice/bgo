using UnityEngine;
using System.Collections;
// Базовый класс эффектов, который позволяет работать в общей системе класса Effects
public class BaseEffect{
    public Transform target = null; // Цель к которой привязывается эффект
    float m_startTime;              // время начала проигрования
    float m_playPeriod;             // полный цикл проигрования эффекта (если это мигание, от полного загорания до затухания, если маштабирование, от увелечения до уменьшения, и т.п.)
    int m_limitCycles;              // Количество проигрываемых циклов
    // конструктов, в ктором инициализация всех основных прараметров, и далее запуск эффекта, фиксация времени начала проигрования
    public BaseEffect(Transform target_, float period, int cycles = -1){
        m_startTime = Time.time;
        m_playPeriod = period;
        m_limitCycles = cycles;
        target = target_;
    }
    bool unscalingTime = false;     // █ (не использовалось, не используется и не проверялось работоспособность) Если нужно, что бы эффект, проигрывался не зависимо от искажения игрового времени, установите эту переменную в значение TRUE
    //public void setUnscalingTime(bool val = true) { } // для доступа к переменной выше
    // описание события, на окончание эффекта
    public delegate void OnEffectDone(BaseEffect effect);
    protected OnEffectDone callBackOnDone = null;
    public void subscribeOnEffectDone(OnEffectDone newCallBack) { callBackOnDone = newCallBack; }
    // Вызывается из Update() в Unity, для обновления эффектов
    public virtual void tick(){
        float dif = Time.time - m_startTime;
        if (unscalingTime) dif *= (1.0f / Time.timeScale);
        if (dif < m_playPeriod) play(dif / m_playPeriod);
        else {
            play(1.0f);
            onPeriodFinish();
        }
    }
    //////////////////////////////////////////////////////////////
    // осоновые функции для переопеределения

    // начало проигрованния
    public virtual void play(float coef) { }
    // на окончания периода, (либо заканчивается работа эффекта, либо следующий цыкл)
    public virtual void onPeriodFinish() {
        if (m_limitCycles-- > 1) m_startTime = Time.time;
        else onCyclesFinish();
    }
    // на окончания цыклов или по остановке, удаления эффекта
    public virtual void onCyclesFinish() {
        if (callBackOnDone != null) callBackOnDone(this);
        Effects.onEffectDone(this);
    }
    // остановка
    public virtual void stop(bool andWithOutCallback = true) {
        if (andWithOutCallback) callBackOnDone = null;
        onCyclesFinish();
    }
}