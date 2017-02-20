using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TemplatesFrom {
    CLIENT_OLD = 0, // (отключено) Тестовые шаблоны, (обычные линии), которые работали в одиночном режиме, в первом прототипе игры
    CLIENT_FABRIC,  // ( недоделано и в силу множества кода было вынесенно в отдельный фалй на рабочем столе) Режим одиночной игры, где система могла анализировать, понимать, генерировать и перепроверять на сервере любыи игровые шаблоны
    JSON_FILE,      // (отключено) Режим в котором шаблоны читаются из файла.
    SERVER          // (актуально) шаблоны получаются от сервера
}
// Ниже идёт описания структуры шаблонов JSON
[System.Serializable]
public class templatesJSON{
    public int Templ_id;
    public int[] Positions;
}
[System.Serializable]
public class categoriesJSON{
    public string Name;
    public int CatId;
    public int Price;
    public string Version;
    public templatesJSON[] Templates;
}
[System.Serializable]
public class TemplateData
{
    public int Ver;
    public categoriesJSON[] Cat;
}
[System.Serializable]
public class ServerTemplatesData : ServerData{
    public TemplateData data;
}
// струкура по типу Vector2 для удобной работой с позициями в шаблонах ( сделано для режима: CLIENT_FABRIC и CLIENT_OLD )
public struct xy {
    public xy(int _x, int _y) {
        x = _x;
        y = _y;
    }
    public int x;
    public int y;
}
// Система шаблонов а так же держатель шаблонов (визуальный в розыгрыше, в верхней части экрана)
// отвечает за отображение шаблонов, и проверку в розыгрыше выиграши и превины по билетам
public class TemplatesHolder : MonoBehaviour {
    public Template templatePrefab;                         // префаб шаблона
    public List<Template> templates = new List<Template>(); // █ список шаблонов
    
    public DigitsLabel expectedWin;  // ожидаемый выиграшь, по всем билетам на текущем розыгрыше
    public Sprite[] digitsSprites;   // цифры используемые для отображения значений в каждом шаблоне
    MAIN main = MAIN.getMain;
    Transform templatesHolder = null;   // для удобного доступа

