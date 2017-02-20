using UnityEngine;
using System.Collections;
// Класс шар! Навешан на шары в лотке, летящих звёздах и в шарах в барабане
public class Ball : MonoBehaviour {
    enum BallState {
        ROLL,
        STOP,
        ALIGNING,
        FLY_TO,
    }

    public delegate void OnBallStop(Ball ball);
    private OnBallStop callBack;
    public void subscribeOnBallStop(OnBallStop newCallBackFunction) { callBack = newCallBackFunction; }
    MAIN main = MAIN.getMain;

    public int number = 0;                      // розыгрышный номер
    float timeBorn;                             // время порождения
    bool aligned = false;                       // Выравнивающиейся. После остановки или очередных соударений, в зависимости от значения этой переменной запускается механизм медленного выравнивания шара.
    float speedAlignRotation = 0.2f;            // скорость выравнивания в лотке по остановке шара
    BallState state;                            // состояние
    //Vector3 prevPos = new Vector3(0, 0, 0);   // Предведущая позиция, запоминается, для коррекции траектории вылета звёзд из этого шара
    const int lenHistorySpeed = 1;              // длинна истории предведущих шаров зафиксированных с определённым интервалом
    //public Vector2[] prevVelocity = new Vector2[lenHistorySpeed]; // масив нужен для много кратной фиксации скоротси шара в лотке с указаным промежутком времени
    public Vector2[] prevVelocity;              // Предведущее значение скорости, запоминается, для коррекции траектории вылета звёзд из этого шара
    float timeDelayForPrevVelocity = 0.0005f;   // период фиксации истории
    float lastFixedVelocityTime;                // последнее значение в истории
    public JsonHandler.BallJSON jsonBallInfo = null; // JSON информация о этом шаре
    Rigidbody2D m_rigibody;                     // для быстрого доступа к собственному Rigidbody2D

    void Awake() {
        prevVelocity = new Vector2[lenHistorySpeed];
        for (int i = 0; i < lenHistorySpeed; i++) {
            prevVelocity[i] = Vector2.zero;
        }
    }
    void Start () {
        timeDelayForPrevVelocity = main.timeDelayFilingBalls * 0.35f;
        timeBorn = Time.time;
        m_rigibody = GetComponent<Rigidbody2D>();
        if (number == 0 && name.Length > 7) { // при инстантивации нового объекта имя подписывается как (clone) это удаляется
            int count = (name.Length == 9) ? 2:1;
            int num = int.Parse( name.Substring(6, count) );
            setNumber(num);
            setState(BallState.ROLL);
        }
	}
    // Спрятать визуальное отображение номера шара
    public void hideMyDigits() { // TODO ONLY DIGITS
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }
    // Тестовая функция, для вывода значения некоторых переменных и объектов данного класса (вызвается из Raffle)
    public void testPrint() {
        main.setMessage("    -█ moved ball #"+number);
        main.setMessage("    -state: " + state);
        main.setMessage("    -aligned: " + aligned);
        main.setMessage("    -m_rigibody:" + m_rigibody);
        main.setMessage("    -callBack:" + m_rigibody);
    }
    // Покрасить шар в зелёный
    public void setGreen(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = MAIN.getMain.getResources().greenBall;
        //sr.color = Color.green;
    }
    // Установить скорость ( используется при порождении шаров )
    public void setVelocity(Vector3 newVelocity) {
		Rigidbody2D rb = GetComponent<Rigidbody2D> ();
		rb.velocity = newVelocity;
	}
    // Получить порядок визуализации, для корректного отображения цифр на этом шаре
    public int getOrderLayer() {
        return GetComponent<SpriteRenderer>().sortingOrder;
    }
    // Установить порядок визуализации (и его детей)
    public void setOrderLayer(int val) {
        GetComponent<SpriteRenderer>().sortingOrder = val;
        if (transform.childCount > 0)
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = val + 1;
    }
    // Установить номер (изменяет визуальную надпись, имя)
    public void setNumber( int num ) {
		MAIN main = MAIN.getMain;
        number = num;
        GetComponent<ObjectCaption>().caption = num.ToString();
        // digits
        GameObject resGO = GameObject.Find("RESOURCES");
        if (!resGO) return;
        RESOURCES resources = resGO.GetComponent<RESOURCES>();
        GameObject go = new GameObject();
        go.transform.parent = transform;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		int orderLayer = getOrderLayer() + 1; // main.ticketDigitsOrder;
        sr.sortingOrder = orderLayer;
        if (num < 10) {
            go.transform.localPosition = Vector3.zero;
			sr.sprite = resources.ticketDigits[num];
        } else {
            GameObject go2 = new GameObject();
            go2.transform.parent = transform;
            SpriteRenderer sr2 = go2.AddComponent<SpriteRenderer>();
            int n = (num / 10);
			sr.sprite = resources.ticketDigits[n];
            sr2.sprite = resources.ticketDigits[num % 10];
			sr2.sortingOrder = orderLayer;
            float scaleX = transform.localScale.x;
            //print(0.35f * scaleX);
            //go.transform.localScale = transform.localScale ;
            //go2.transform.localScale = transform.localScale ;
			float indent = main.indentTicketDigets * 0.5f;
			go.transform.localPosition = new Vector3(-indent, 0.0f, 0.0f);
			go2.transform.localPosition = new Vector3(indent, 0.0f, 0.0f);
        }
    }
    // изменение стейта...
    void setState(BallState newState) {state = newState;}
    
