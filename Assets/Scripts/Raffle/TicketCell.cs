using UnityEngine;
using System.Collections;
// Класс отвечающий за работу клеток билетов и их отображения
public class TicketCell : MonoBehaviour {
    public enum Oriol   // состояния маркировки ориолов 
    {
        NONE = 0,       // нет никакой
        SHOW_PREWIN,    // анимация показа превина
        PREWIN,         // превин
        SHOW_WIN,       // анимация отображения выиграша
        WIN             // выиграшь
    }

    public bool isMarked = false; // маркирован ли
    public bool isHorseshoe = false; // подкова ли
    public TypeMark markType = TypeMark.NONE; // тип маркировки
    private int m_numValue;     // номер шара в клетки 
    int orderLayer = 3;         // приоритет рендеренга

    //ObjectCaption objectCaption;// старая подпись клеток
    SpriteRenderer markSR;      // маркировочный SpriteRenderer
    SpriteRenderer oriolSR;     // SpriteRenderer ориола превинов
    public Sprite markSprite;   // картинка маркировки
    public Sprite oriolSprite;  // картинка ориолы превинов
    public Sprite greenBall;    // (не актуально, подкрашивается белый)картинка зелёного шара
    RESOURCES resources;        // для удобного доступа
	MAIN main = MAIN.getMain;

    void Start () {
        GameObject resGO = GameObject.Find("RESOURCES");
        resources = resGO.GetComponent<RESOURCES>();
        //objectCaption = GetComponent<ObjectCaption>();
        updateCellCaption();    // инициализациия подписи
        orderLayer = transform.parent.parent.GetComponent<SpriteRenderer>().sortingOrder;
    }
    public void changeMark() // изменение маркировки клетки
    {
        if (isMarked) unmark();
        else mark();
    }
    public Ticket getMyTicket() // доступ к билету
    {
        return transform.parent.parent.GetComponent<Ticket>();
    }
    public enum TypeMark    // маркировка шаром
    {
        NONE = 0,       // нет никакой 
        PRESENT,        // присутствует (зелёный)
        PREWIN,         // мигающий шар
        //SHOW_PREWIN,
        WITH_OUT,       //
        //SHOW_WIN,
        WIN             // выиграшный золотистый
    }
    public void mark(TypeMark typeMark = TypeMark.NONE) // маркировка клетки
    {
        //print("█ #"+m_numValue+", ticket#"+getMyTicket().number+" newTypeMark:" + typeMark + " old:" + markType);
        if (typeMark == markType) return;
        if (markType == TypeMark.WIN){
            //print("█ #" + m_numValue + " when WIN!, newState: " + typeMark);
            return;
        } else if(markType == TypeMark.WITH_OUT){
            markSR.tag = "ticketBall";
            stopPlayPrewin(true);
        }

        if (!markSR) {
            //Instantiate(Sprite);
            GameObject go = new GameObject();
            go.transform.parent = this.transform;
            go.tag = "ticketBall";
            go.name = go.tag + numValue;
            markSR = go.AddComponent<SpriteRenderer>();
            markSR.sprite = Instantiate(greenBall);
            markSR.transform.position = Vector3.zero;
            markSR.transform.localScale = transform.localScale;
            markSR.sortingOrder = orderLayer + 1;
            go.transform.position = Vector3.zero;
            go.transform.localPosition = Vector3.zero;
        }
        markSR.enabled = true;
        switch (typeMark) {
            case TypeMark.PREWIN: {setOriol(Oriol.PREWIN); } break;
            case TypeMark.WITH_OUT: {
                    //print("█ #"+m_numValue+" WITH_OUT, prewState: " + markType);
                    startPlayPreWin();
                    tag = "missingBall";
                } break;
            case TypeMark.NONE: { } break;
            case TypeMark.PRESENT: { } break;
            case TypeMark.WIN: {
                    //print("█ █#" + m_numValue + " WIN, prewState: " + markType + " markSR.name == "+ markSR.name);
                    markSR.sprite = markSprite;
                    markSR.color = Color.yellow;
                    setOriol(Oriol.NONE);
                    if (GetComponent<Scaling>() == null)
                        Scaling.set(gameObject, 1.5f, 0.75f, 1); // # маштабирование каждого шара на виновом состоянии
                    if (GetComponent<Flickering>() == null)
                        Flickering.set(gameObject, 0.75f, 1); // #V мигание каждого шара на виновом состоянии
                } break;
            }
        markType = typeMark;
        isMarked = true;
    }

