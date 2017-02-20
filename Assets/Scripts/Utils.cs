using UnityEngine;
using System.Collections.Generic;
// Класс объединяющий функции общего назначения
public static class Utils {
    public static void screenShot(string fileName) // Делает скрин шот
    {
        if (MAIN.IS_TEST){
            Application.CaptureScreenshot(fileName);
        }
    }
    //============================[ математические ]=====================================
    // просто делает рандом от -v до +v в переменных Vector2
    public static Vector2 rand(float v) { return new Vector2(Random.Range(-v, v), Random.Range(-v, v)); }

    //=========================================================================
    // Находит кнопку, в указанной цели, или среди его детей
    public static BaseController findBaseControllIn(Transform t)
    {
        //print("Try find base controll in: "+t.name);
        BaseController bc = t.GetComponent<BaseController>();
        if (bc != null) return bc;
        int cCount = t.childCount;
        if (cCount == 0) return null;
        for (int i = 0; i < cCount; i++)
        {
            bc = findBaseControllIn(t.GetChild(i));
            if (bc != null) return bc;
        }
        return null;
    }

    //=====================[работа с SpriteRenderer в иерархиях]==================================
    // функционал внизу позволяет устанавливать order во всей иерархии объектов
    public static void increaseOrder(Transform in_, int increaseOn){
        SpriteRenderer sr = in_.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder += increaseOn;
        for (int i = 0; i < in_.transform.childCount; i++) increaseOrder(in_.transform.GetChild(i), increaseOn);
    }

    // функционал внизу позволяет устанавливать цвет во всей иерархии объектов
    public static Dictionary<SpriteRenderer, Color> getSpriteRendererColor(Transform t){
        spriteRenderColor = new Dictionary<SpriteRenderer, Color>();
        findAllSR(t);
        return spriteRenderColor;
    }
    static Dictionary<SpriteRenderer, Color> spriteRenderColor;
    static void findAllSR(Transform in_){
        SpriteRenderer sr = in_.GetComponent<SpriteRenderer>();
        if (sr != null) spriteRenderColor.Add(sr, sr.color);
        for (int i = 0; i < in_.transform.childCount; i++) findAllSR(in_.transform.GetChild(i));
    }
    // █ МОЖНО ИСПОЛЬЗОВАТЬ ТОЛЬКО если она будет вызвана один раз
    public static void setAlpha(float alpha, Transform t){ 
        setAlpha( alpha, getSpriteRendererColor(t));
    }
    public static void setAlpha(float alpha, Dictionary<SpriteRenderer, Color> src) {
        spriteRenderColor = src;
        setAlpha_(alpha);
    }
    static void setAlpha_(float alpha){
        foreach (var key in spriteRenderColor.Keys){
            Color col = spriteRenderColor[key];
            key.color = new Color(col.r,col.g,col.b,col.a*alpha);
        }
    }

    //=================================================================
}
