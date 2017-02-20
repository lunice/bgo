using UnityEngine;
using System.Collections;
// Класс наполнитель контента всплывающего окна: "Обмене кристалов на золото"
public class BuyGoldWnd : PopUpWindow {
    WindowController windowController = WindowController.getWinController;  // для удобного доступа
    void Start () { }
	void Update () { }

    public RESOURCES getResources() // для удобного доступа к ресурсам
    {
        GameObject resGO = GameObject.Find("RESOURCES");
        return resGO.GetComponent<RESOURCES>();
    }
    public override GameObject createContent() // создание контента, на основе радиобатона, заполнение заготовленых айтемов
    {
        var goldFromCrystalExchange = windowController.getMarketExchange();
        GameObject prefab = getResources().goldBuyItemPrefab;
        GameObject content = new GameObject("Content");
        GameObject radioButtons = new GameObject("RadioButtons");
        radioButtons.transform.parent = content.transform;
        RadioButtons rb = radioButtons.AddComponent<RadioButtons>();
        rb.shift = new Vector2(0.0f, -2.09f);
        int len = goldFromCrystalExchange.Length;
        rb.init(prefab, len, RadioButtons.TypeDisposition.VERTICAL, 1.0f, true);
        isHaveBonuses = false;
        for (int i = 0; i < goldFromCrystalExchange.Length; i++)
            if (goldFromCrystalExchange[i].Free > 0) isHaveBonuses = true;
        fillGoldItems(rb);
        rb.subscribeOnRadioBtnSelected(onBuyGoldClick);
        return content;
    }

    bool isHaveBonuses = true;  // наличие 
    void fillGoldItems(RadioButtons radioButtons){
        var items = windowController.getMarketExchange();
        for (int i = 0; i < radioButtons.transform.childCount; i++) {
            Transform tChild = radioButtons.transform.GetChild(i);
            var ico = tChild.FindChild("ico").GetComponent<SpriteRenderer>();
            string icoAddr = "PopUpWindows/money" + (i + 1) + "a";
            ico.sprite = Resources.Load<Sprite>(icoAddr);
            var countGold = tChild.FindChild("countBuyItem").GetComponent<DigitsLabel>();
            var aditionalCrystals = tChild.FindChild("aditionalBuyItem").GetComponent<DigitsLabel>();
            var buttonGO = tChild.Find("Button");
            var cost = buttonGO.Find("costLabel").GetComponent<DigitsLabel>();
            var gE = items[i];
            int lenFrom = gE.From.Name.Length;
            int lenTo = gE.To.Name.Length;

            countGold.setValue(gE.To.Count);
            if (gE.Free > 0 )
                aditionalCrystals.setValue(gE.Free, DigitsLabel.AdditionalPrefixSymbols.PLUS);
            else { // если бонусов нету прячем ненужные элементы:
                aditionalCrystals.gameObject.SetActive(false);
                tChild.Find("coinsIco").gameObject.SetActive(false);
                tChild.Find("freeLabel").gameObject.SetActive(false);
                tChild.Find("underButtonLabel").gameObject.SetActive(false);
                if (!isHaveBonuses) {
                    var p = countGold.transform.position;
                    countGold.transform.position = new Vector2(p.x + 1.0f, p.y);
                }
            }
            cost.setValue(gE.From.Count);
            buttonGO.name = i.ToString();
        }
    }

    void onBuyGoldClick(BaseController selectedBtn) {
        var exchangeRqst = windowController.getMarketExchange()[int.Parse(selectedBtn.name)];
        if (MAIN.getMain.rubins.getValue() >= exchangeRqst.From.Count)
            ExchangeEvent.OnExchange(exchangeRqst.From, exchangeRqst.To, 1);
        else {
            HUD.playAnimNeedMoreRubins();
        }
    }
}