    void startPlayPreWin() // начать проигрывать анимацию превина
    { 
        isPlayAnimateion = true;
    }
    bool stoping = false;   // остановка проигрывания анимации
    public void stopPlayPrewin(bool isVisible = false) // остановка проигрования анимации превина
    {
        if (isVisible) {
            //markSR.color = Color.white;
            var col = markSR.color;
            markSR.color = new Color(col.r, col.g, col.b, 1.0f);
            isPlayAnimateion = false;
        } else stoping = true;
    }
    public bool isPlayAnimateion = false; // играет ли анимация
    float timeStartPlay = 0;     // начало времени проиграша анимации
    float timePeriodPlay = 1.0f; // период времени проиграша анимации
    void Update() // нужно только для проигрования анимации 
    {
        if (isPlayAnimateion) {
            float diff = (Time.time - timeStartPlay) * (1.0f / Time.timeScale);
            if ( diff > timePeriodPlay) {
                if (stoping){ stoping = false; isPlayAnimateion = false; }
                timeStartPlay = Time.time;
            } else {
                float progress = diff / timePeriodPlay;
                if (progress >= 0.5f)
                    progress = 0.5f - (progress - 0.5f);
                markSR.color = new Color(1.0f, 1.0f, 1.0f, progress * 2);
            }
        }
    }

    public void unmark() // снять маркировку с клетки
    {
        if (!isMarked)
            return;
        markSR.enabled = false;
        isMarked = false;
    }
    public void setOriol(Oriol oriolType) // установить/снять/проиграть анимацию предвыиграшных ориолов 
    {
        if (!oriolSR ) {
            if (oriolType == Oriol.NONE)
                return;
            //Instantiate(Sprite);
            GameObject oriol = new GameObject();
            oriol.transform.parent = this.transform;
            oriol.name = "ballAureole";
            oriol.tag = "aureols";
            oriolSR = oriol.AddComponent<SpriteRenderer>();
            oriolSR.sprite = Instantiate(oriolSprite);
            oriolSR.transform.position = Vector3.zero;
            oriolSR.sortingOrder = orderLayer + 2;
            oriolSR.transform.localScale = transform.localScale;
            oriol.transform.position = Vector3.zero;
            oriol.transform.localPosition = Vector3.zero;
        } else {
            if (oriolType == Oriol.NONE) oriolSR.gameObject.SetActive(false);
            else oriolSR.gameObject.SetActive(true);
        }
    }
    public void setHorseshoe(bool horseshoe) // установить подкову (при проверках в шаблонах, в старой логике прототипа проходила проверку как присутствующий шар)
    {
        isHorseshoe = horseshoe;
        isMarked = true;
        if (!resources) Start();
        if (isHorseshoe) {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = resources.horseshoeTicketCell;
        }
    }
    public void updateCellCaption() // обновить/установить подпись клетки
    {
        if (transform.childCount > 0)
            return;
            for(int i=0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i));
            }
        if (isHorseshoe)
            return;
        
        if (!resources) return;
        
        GameObject go = new GameObject();
        go.transform.parent = transform;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        int order = orderLayer + 4;
        sr.sortingOrder = order; 
        if (m_numValue < 10) {
            go.transform.localPosition = Vector3.zero;
            sr.sprite = resources.ticketDigits[m_numValue];
            //sr.color = Color.blue;
        } else {
            GameObject go2 = new GameObject();
            go2.transform.parent = transform;
            SpriteRenderer sr2 = go2.AddComponent<SpriteRenderer>();
            int n = (m_numValue / 10);
			sr.sprite = resources.ticketDigits[n];
			sr2.sprite = resources.ticketDigits[m_numValue % 10];
            sr2.sortingOrder = order;
            //sr.color = Color.blue;
            //sr2.color = Color.blue;
            float indent = main.indentTicketDigets * 0.5f;
			go.transform.localPosition = new Vector3(-indent, 0.0f, 0.0f);
			go2.transform.localPosition = new Vector3(indent, 0.0f, 0.0f);
        }
    }
    public int numValue // установить / получить значение клетки
    {
        set {
            m_numValue = value;
            updateCellCaption();
        }
        get { return m_numValue; }
    }
/*    void OnGUI() // для отображения подписик клеток, устарелая логика в прототипах
      {
        Vector3 transformPos = this.transform.localPosition;
        transformPos.x += Screen.width * 0.5f;
        transformPos.y += Screen.height * 0.5f;

        print(transformPos);
        Rect labelRect = new Rect(transformPos, new Vector2(128.0f, 128.0f));
        GUI.Label(labelRect, numValue.ToString());
    }*/
}
