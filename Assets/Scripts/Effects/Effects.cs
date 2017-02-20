using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Класс контролирующий множество эффектов, в текущем Transform-e
// █ так же он позволяет навешать несколько эффектов одного и того же типа на один transform

public class Effects : MonoBehaviour{
    List<BaseEffect> listEffects = new List<BaseEffect>();  // █ список всех рабочих эффектов
    // получить класс эффекты (если его нет он создаётся)
    public static Effects getEffects(GameObject target) {   
        Effects e = target.GetComponent<Effects>();
        if (e != null) return e;
        return target.AddComponent<Effects>();
    }
    public static void onEffectDone(BaseEffect effect)  // по окончанию эффекта ( удаление из списка, как последней ссылки на объект, объект же в этом случае должен удалятся системой Unity )
    {
        if (effect.target == null){
            Debug.Log("[onCyclesFinish] target in SimpleWaveEffect is null");
            Errors.showTest("Если вы увидели эту ошибку то, ВОЗМОЖНО, задача MOB-256 исправлена если розыгрышь не зависнет сейчас");
            return;
        }
        var e = getEffects(effect.target.gameObject);
        if (e !=null) e.listEffects.Remove(effect);
    }
    public void addNewEffect(BaseEffect newEffect)      // добавить новый эффект к списку
    {
        listEffects.Add(newEffect);
    }
    // Получить список эффектов, прикреплённых к указанной цели
    public List<BaseEffect> getEffectsOnTarget() { return listEffects; }
    // получить список эффектов 
    public List<T> getEffectsOnTarget<T>() {
        List<T> res = new List<T>();
        for (int i = 0; i < listEffects.Count; i++)
            if (listEffects[i].GetType() == typeof(T))
                res.Add((T)(object)listEffects[i]);
        return res;
    }

    void Start () {}
    void Update () // Здесь на каждый тик, проходя по всему списку вызываются тики эффектов, (пустые удаляются)
    {
        for (int i = 0; i < listEffects.Count; i++)
            listEffects[i].tick();
        if (listEffects.Count == 0) Destroy(this);
    }
    /// ///////////////////////////////////////////
    /// список поддерживаемых эффектов:
    /// ///////////////////////////////////////////
    public static FadeEffect addFade(GameObject target, float playPeriod, bool out_ = true) // затухание указанного объекта
    {
        FadeEffect fe = new FadeEffect(target.transform, playPeriod, out_);
        var e = getEffects(target);
        e.addNewEffect(fe);
        return fe;
    } 
    public static SimpleWaveEffect addSimpleWave(Vector2 from, GameObject[] waving, SimpleWaveEffect.onWaveEvent callBack,float playPeriod, float radius, int countVaves = 1) // Волна указывается в периоде время существования и длина пути по которому она пройдёт за это время, позицию откуда она начнётся и объекты которые поддаются её влиянию, точнее в которых она вызовет указаное событие
    {
        var go = new GameObject("TempVaveEffect");
        go.transform.position = from;
        return addSimpleWave(go, waving, callBack, playPeriod, radius, countVaves, true);
    }
    public static SimpleWaveEffect addSimpleWave(GameObject target, GameObject[] waving, SimpleWaveEffect.onWaveEvent callBack,float playPeriod, float radius, int countVaves = 1, bool destroy = false) // аналогично указанной выше функции, только вместо позиции эпицентра волны указывает объект и последний параметр, нужно ли его удалять по окончанию эффекта
    {
        SimpleWaveEffect se = new SimpleWaveEffect(target.transform, waving, callBack, playPeriod, radius, countVaves, destroy);
        var e = getEffects(target);
        e.addNewEffect(se);
        return se;
    }
    public static FontaineCoins addFontaineCoins(GameObject from, GameObject to, int count, float fontainPower = 0.025f) // при повторных вызовах, проверяет на наличие уже существующего эффекта в указаном объекте (from) и создаёт либо новый фонтан монет либо добавляет к старому новое значение
    {
        if (from == null) {
            Errors.showTest("Невозможно создать фонтан монет, отсутствует источник!");
            return null;
        }
        var e = getEffects(from);
        var fontaineCoinsList = e.getEffectsOnTarget<FontaineCoins>();
        FontaineCoins fc = null;
        if (fontaineCoinsList.Count > 1 ){
            Errors.showTest("на одном источнике найдено несколько фонтанов монет!");
            return null;
        } else if (fontaineCoinsList.Count == 0) {
            fc = new FontaineCoins(from.transform, to.transform, count, fontainPower);
            e.addNewEffect(fc);
        } else {
            fc = fontaineCoinsList[0];
            fc.addToCount(count);
        }
        return fc;
    }
}
