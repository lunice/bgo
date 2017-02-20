using UnityEngine;
using System.Collections;
using System.Collections.Generic;
////////////////////////////////////////////////////////////////////
// Класс отвечающий за шаблон, и его визуальном отображении в розыгрыше, в верхней части экрана ими заполняется панель в которой есть ряд из трёх таких шаблонов
// ███ !! ( TODO ) он создаётся при самом розыгрыше, что не есть хорошо и нужно вынести его инициализацию после или во время авториации!
public class Template : MonoBehaviour {
    MAIN main = MAIN.getMain;

    public Sprite templateTiledCell;    // маленький спрайт которым визуально отображаются выиграшные позиции на билете, и их стоимость в верхней информационной панеле
    //public Sprite BackGround = null;
    Vector2 shift = new Vector2(-0.03f,0.3f);       // смещение позиции шаблона, не зависимо от BackGround
    Vector2 indent = new Vector2(0.18f, 0.179f);    // отступ между позициями шаблона
    DigitsLabel costLabel;                          // стоимость шаблона
    TemplatesHolder templatesHolder;                // Держатель шаблонов ( панель в верхней части экрана )
    Transform positionHolder;                       // Позиция держателя
    float drawingDelay = 0.5f;                      // задержка между отображением шаблонов в категориях
    public float lastDraw;                          // фиксация отрисовки текущего шаблона
    int currentCategoryNum;                         // текущая категория (поскольку шаблоны могут отображатся из разных категорий)

    // Класс - рисующая категория (отображает в себе список шаблонов
    public class DrawingCategory {                  
        public int categoryID;          
        public int[] drawingTemplates;  // список отображаемых шаблонов
        public int currentTemplate;     // текущий отображаемый шаблон
        public int reward;              // награда за текущий шаблон
        // Рисование категорий
        public DrawingCategory(int catID, int[] numTemplates = null) {
            categoryID = catID;
            var th = MAIN.getMain.templatesHolder;
            reward = th.getCategoryPrice(catID);
            if (numTemplates == null) {
                drawingTemplates = new int[th.getTemplatesCount(categoryID)];
                for (int i = 0; i < drawingTemplates.Length; i++) drawingTemplates[i] = i;
            } else drawingTemplates = numTemplates;
            currentTemplate = 0;
        }
    }

    // Список из рисующихся категорий
    List<DrawingCategory> drawingCategories = new List<DrawingCategory>();
    // Возвращает визуальный порядок отображения текстур
    int getOrderLayer() {
        SpriteRenderer templatesHolderSP = templatesHolder.GetComponent<SpriteRenderer>();
        return templatesHolderSP.sortingOrder + 1;
    }
    // инициализация лейбела для вывода стоимости шаблона 
    void Start() {
        templatesHolder = main.templatesHolder;
        if (templatesHolder == null) {
            print("Error! [Start] templatesHolder == null");
            return;
        }
        var costLabelGO = new GameObject("costLabel");
        costLabel = costLabelGO.AddComponent<DigitsLabel>();
        costLabel.spriteDigits = main.templatesHolder.digitsSprites;
        costLabelGO.transform.parent = transform;
        costLabel.transform.localScale = transform.localScale * 0.5f;
        costLabel.transform.localPosition = new Vector3(-0.47f,0.19f,0.0f);
        costLabel.indent = 0.1f;
        costLabel.scale = 1.2f;
        costLabel.orderLayer = getOrderLayer() + 1;
        GameObject go = new GameObject("positionHolder");
        go.transform.parent = this.transform;
        positionHolder = go.transform;
        positionHolder.localPosition = Vector3.zero;
        positionHolder.localScale = templatesHolder.transform.localScale;
        shift = new Vector2(shift.x - indent.x * 5, shift.y - indent.y * 5);
    }
    // Добавить категорию к рисованию текущией категории
    public void addCategoryToDrawing( int categoryID) {
        for (int i = 0; i < drawingCategories.Count; i++)
            if (categoryID == drawingCategories[i].categoryID)
                return;
        drawingCategories.Add(new DrawingCategory(categoryID));
    }
    // Удалить категорию из списка рисования категории
    public void removeCategoryFromDrawing(int categoryID) {
        for (int i = 0; i < drawingCategories.Count; i++)
            if (drawingCategories[i].categoryID == categoryID)
                drawingCategories.RemoveAt(i);
    }
    // начать рисования с ...
    public void startDrawTemplatesInCategories(int from = 0) { currentCategoryNum = from; }
    // отрисовка позиций текущего шаблона в категории, и его цена
    void drawTemplateCells(int[] positions,int price = 0) {
        int startCreateFrom = 0;
        if (positionHolder.childCount > positions.Length) {
            for(int i = positions.Length; i < positionHolder.childCount; i++) {
                positionHolder.GetChild(i).gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < positions.Length; i++) {
            GameObject cell;
            if ( i > positionHolder.childCount - 1 ) {
                cell = new GameObject("position" + i);
                cell.transform.parent = positionHolder;
                cell.transform.localScale = positionHolder.localScale;
                var sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = templateTiledCell;
                sr.sortingOrder = getOrderLayer() + 3;
                //print("sr.sortingOrder:" + sr.sortingOrder);
            } else {
                cell = positionHolder.GetChild(i).gameObject;
                cell.SetActive(true);
            }
            int x = positions[i] % 5;
            int y = positions[i] / 5;
            if (x == 0) y--;
            cell.transform.localPosition = new Vector3(x * indent.x + shift.x, y * indent.y - shift.y, 0.0f);
        }

        if (price != 0) costLabel.setValue(price);
    }
    //void showTemplate(int numCategory, int numTemplate) {}
    //void draw( int numVariable ) {}
    // Здесь происходит отчисление времени и смена отображаемых шаблонов в установленных категориях, и соответственная установка их цен
    void Update () {
        if (drawingCategories.Count > 0 && Time.time - lastDraw > drawingDelay) {
            lastDraw = Time.time;
            int categoryID = drawingCategories[currentCategoryNum].categoryID;
            int curTemplate = ++drawingCategories[currentCategoryNum].currentTemplate;
            
            var positions = templatesHolder.getTicketPositionsByCategoryID(categoryID, curTemplate);
            //print("-------------" + categoryID);
            //print("-------------" + drawingCategories[currentCategoryNum].currentTemplate);
            drawTemplateCells(positions);
            int newPrice = templatesHolder.getCategoryPrice(categoryID);
            if (newPrice != costLabel.getValue()) costLabel.setValue(newPrice);

            if (curTemplate == drawingCategories[currentCategoryNum].drawingTemplates.Length) {
                drawingCategories[currentCategoryNum].currentTemplate = 0;
                currentCategoryNum++;
            }
            if (currentCategoryNum == drawingCategories.Count) currentCategoryNum = 0;
        }
    }
}
