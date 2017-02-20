using UnityEngine;
using System.Collections;

public class ChangeColor : BaseEffect
{
    Color toColor;
    Color nativeColor;
    SpriteRenderer sr;
    bool isSmoothness;
    public static ChangeColor set(Transform target, Color onColor, float period, bool smoothness = true, int countCycles = -1) {
        ChangeColor cce = new ChangeColor(target, onColor, period, smoothness, countCycles);
        var e = Effects.getEffects(target.gameObject);
        e.addNewEffect(cce);
        return cce;
    }

    public ChangeColor(Transform target, Color onColor, float period, bool smoothness = true, int countCycles = -1) : base(target, period, countCycles)
    {
        sr = target.GetComponent<SpriteRenderer>();
        toColor = onColor;
        isSmoothness = smoothness;
        if (sr != null){
            nativeColor = sr.color;
            difColor = onColor - sr.color;
        }
    }

    Color difColor;
    public override void play(float coef)
    {
        base.play(coef);
        if (sr == null) return;
        if (isSmoothness)  coef = ((coef < 0.5f ? coef : (0.5f - (coef - 0.5f))) * 2);
        else coef = coef < 0.5f ? 1.0f : 0.0f;
        sr.color = nativeColor + difColor * coef;
        
    }

}
