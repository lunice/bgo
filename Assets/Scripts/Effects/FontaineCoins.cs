using UnityEngine;
using System.Collections;
// Эффект, фонтанирования монет, но можно приспособить и к любым другим объектам
public class FontaineCoins : BaseEffect{
    int totalCount = 0;     // общее количество монет
    Transform toTarget;     // цель к которой направлен фонтан
    int moneyInOneCoin = 1; // значение(номинал) одной монеты
    float fontainPower;     // (сила фонтана) скорость полёта каждой монеты, включает в себя управление двух переменных: начальная случайная в разные стороны скорость монеты, и прогрессирующая в сторону цели
    GameObject coinPref;    // префаб монеты
    DigitsLabel dlFrom;     // цифровой лебел(от цели), если указан, будет изменяться на вечену номинала монеты
    DigitsLabel dlTo;       // цифровой лебел(к цели), если указан, будет изменяться на вечену номинала монеты
    Vector2 shiftFromPos;   // смещение позиции генерации монет (смещение добавляется к цели от которой летят монеты)
    //int delyanCoef = 50;  // делящий коэфициент
    // установить номинал монеты
    public void setNominalCoin(int nominalValue) { moneyInOneCoin = nominalValue; }
    // █ Конструктор, в котором частичная инициализация фонтана и запуск монет
    public FontaineCoins(Transform from, Transform to, int count, float fontainPower_ = 0.025f, float period = 0.005f) : base(from, period, -1){
        toTarget = to;
        fontainPower = fontainPower_;
        coinPref = MAIN.getMain.getResources().coinPrefab;
        addToCount(count);
        dlFrom = from.GetComponent<DigitsLabel>();
        dlTo = to.GetComponent<DigitsLabel>();
    }
    // сместить позициию генерации объектов фонтана на:
    public void shiftStartPosOn(Vector2 shift) { shiftFromPos = shift; }
    // добавить к общему ЗНАЧЕНИЮ монет(НЕ количеству монет), которое нужно отфонтанить.
    public void addToCount(int count) { setTotalCount(count + totalCount); }
    // установить текущее ЗНАЧЕНИЕ
    public void setTotalCount(int count){
        totalCount = count;
        if (moneyInOneCoin == 0) moneyInOneCoin = 1;
    }
    // переопределённый базовый тик необходим для порождения монет 
    // █ пока происходит практически на каждый тик, а потому интенсивность высыпания монет, напрямую зависит от мощьности устройтва
    public override void tick(){
        if (totalCount > 0){
            emitCoin();
        };
    }
    // получить общее ЗНАЧЕНИЕ монет (НЕ самих монет), которое нужно отфонтанить
    public int getTotalCount() { return totalCount; }
    // Порождение монетки
    void emitCoin() {
        int _moneyInOneCoin = moneyInOneCoin;
        if (totalCount < _moneyInOneCoin) _moneyInOneCoin = totalCount;
        totalCount -= _moneyInOneCoin;
        //Debug.Log("coin#" + MAIN.testCountCoins + " moneyInOneCoin: "+ _moneyInOneCoin);
        if (dlFrom != null) dlFrom.addValue(-_moneyInOneCoin);
        GameObject coin = Object.Instantiate(coinPref);
        coin.GetComponent<Coin>().value = _moneyInOneCoin;
        Flying f = coin.GetComponent<Flying>();
        //Flickering fl = Flickering.set(coin, 0.03f);
        f.transform.position = new Vector2(target.position.x, target.position.y) + shiftFromPos + Utils.rand(0.03f);
        //Animation anim = coin.GetComponent<Animation>().clip();
        f.init(toTarget.position, fontainPower, new Vector2(
            Random.Range(-fontainPower * 2.3f, fontainPower * 2.3f),
            Random.Range(-fontainPower, fontainPower)));
        f.subscribe(onCoinArrive); // подписываемся на прибитие
        Rotating.set(coin, Random.Range(-1.0f, 1.0f));
    }
    // Событие по прибитию монетки к конечной цели (подпись в функции выше). Издаётся звук, удаляется монетка
    // ███ !!! необходима оптимизация, создания пула монет, в котором они не будут удалятся, а прятатся и вторично использоваться, нежели каждый раз удаляться и создаваться...
    void onCoinArrive(GameObject coinGO)
    {
        Coin coin = coinGO.GetComponent<Coin>();
        if (coin != null && dlTo != null)
        {
            dlTo.addValue(coin.value);
            SoundsSystem.play(Sound.S_COIN, dlTo.transform.position);
        }
    }
}
