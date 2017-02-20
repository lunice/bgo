using UnityEngine;
// Класс описывающий визуальный функционал покупки кристалов
public class BuyCrystalsWnd : PopUpWindow {
    WindowController windowController = WindowController.getWinController; // для удобного доступа
    public RESOURCES getResources() // для удобного доступа, и загрузки части ресурсов / префабов
    {
        GameObject resGO = GameObject.Find("RESOURCES");
        return resGO.GetComponent<RESOURCES>();
    }
    public override GameObject createContent() // создание контента при условии полученых данных от сервера
    {
        var crystalItems = windowController.getCrystalItems();
        GameObject prefab = getResources().crystalBuyItemPrefab;
        var market = windowController.getMarketEvent();
        GameObject content = new GameObject("Content");
        GameObject radioButtons = new GameObject("RadioButtons");
        radioButtons.transform.parent = content.transform;
        RadioButtons rb = radioButtons.AddComponent<RadioButtons>();
        rb.shift = new Vector2(0.0f, -1.67f);
        int len = crystalItems.Length;
        rb.init(prefab, len, RadioButtons.TypeDisposition.VERTICAL, 0.8f, true);
        rb.subscribeOnRadioBtnSelected(onBuyCrystalClick);
        fillCrystalItems(rb);   // заполнение
        return content;
    }
    void fillCrystalItems(RadioButtons radioButtons) // заполнения контента, данными из сервера
    {
        var crystalItems = windowController.getCrystalItems();
        //print(crystalItems.Length);
        for (int i = 0; i < radioButtons.transform.childCount; i++) {
            Transform tChild = radioButtons.transform.GetChild(i);
            var ico = tChild.FindChild("ico").GetComponent<SpriteRenderer>();
            string icoAddr = "PopUpWindows/rubin" + (i + 2);
            ico.sprite = Resources.Load<Sprite>(icoAddr);
            var countCrystals = tChild.FindChild("countBuyItem").GetComponent<DigitsLabel>();
            var buttonGO = tChild.Find("Button");
            var aditionalCrystals = tChild.FindChild("aditionalBuyItem").GetComponent<DigitsLabel>();
            var cost = tChild.Find("Button").Find("costLabel").GetComponent<DigitsLabel>();
            int len = crystalItems[i].Name.Length;
            string str = crystalItems[i].Name.Substring(8, len - 8);
            countCrystals.setValue(int.Parse(str));
            if (crystalItems[i].Free > 0)
                aditionalCrystals.setValue(crystalItems[i].Free, DigitsLabel.AdditionalPrefixSymbols.PLUS);
            else { // если бонусов нету прячем ненужные элементы:
                aditionalCrystals.gameObject.SetActive(false);
                tChild.Find("coinsIco").gameObject.SetActive(false);
                tChild.Find("freeLabel").gameObject.SetActive(false);
                tChild.Find("underButtonLabel").gameObject.SetActive(false);
            }
            cost.setFloatValue((float)(crystalItems[i].Price));

            buttonGO.name = i.ToString();
        }
    }
    void buy(BaseController selectedBtn) // операция покупки
    {
        var crystalItems = windowController.getCrystalItems()[int.Parse(selectedBtn.name)];
        MAIN.getMain.purchase.BuyProduct(crystalItems.Name);
    }

    void onBuyCrystalClick(BaseController selectedBtn) // событие: нажатие на одну из кнопок покупки рубинов
    {
        if (MAIN.getMain.isShowWarningWindow) {
            var wnd = Errors.showWarningWindow();
            wnd.setAction(0, () => { buy(selectedBtn); });
        } else buy(selectedBtn);
    }
}