    void Awake() {
        main.templatesHolder = this;
        //if (main.gameMode != GameMode.SERVER) // если режим не сервер ранее работала система CLIENT_FABRIC. Поскольку она совсем отключена и вынесена из проэкта, данный код совсем не актуален
        //    main.loadDefaultTemplates();
    }
    void Start () {
        templatesHolder = transform.Find("Templete");
        expectedWin = transform.FindChild("ExpectedWin").GetComponent<DigitsLabel>();
        if (expectedWin != null) expectedWin.setValue(0);
        // █ инициализация шаблонов!
        main.templatesHolder.setServerTemplates();
    }
    // инициализированы ли шаблоны
    public bool isServerTemplatesInit() {
        return Rooms.get.serverTemplates != null;
    }
    // (не используется) Получить индексы категорий, имеющие заданую цену
    /*List<int> getCategoriesByPrice(int price, int from = 0) {
        List<int> res = new List<int>();
        for (int i = from; i < Rooms.get.serverTemplates.Cat.Length; i++)
            if (Rooms.get.serverTemplates.Cat[i].Price == price) res.Add(i);
        return res;
    }*/
    // █ обновления значений ожидаемого выиграша, по событию - появление/исчезновение первинов, а так же начисление монет, по выиграшу, маркирование шаров соответственно
    public void updateExpectedWinByNewPrewinsAndWins(JsonHandler.PreWin[] preWins, JsonHandler.Win[] wins ) {
        if (preWins != null)
            for(int i=0; i<preWins.Length; i++) 
                expectedWin.addValue(preWins[i].W);
        if (wins!=null)
            for (int i = 0; i < wins.Length; i++) {
                //expectedWin.addValue();
                main.raffle.flyWinMoneyToPocket += wins[i].W;
            }

    }
    // Инициализация визуально отображаемых шаблонов в верхней панеле (█ только после инициализации шаблонов)
    public void setServerTemplates() {
        int countTemplates = 3;
        createTemplates(countTemplates);
        // индекс в масиве - это № визуальной ячейки в которой будет отображаться шаблоны.
        // индекс в параметре - это индекс категории, со всем списком шаблонов, которые поочереди будут отображаться в заданной ячейке
        templates[0].addCategoryToDrawing(6);
        templates[0].addCategoryToDrawing(5);
        templates[1].addCategoryToDrawing(4);
        templates[1].addCategoryToDrawing(3);
        templates[1].addCategoryToDrawing(2);
        templates[2].addCategoryToDrawing(1);
    }
    // Получить категорию по её (JSON) id
    categoriesJSON getCategoryByID(int id){
        if (Rooms.get.serverTemplates != null)
        for (int i = 0; i < Rooms.get.serverTemplates.Cat.Length; i++)
            if (Rooms.get.serverTemplates.Cat[i].CatId == id)
                return Rooms.get.serverTemplates.Cat[i];
        return null;
    }
    // (отлкючено) Получить категорию по её индексу в масиве
    /*categoriesJSON getCategory(int index) {
        print("getCategory(" + index + ")");
        return serverTemplates.Cat[index];
    }*/
    // Получить шаблоны из указаной категории, по указаному (JSON) id
    templatesJSON getTemplate(categoriesJSON inCategory, int id) {
        if (inCategory != null) {
            //print("inCategory.Templates.Length: " + inCategory.Templates.Length);
            for (int i = 0; i < inCategory.Templates.Length; i++) {
                if (inCategory.Templates[i].Templ_id == id)
                    return inCategory.Templates[i];
            }
        }
        return null;
    }
    /*public int getCategoryPrice(int categoryIndex) {
        return getCategory(categoryIndex).Price;
    }*/
    // получить цену за шаблон в категории: (JSON) id
    public int getCategoryPrice(int categoryID) {
        //print("getCategoryPrice(" + categoryID + ")");
        return getCategoryByID(categoryID).Price;
    }
    // количество категорий
    public int getCategoriesCount() {
        return Rooms.get.serverTemplates.Cat.Length;
    }
    // количество шаблонов в категории: (JSON) id
    public int getTemplatesCount(int categoryID) {
        return getCategoryByID(categoryID).Templates.Length;
    }
    /*public int[] getTicketPositions(int categoryIndex, int templateNum) {
        return getTicketPositions(getCategory(categoryIndex), templateNum);
    }*/
    // получить позиции по указанным: (JSON) id категории и шаблона
    public int[] getTicketPositionsByCategoryID(int categoryID, int templateNum) {
        //print("getTicketPositionsByCategoryID(" + categoryID + ")");
        return getTicketPositions(getCategoryByID(categoryID), templateNum);
    }
    int[] getTicketPositions(categoriesJSON c, int templateNum){
        //var c = getCategoryByID(categoryID);
        var t = getTemplate(c, templateNum);
        //for (int i = 0; i < t.Positions.Length; i++) print(t.Positions[i]);
        if (t!=null) {
            return t.Positions;
        }
        //print("Error! [getTicketPositions] categoryID#"+ c.CatId + " with template#"+ templateNum + " not find!");
        return new int[] { };
    }
    // ( █ Не проверялось для режима SERVER ) удаление всех шаблонов
    public void removeAllTemplates() {
        if (templatesHolder != null && templatesHolder.childCount > 0)
            for (int i = 0; i < templatesHolder.childCount; i++)
                Destroy(templatesHolder.GetChild(i).GetComponent<Template>().gameObject);
    }
    // Выравнивание позиций визуально отображающих шаблонов в верхней позиции экрана
    void alignTempleatesPos() {
		float templatesWidth = main.templatesWidth;// 2.5f;
        int templatesCount = templatesHolder.childCount;
        float totalWidth = templatesCount * templatesWidth;
        float sLeft = -totalWidth * 0.5f + templatesWidth * 0.5f;
        for (int i = 0; i < templatesCount; i++) {
            Transform t = templatesHolder.GetChild(i);
            t.localPosition = new Vector3(sLeft + i * templatesWidth, 0.0f, 0.0f);
        }
    }
    // (отключено, только для режима CLIENT_FABRIC) загрузка шаблонов по умолчанию или из файла где они были сгенерированы до этого в клиенте.
    /*public void loadTemplatesFromStrings(List<string> s) {
        string[] strings = new string[s.Count];
        for (int i = 0; i < s.Count; i++)
            strings[i] = s[i];
        loadTemplatesFromStrings(strings);
    }*/
    // создание визуального шаблона
    void createTemplates(int count) {
        for (int i = 0; i < count; i++) {
            Template t = GameObject.Instantiate(templatePrefab) as Template;
            t.name = "Template" + i;
            t.transform.parent = templatesHolder.transform;
            t.transform.localScale = transform.localScale;
            templates.Add(t);
        }
        alignTempleatesPos();
    }

    /*public void loadTemplatesFromStrings(string[] s) {
        //createTemplates() here was bin and in body cikle:  //t.initWithString(s[i]);
    }*/
    // получить шаблон по имени
    public Template getTemplateByName(string name) {
        //print("[getTemplateByName] with name:" + name);
        for (int i = 0; i < templates.Count; i++) {
            if (templates[i].name == name)
            {
                //print("name in list[" + i + "]:" + templates[i].name);
                return templates[i];
            }
        }
        return null;
    }
}