    int fixedPause = 0; // фиксированя пауза, перед открытием пасти ( забыл зачем нужен этот костыль )
    // ряд процессов описаные внутри
    void FixedUpdate() {
        if (tag != "ReceivingTrayBall") return; // Вся логика ниже описанна только для шаров из лотка
        if ( DragonHead.isFullOpen()){          // если пасть дракона открылась полностью и нужно высыпать шары
            if (fixedPause++ > 10) fixedPause = 0; else return; // пауза...
            float py = transform.localPosition.y;
            if (py >= 2.0f && py <= 3.0f)       // глядя на позиции определяем местонахождения шара ( в нижней части лотка или верхней ) и толкание их в соответственном направлении
                m_rigibody.velocity = new Vector3(-5.0f, 0.0f); // влево
            else if (py <= -3.0f && py >= -3.8)
                m_rigibody.velocity = new Vector3(5.0f, 0.0f);  // вправо
            else if (py < -5.0f) Destroy(gameObject);   // если наш шар ниже экрана можем удалять
            return;
        } else
        if ( aligned ) return; // окончательный возможный стейт, после которого ничего не должно происходить
        if (m_rigibody == null) return;
        // фиксация скорости шара для корректной постройки траектории вылетающих звёзд
        if (lastFixedVelocityTime + timeDelayForPrevVelocity < Time.time) {
            for (int i = 1; i < lenHistorySpeed; i++)
                prevVelocity[i] = prevVelocity[i - 1];
            prevVelocity[0] = m_rigibody.velocity;
            lastFixedVelocityTime = Time.time;
        }
        // дополнительные действия в зависимости от стейта, оказались куда менее чем ожидалось...
        switch (state) {
            case BallState.ROLL : break;
            case BallState.STOP : break;
            case BallState.ALIGNING : { // крутим шар, ставим ровно
                    float rZ = transform.localRotation.z;
                    if ( Mathf.Abs( rZ ) > speedAlignRotation * 0.02f ) {
                        transform.Rotate(new Vector3(0.0f, 0.0f, (rZ > 0 ) ? -speedAlignRotation : speedAlignRotation));
                    } else {
                        //GetComponent<SpriteRenderer>().color = Color.yellow;
                        setState(BallState.STOP);
                        aligned = true;
                    }
                } break;
            case BallState.FLY_TO: { } break;
        }
    }
    
    // Ниже калбэк вызывающийся системой Unity при столкновений физических объектов между собой
    // В нём отслеживаются первые столкновения шаров, все кто подписан на наш шар, получают соответственные события
    // TODO шар после каждого удара, пытается выравнятся (возможно стоило бы добавить условие, на состояние покоя)
    int countCllisionWithBalls = 0; // количество столкновений об другой шар или же об челюсть дракона
    void OnCollisionEnter2D(Collision2D collision) {
        if (tag != "ReceivingTrayBall") return;
        //print("[OnCollisionEnter] #ball:"+number);
        foreach (ContactPoint2D contact in collision.contacts){
            //Debug.DrawRay(contact.point, contact.normal, Color.white);
            Ball ball = contact.collider.gameObject.GetComponent<Ball>();
            if (ball != null || contact.collider.gameObject.name == "DecorHead") {
                //prevVelocity[0] = GetComponent<Rigidbody2D>().velocity;
                if (countCllisionWithBalls++ == 0){ // если удар этого шара с другим шаром впервые!
                    if (callBack != null) callBack(this);
                } else
                    prevVelocity[0] = GetComponent<Rigidbody2D>().velocity * 0.4f; // умышленное уменьшение скорости в истории, для уменьшение звучания вторичных ударов шаров между собой
                
                var ba = SoundsSystem.play(Sound.S_BALL_KICK, transform.position);
                if (ba != null) { 
                    var aS = ba.GetComponent<AudioSource>();
                    aS.pitch = 0.5f + main.receivingTray.getCountBallsInTray() * 0.03f;
                    aS.volume = prevVelocity[0].magnitude / 10.0f;//getVolumeByState();
                }
                //print("Velocity Ball #" + number + " ==" + prevVelocity[0] + " magnitude:"+ prevVelocity[0].magnitude+ " VOLUME:"+ aS.volume);
                setState(BallState.ALIGNING);
                aligned = false;
            }
        }
        //audio.Play();
    }
}
