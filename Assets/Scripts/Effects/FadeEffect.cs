using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Эффект затухания. Задаёт альфу от 1 до 0 
// █ если объект полупрозрачен, сработает правильно
public class FadeEffect : BaseEffect{
    bool isHiding;   // в процессе ли сокрытия
    Dictionary<SpriteRenderer, Color> spriteRenderColors; // если в объекте есть дети и какие-то из них имеют SpriteRenderer, состояния их параметров занесутстся в этот словарь, и коректно сокроются вместе с родителем
    // в конструкторе задаются все необходимые параметры и начинается проигрование
    public FadeEffect(Transform target, float period, bool isHide) : base(target, period, 1){
        spriteRenderColors = Utils.getSpriteRendererColor(target);
        isHiding = isHide;
    }
    // начало проигрования, запускается из базового класса
    public override void play(float coef){
        //base.play(coef);
        Utils.setAlpha(isHiding ? 1.0f - coef : coef, spriteRenderColors);
    }
}
