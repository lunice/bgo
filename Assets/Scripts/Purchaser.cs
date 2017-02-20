using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class PurchaseableProductNames {
    public const string Crystal_1 = "crystal_1";
}

public class Purchaser : IStoreListener {

    private static IStoreController _storeController;
    private static IExtensionProvider _storeExtensionProvider;
    private List<PurchaseableItem> _purchaseableProducts = new List<PurchaseableItem>();
    private EventHandler<EventArgs> _onReady;
    private MarketPurchase[] _items;

    public List<PurchaseableItem> purchaseableProducts { get { return _purchaseableProducts; } }

    public void InitializePurchasing(MarketPurchase[] items, EventHandler<EventArgs> ready) {
        _onReady = ready;
        _items = items;

        if (_storeController == null) {
            if (IsInitialized()) {
                return;
            }
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (MarketPurchase item in _items) {
            builder.AddProduct(item.Name, ProductType.Consumable, new IDs() { { item.Name, GooglePlay.Name } });
        }
        UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized() {
        return _storeController != null && _storeExtensionProvider != null;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        Debug.Log("OnInitialized: OK");
        _storeController = controller;
        _storeExtensionProvider = extensions;

        foreach (MarketPurchase item in _items) {
            Product product = _storeController.products.WithID(item.Name);
            if (product != null && product.availableToPurchase) {
                _purchaseableProducts.Add(new PurchaseableItem {
                    Name = product.definition.id,
                    Currency = product.metadata.isoCurrencyCode,
                    Price = product.metadata.localizedPrice,
                    Free = item.Free
                });
                Debug.Log("Product: " + product.definition.id + ", " + product.metadata.isoCurrencyCode + ", " + product.metadata.localizedPrice + ", " + item.Free);
            }
        }
        
        _onReady(this, new EventArgs());
    }


    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        // какая-то из полученых от нашего сервера - айтемок, не смогла проинициализироваться в плей маркете
        Errors.showError(Errors.TypeError.EP_ON_INITIALIZE_FAILED);
    }

    public int BuyProduct(string productId) {
        if (productId != null) {
            if (IsInitialized()) {
                Product product = _storeController.products.WithID(productId);

                if (product != null && product.availableToPurchase) {
                    printLog(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    _storeController.InitiatePurchase(product);
                    return 0;
                }
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase"); // внутрення ошибка покупки
                Errors.showError(Errors.TypeError.EP_ON_BUY_NOT_FIND);
                return -1;
            }
            Debug.Log("BuyProductID FAIL. Not initialized."); // внутренняя ошибка покупки
            Errors.showError(Errors.TypeError.EP_ON_INITIALIZE_FAILED);
            return -1;
        }
        Debug.Log("ConfirmPurchase: product is empty"); // пусто, вероятнее всего с маркета ничего не получил
        Errors.showError(Errors.TypeError.EP_ON_BUY_NOT_FIND);
        return -1;
    }

    void printLog(string s) {
        MAIN main = MAIN.getMain;
        main.setMessage("█"+s);
        //Debug.Log(s);
    }
    public int ConfirmPurchase(string productId) {
        //Debug.Log("█ [ConfirmPurchase]" + productId);
        if (productId != null) {
            var product = _storeController.products.WithID(productId);
            if (product != null) {
                if (_storeController.products.WithID(productId).hasReceipt) {
                    _storeController.ConfirmPendingPurchase(_storeController.products.WithID(productId));
                    SoundsSystem.play(Sound.S_RUBINS_BUY);
                    return 0;
                }
                printLog("ConfirmPurchase: Hasn't Receipt"); // ошибка покупки, нету ордера / заказа. Неправильно выбран продукт
                Errors.showError(Errors.TypeError.EP_ON_FAILED_CONFIRM_PURCHASE);
                return -1;
            }
            printLog("ConfirmPurchase: Hasn't product: " + productId); // ошибка покупки, нет товара
            Errors.showError(Errors.TypeError.EP_ON_FAILED_CONFIRM_PURCHASE);
            return -1;
        }
        printLog("ConfirmPurchase: product is empty"); // ошибка покупки продукт пустой.
        Errors.showError(Errors.TypeError.EP_ON_FAILED_CONFIRM_PURCHASE);
        return -1;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {

        Debug.Log("Receipt: " + args.purchasedProduct.receipt);
        if (_storeController.products.WithID(args.purchasedProduct.definition.id) != null) {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            BuyEvent.OnBuy(args.purchasedProduct.receipt, args.purchasedProduct.metadata.isoCurrencyCode, args.purchasedProduct.metadata.localizedPrice);
            return PurchaseProcessingResult.Pending;
        } else {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id)); // неопределён продукт, не понятно что это
        }
        return PurchaseProcessingResult.Complete;
    }

    public void ClearPurchase() {
        try {
            UnityPurchasing.ClearTransactionLog();
        } catch (Exception ex) {
            if (MAIN.IS_TEST) {
                Debug.Log("Exception: " + ex);
                Debug.Log("StackTrace: " + ex.StackTrace);
            }
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason)); // при ошибке покупки, что за продукт по причине PurchaseFailureReason
    }

    public bool isHasNameInStartString(string s, string name) {
        if (s.Length < name.Length) return false;
        for (int i = 0; i < name.Length; i++) if (s[i] != name[i]) return false;
        return true;
    }
    Dictionary<String, PurchaseableItem[]> sortingPurchaseableItem = new Dictionary<string, PurchaseableItem[]>();
    public PurchaseableItem[] getMarketItesByName(string name) {
        if (sortingPurchaseableItem.ContainsKey(name)) return sortingPurchaseableItem[name];
        if (purchaseableProducts.Count == 0) {
            Debug.Log("Error! [getMarketItesByName(" + name + ")] : marketPurchaser == null || marketPurchaser.Length == 0"); // ошибка клиента покупочный модуль не определён
            //testInit();
            return null;
        }
        List<PurchaseableItem> lMP = new List<PurchaseableItem>();
        for (int i = 0; i < purchaseableProducts.Count; i++){
            string curName = purchaseableProducts[i].Name.Substring(0, name.Length);
            if (isHasNameInStartString(curName,name)) lMP.Add(purchaseableProducts[i]);
        }
        if (lMP.Count == 0) {
            Debug.Log("Warning! [getMarketItesByName(" + name + ")] : name not find"); // ошибка клиента не удалось получить предмет по имени
            return null;
        }
        PurchaseableItem[] res = new PurchaseableItem[lMP.Count];
        for (int i = 0; i < lMP.Count; i++) res[i] = lMP[i];
        sortingPurchaseableItem.Add(name, res);
        return res;
    }
}