using UnityEngine;
using System.Collections;
// Класс, контейнер, содержащий в себе билеты
public class TicketsHolder : MonoBehaviour {
    MAIN main = MAIN.getMain;
    public Ticket ticketPrefab;     // префаб билета
    bool isNeedSetTickets = false;  // нужно ли устанавливать билеты ( иначе говоря пустой ли контейнер )
    float ticketWidth;             // ширина билета!
    float halfTicketWidth;
    float startPosX;                // изначально установленная позиция по X
    // инициализация
    void Awake() {
        startPosX = transform.position.x;
        main.ticketHolder = this;
        if (ticketPrefab)
            main.ticketPrefab = ticketPrefab;
        else print("Error [Awake] ticketPrefab not defined");
    }
    void Start() {}
    // вывод состояния некоторых переменных и объектов вызывается из Raffle
    public void testPrint(){
        main.setMessage("TicketHolder:");
        main.setMessage("  --ballsWave:" + ballsWave);
    }
    // установить (визуально создаются) биллеты из списка JSON (полученых из сервера)
    public void setTickets(JsonHandler.TicketJSON[] jsonTickets){
        transform.position = new Vector2(startPosX, transform.position.y);
        restoreTicketsPosXTo = 0.0f;
        ballsWave = null;
        Rooms.countTickets = jsonTickets.Length;
        if (Rooms.countTickets == 0 ) {
            print("Error![setTickets] ticketsCount == 0");
            return;
        }

        if (transform.childCount > 0) removeAllTickets();

        for(int i = 0; i < Rooms.countTickets; i++) {
            Ticket ticket = GameObject.Instantiate(ticketPrefab) as Ticket;
            if (!ticket.initWithJsonStruct(jsonTickets[i])) {
                print("Error! [setTickets] fail read json ticket");
            }
            ticket.number = jsonTickets[i].N;
            ticket.name = i.ToString();
            Transform ticketT = ticket.transform;
            ticketT.parent = transform;
            ticketT.localScale = new Vector3(main.ticketScale, main.ticketScale, 1.0f);
            SpriteRenderer sr = ticketT.GetComponent<SpriteRenderer>();
            ticketWidth = (sr.sprite.texture.width * 0.01f-0.3f) * ticketT.localScale.x;
            halfTicketWidth = ticketWidth * 0.5f;
        }
        alignTicketsPos(); // после выравнивание
    }
    // (не используется, ранее использовалось в одиночной игре, где можно было докупать новый билет, клиент генерил его сходу) создать новый билет
    public void createNewTicket() {
        if ( ballsWave != null ) { ballsWave.stop(); }
        if (Rooms.countTickets < transform.childCount) return;
        switch (main.gameMode) {
            case GameMode.CLIENT_GENERATE: {
                    _createNewTicket();
                    if (transform.childCount + 1 == Rooms.countTickets)
                        main.setEnableBtn("BuyTicket", false);
                    return;
                } break;
            case GameMode.JSON_FILE: setTickets(main.jsonHandler.getTickets()); break;
            case GameMode.SERVER: {
                    if (!main.isWaitingReplyAboutRaffleFromServer)
                        setTickets(main.handlerServerData.getTickets());
                    else
                        isNeedSetTickets = true;
            }; break;
        }
        main.setEnableBtn("BuyTicket", false);
    }
    // получить билет за порядковым новером в держателе
    public Ticket getTicket(int number) {
        var tickets = getTickets();
        for (int i = 0; i < tickets.Length; i++)
            if (tickets[i].number == number)
                return tickets[i];
        Errors.showTest("Error! [getTicket] ticket with number #" + number + " not find!");
        return null;
    }
    // внутренняя функция создания нового билета
    void _createNewTicket() {
        Ticket ticket = GameObject.Instantiate(ticketPrefab) as Ticket;
        Transform ticketT = ticket.transform;
        ticketT.parent = transform;
        ticketT.localScale = new Vector3(main.ticketScale * transform.localScale.x, main.ticketScale * transform.localScale.y, 1.0f);
        SpriteRenderer sr = ticketT.GetComponent<SpriteRenderer>();
        //print("-----------------"+sr.sprite.texture.width);
        ticketWidth = sr.sprite.texture.width * 0.01f * ticketT.localScale.x;
        alignTicketsPos();
    }
    // выравнивание позиций билетов
    void alignTicketsPos() {
        int ticketsCount = transform.childCount;
        float totalWidth = ticketsCount * ticketWidth;
        float sLeft = -totalWidth * 0.5f + ticketWidth * 0.5f;
        for (int i = 0; i < ticketsCount; i++) {
            Transform t = transform.GetChild(i);
            t.localPosition = new Vector3(sLeft + i * ticketWidth, 0.0f, 0.0f);
        }
    }
    // удаление билетов
    public void removeAllTickets() {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy( transform.GetChild(i).GetComponent<Ticket>().gameObject );
        }
    }
    // получить все билеты
    public Ticket[] getTickets() {
        Ticket[] res = new Ticket[Rooms.countTickets];
        for(int i = 0; i < transform.childCount; i++){
            if (transform != null)
            {
                var ri = transform.GetChild(i).GetComponent<Ticket>();
                res[i] = ri;
            }
            else Errors.showTest("Ага! Ошибка");
        }
        return res;
    }
    // █ здесь начинается вызов цепи функций, что визуально прячет шары на билетах, и переворачивает сами билеты
    // на первом этапе, запускается сокрытие превинов, по окончанию запускает волна сокрытия всех шаров. В функции ниже...
    public static void startHideTickets() {
        MAIN.getMain.templatesHolder.expectedWin.setValue(0);
        // AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/CoinDrop"), Vector3.zero); // #V тестовая строчка для проиграша указаного звука
        var th = MAIN.getMain.ticketHolder;
        GameObject[] mBalls  = GameObject.FindGameObjectsWithTag("missingBall");
        GameObject[] aureols = GameObject.FindGameObjectsWithTag("aureols");
        for (int i=0; i<mBalls.Length; i++) {
            var tc = mBalls[i].GetComponent<TicketCell>();
            tc.stopPlayPrewin();
        }
        FadeEffect fe = null;
        for (int i = 0; i < aureols.Length; i++) fe = Effects.addFade(aureols[i], 2.5f); // #V конец розыгрыша скорость исчезновения ареолов
        if (fe != null) fe.subscribeOnEffectDone(onAureolsHide);
        else onAureolsHide(null);
    }
    // запускает эффект волну которая прячет шары, после окончания вызывается через калл бэк, функция ниже, которая запускает волну переворота билетов
    static SimpleWaveEffect ballsWave;   // переменная которая хранит в себе эту волну
    static void onAureolsHide(BaseEffect effect){
        GameObject[] tBalls = GameObject.FindGameObjectsWithTag("ticketBall");
        ballsWave = Effects.addSimpleWave(new Vector2(0.0f, 0.0f), tBalls, onWaveGameObject, 3f, 8.0f); // V# запуск волны
        ballsWave.subscribeOnEffectDone(onWaveDone);
        SoundsSystem.play(Sound.S_DISAPEAR);
    }
    // По окнончанию волны сокрытия шаров, здесь запускается функция переворотов билетов и их уменьшения в маштабе
    static void onWaveDone(BaseEffect effect){
        //print("█ [onWaveDone]");
        var tickets = MAIN.getMain.ticketHolder.getTickets();
        if (tickets == null || tickets.Length == 0 ) return;
        //print("tickets.Length:" + tickets.Length);
        GameObject[] go = new GameObject[tickets.Length];
        for (int i = 0; i < tickets.Length; i++)
            if (tickets[i] != null && tickets[i].gameObject != null)
                go[i] = tickets[i].gameObject;
            else return;
        Effects.addSimpleWave(tickets[0].gameObject, go, onWaveTicket, 0.5f, 12.0f);
        ballsWave = null;
    }
    // При прохождении волны по текущему билету, начинает проигрывать анимацию и маштабирование
    static void onWaveTicket(GameObject waveGO) {
        Animator anim = waveGO.GetComponent<Animator>();
        if (anim != null){
            anim.enabled = true;
            SoundsSystem.play(Sound.S_TICKET_TURN, waveGO.transform.position);
        }

        var s = Scaling.set(waveGO, 0.85f, 0.8f, 1);
        s.setWithOutReturedAnim();

        for (int i = 0; i < waveGO.transform.childCount; i++) { 
            waveGO.transform.GetChild(i).gameObject.SetActive(false);
            //var cs = Scaling.set(waveGO.transform.GetChild(i).gameObject, new Vector3(0.0f, 1.0f, 1.0f), 0.5f, 1);
            //cs.setWithOutReturedAnim();
        }
    }
    // При прохождении волны по текущему шару запускает сокрытие и его удаление после
    static void onWaveGameObject(GameObject waveGO){
        Effects.addFade(waveGO, 0.25f); // затухание зелёных и золотистых шаров
    }
    public float restoreTicketsPosXTo = 0.0f;
    float speedRestorePosX;
    float minSpeedRestorePosX = 0.25f;
    //float indent = -1.8f;
    public static void onTicketDragEvent(BaseController btn, BaseController.TypeEvent type) {
        //print("█ onTicketDragEvent == " + type);
        if (type == BaseController.TypeEvent.ON_MOUSE_UP) {
            var th = MAIN.getMain.ticketHolder;
            var hud = HUD.getHUD;
            //print(th.transform.position.x);
            //print("hud.left.position.x: " + hud.left.position.x);
            //print("hud.right.position.x: " + hud.right.position.x);
            //print("th.ticketWidth == " + th.ticketWidth);
            int ticketsCount = th.transform.childCount;
            float totalHalfWidth = ticketsCount * th.halfTicketWidth;
            if (th.transform.position.x < hud.left.position.x + th.halfTicketWidth - totalHalfWidth) th.restoreTicketsPosXTo = hud.left.position.x + th.halfTicketWidth - th.transform.position.x- totalHalfWidth;
            else if (th.transform.position.x > hud.right.position.x - th.halfTicketWidth + totalHalfWidth) th.restoreTicketsPosXTo = hud.right.position.x - th.halfTicketWidth - th.transform.position.x+ totalHalfWidth;
            //print("█ th.restoreTicketsPosXTo == " + th.restoreTicketsPosXTo);
        } else if (type == BaseController.TypeEvent.ON_MOUSE_DOWN)
            MAIN.getMain.ticketHolder.restoreTicketsPosXTo = 0.0f;
    }
    // █ Здесь постоянно проходит опрос по main.handlerServerData.getTickets() на пришедшую информацию от сервера, лучше было бы сделать соответсвтенное событие, по которому и произведётся установка билетов... 
    
    void Update () {
        if (restoreTicketsPosXTo != 0.0f) {
            speedRestorePosX = Mathf.Abs(restoreTicketsPosXTo) * 0.25f;
            if (speedRestorePosX < minSpeedRestorePosX) speedRestorePosX = minSpeedRestorePosX;
            if ( restoreTicketsPosXTo > 0) {
                if (restoreTicketsPosXTo >= speedRestorePosX ){
                    restoreTicketsPosXTo -= speedRestorePosX;
                    transform.position += new Vector3(speedRestorePosX, 0.0f, 0.0f);
                } else {
                    transform.position += new Vector3(restoreTicketsPosXTo, 0.0f, 0.0f);
                    restoreTicketsPosXTo = 0.0f;
                }
            } else {
                if (restoreTicketsPosXTo <= speedRestorePosX) {
                    restoreTicketsPosXTo += speedRestorePosX;
                    transform.position += new Vector3(-speedRestorePosX, 0.0f, 0.0f);
                } else {
                    transform.position += new Vector3(-restoreTicketsPosXTo, 0.0f, 0.0f);
                    restoreTicketsPosXTo = 0.0f;
                }
            }
            
        }
        if (isNeedSetTickets && !main.isWaitingReplyAboutRaffleFromServer) { 
            setTickets(main.handlerServerData.getTickets());
            isNeedSetTickets = false;
        }
    }
}
