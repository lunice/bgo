using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Класс отвечающий за логику работы билета и его визуального отображения
public class Ticket : MonoBehaviour {
	public TicketCell tiledCell;   // █ префаб клеток (затайливает ими ячейки билетов). Они не рисуются, поскольку нарисованы в фоне. И при каких либо их маркировках создаются динамически
	public Vector2 indent = new Vector2(0.1f,0.1f); // отступ между ячейками
    public int number;                              // номер билета
    private MAIN main;                              // для удобного доступа
    private bool isInit = false;                    // инициализирован ли
    TicketCell[,] cells = new TicketCell[MAIN.ticketCountRows, MAIN.ticketCountColumns]; // █ масив клеток ( 5х5 )
    int[,] ticketNumbers = new int[MAIN.countTicketNumbersInColum + 1, MAIN.ticketCountColumns]; // масив этих же клеток но только номеров. Возникала необходимость для более удобной работы с этими клетками

    int countWinLines = 0;          // (не актуально) количество линий ( для старого режима одиночной )
    int totalCellsCount;            // общее количество клеток (всегда 25)
    public Transform cellsHolder;   // держатель клеток (создался в силу того что в билете появили визуальное отображение номеров, что нарушало количество детей в иерархии данного трансформа, потому пришлось создавать этот контейнер, для клеток)

    List<Template> winsTemplate = new List<Template>();     // список выиграшей в этом билете
    List<Template> prevWinsTemplate = new List<Template>(); // список превинов в этом билете

    void Awake(){
        cellsHolder = transform.FindChild("cellsHolder").transform;
        Movable m = GetComponent<Movable>();
        m.subscribeOnControllEvents(TicketsHolder.onTicketDragEvent);
    }
    void Start() {
        if (!isInit) {
            main = MAIN.getMain;
            totalCellsCount = MAIN.ticketCountRows * MAIN.ticketCountColumns;
            // для старого режима
            //if (main.gameMode == GameMode.CLIENT_GENERATE)
            //    testClientInit();   
            isInit = true;
        }
    }
    /*public int getCountWinLines() // использовалось при тестовых шаблонах в протипе этой игры
     * {
        return countWinLines;
    }*/
    public TicketCell getCellByPosition(int num) // получить клетку за её позицией на билете ( 0..24 )
    {
        return cellsHolder.GetChild(num).GetComponent<TicketCell>();;
    }
    public TicketCell getCellByNum(int num) // получить клетку за её индексом в масиве ( 0..24 )
    {
        //print(transform.childCount);
        for(int i=0; i < cellsHolder.childCount; i++) {
            TicketCell tC = cellsHolder.GetChild(i).GetComponent<TicketCell>();
            if (tC.numValue == num)
                return tC;
        }
        return null;
    }
    void newWinLine() //(не актуально) использовалось при тестовых шаблонах в протипе этой игры
    {
        countWinLines++;
    }
    
    public bool checkOnWin(TicketCell newCell) // ( недоделано и не актуально ) старая проверка на выиграшь при новом шаре, проверка выполняется, но нет никаких сообщений
    {
        /*switch (main.templetesFrom) {
            case TemplatesFrom.CLIENT_OLD: return checkOnWinOld(newCell);
            case TemplatesFrom.CLIENT_FABRIC: return checkOnWinCF(newCell);
            //case TemplatesFrom.SERVER: return checkNewBall();
        }*/
        if (main.gameMode == GameMode.SERVER) return false; 
        return checkOnWinOld(newCell);  // старая логика
        //print("Error! [checkOnWin] unknown TemplatesFrom:" + main.templetesFrom);
        //return false;
    }
    //================================[ wins logic ]==============================
    void markCells(int[] positions, TicketCell.TypeMark markType, int withOut = -1) // Маркировка клеток (withOut нужне для привинов, если он не -1 то клетка за указаным номером отмаркируется мигающим шаром)
    {
        for (int i = 0; i < positions.Length; i++)
            if (positions[i] != withOut && getCellByPosition(positions[i] - 1).markType != TicketCell.TypeMark.WIN)
                getCellByPosition(positions[i] - 1).mark(markType);
            else
                getCellByPosition(positions[i] - 1).mark(TicketCell.TypeMark.WITH_OUT);
    }
    /*bool checkPositionsOn(int[] positions,  params TicketCell.TypeMark[] typeCell){
        return false
    }*/
    bool checkPositionsOn(int[] positions, int withOut = -1, params TicketCell.TypeMark[] typeCell) // █ перероверка за сервером превинов и винов но не выводит никаких сообщений
    {
        for (int i = 0; i < positions.Length; i++)
            for(int j=0; j<typeCell.Length; j++)
                if (getCellByPosition(positions[i] - 1).markType == typeCell[j] &&
                    (withOut == -1 || positions[i] == withOut) )
                return true;
        return false;
    }
    public bool setPreWinIfReady(JsonHandler.PreWin preWin) // █ отмечание превина
    {
        if (preWin == null) return false;
        var positions = main.templatesHolder.getTicketPositionsByCategoryID(preWin.C, preWin.T);
        bool res = checkPositionsOn(positions, preWin.P, TicketCell.TypeMark.NONE);
        if (!res) markCells(positions, TicketCell.TypeMark.PREWIN, preWin.P);
        return !res;
    }
    public bool setWinIfReady(JsonHandler.Win win) // █ отмечания выиграша
    {
        if (win == null) return false;
        var positions = main.templatesHolder.getTicketPositionsByCategoryID(win.C, win.T);
        bool res = checkPositionsOn(positions, -1, TicketCell.TypeMark.NONE);
        if (!res) markCells(positions, TicketCell.TypeMark.WIN);
        return !res;
    }
    public void setPreWins(JsonHandler.PreWin[] preWins) // отмечание превина
    {
        if (preWins != null) { 
            for(int i=0; i<preWins.Length; i++)
                markCells( main.templatesHolder.getTicketPositionsByCategoryID(preWins[i].C,preWins[i].T), TicketCell.TypeMark.PREWIN, preWins[i].P );
            SoundsSystem.play(Sound.S_PRE_WIN);
        }
    }
    public void setWins(JsonHandler.Win[] wins) //отмечания выиграша
    {
        if (wins != null){
            for (int i = 0; i < wins.Length; i++)
                markCells(main.templatesHolder.getTicketPositionsByCategoryID(wins[i].C, wins[i].T), TicketCell.TypeMark.WIN);
        }
    }
    public void setPreWinsTest() // (отключено) для теста
    {
        int[] test = { 1,2,3,4,5 };
        markCells(test, TicketCell.TypeMark.WIN, 5);
    }
    /*bool isHaveTemplateInList(List<Template> lT, Template t) {
        return isHaveTemplateInList(lT, t, t.currentVariable);
    }*/
    bool isHaveTemplateInList(List<Template> lT, Template t, int variableNum) // (недоделано, отключено и не актуально - делалось для client_fabric режима одиночной игры) Проверяет есть ли указанный шаблон в указанном билете, с вариациями. 
    {
        for(int i = 0; i < lT.Count; i++ ) {
            /*if (lT[i].name == t.name && variableNum == t.currentVariable) {
                return true;
            }*/
        }
        return false;
    }
    xy getCoordsByTicketCell(TicketCell tCell) // Возвращает позиции клекти
    {
        for (int i = 0; i < MAIN.ticketCountRows; i++)
            for (int j = 0; j < MAIN.ticketCountColumns; j++)
                if (cells[i, j] == tCell)
                    return new global::xy(i,j);
        print("Error! [getCoordsByTicketCell] coords cell#" + tCell.name + " not find!!!");
        return new global::xy(-1, -1);
    }
    //==============================[ client  fabric ]================================
    bool checkOnWinCF(TicketCell newCell ) // (отключено, недоделано и неактуально)
    {
        /*List<Template> templates = main.templatesHolder.templates;
        for(int i=0; i< templates.Count; i++) {
            Template template = templates[i];
            for(int j=0; j<template.variables.Length; j++) {
                //xy[] coordsCells = getCoordsByTicketCell(newCell);
                //getVariableCellsByCell( new xy(newCell) )
                //Template win = template()
            }
        }*/
        return false;
    }
    //=============================[ Old client check ]================================
    public bool checkOnWinOld(TicketCell newCell) // (отключено и неактуально) старая логика протоипа игры, проверка на наличие простых захаркодженных шаблонов 
    {
        int ii = 0;
        int jj = 0;
        bool res = false;
        for (int i = 0; i < MAIN.ticketCountRows; i++)
            for (int j = 0; j < MAIN.ticketCountColumns; j++)
                if (cells[i, j] == newCell) {
                    ii = i; jj = j; break;
                }

        bool line = true;
        for (int i = 0; i < MAIN.ticketCountRows; i++)
            if (!cells[i, jj].isMarked) {
                line = false;
                break;
            }
        if (line) {
            newWinLine();
            res = true;
        }

        line = true;
        for (int j = 0; j < MAIN.ticketCountColumns; j++)
            if (!cells[ii, j].isMarked) { 
                line = false;
                break;
            }
        if (line) {
            newWinLine();
            res = true;
        }


        line = true;
        if (MAIN.ticketCountColumns == MAIN.ticketCountRows && ii == jj)
        {
            for (int i = 0; i < MAIN.ticketCountColumns; i++)
                if (!cells[i, i].isMarked){
                    line = false;
                }
            if (line) {
                newWinLine();
                res = true;
            }

        }

        line = true;
        if (MAIN.ticketCountColumns == MAIN.ticketCountRows && ii == (MAIN.ticketCountRows - 1 ) - jj) {
            for (int i = 0; i < MAIN.ticketCountColumns; i++) { 
                if (!cells[i, (MAIN.ticketCountRows - 1) - i].isMarked) {
                    line = false;
                    break;
                }
                int a = (MAIN.ticketCountRows - 1) - i;
                //print("i:" + i + ", j:" + a);
            }
            if (line) {
                newWinLine();
                res = true;
            }
        }
        return res;
    }
    // =============================[ GENERATE TICKET LOGIC ]==================
    // █ (ПОДКЛЮЧЕНО!) это система генерации билета с рандомными значениями клеток, позже рандомная генерация была отключена,
    // и подключена трансляция поклеточно из структуры initWithJsonStruct (при чтении из файла или из данных от сервера)
    // █ генератор перепроверяет эти данные, в случае ошибки может быть непредвиденный креш, смотреть последнюю функцию!
    private Vector2 p; // (сугубо технического назначения переменная)
    bool beforeGenerateInit() // Пред инициализация до генерации билета
    {
        if (!tiledCell) {
            print("Error![generateTicket] tiledCell not defined");
            return false;
        }
        resetTicketNumbers();
        p.x = indent.x * (MAIN.ticketCountRows - 1) * -0.5f;
        p.y = indent.y * (MAIN.ticketCountColumns - 1) * -0.5f;
        return true;
    }
    public bool initWithJsonStruct(JsonHandler.TicketJSON jsonTicket) // Преобразование JSON структуры, и инициализация полученных данных
    {
        Start();
        if (!beforeGenerateInit()) return false;
        int currentCell = 0;
        number = jsonTicket.N;
        if(jsonTicket.B == null) {
            print("Error! [] in jsonTicket:"+jsonTicket.N + " not init cells!");
            return false;
        }
        int countCells = jsonTicket.B.Length;
        if ( countCells != totalCellsCount) {
            print("Error![initWithJsonStruct] rows(" + MAIN.ticketCountRows + ") and columns(" + MAIN.ticketCountColumns + ") do not coincide with count cells in json ticket(" + countCells + ")");
            return false;
        }
        for (int j = MAIN.ticketCountColumns - 1; j >= 0 ; j--){
            for (int i = 0; i < MAIN.ticketCountRows; i++) {
                TicketCell newCell = Instantiate(tiledCell);
                newCell.transform.parent = cellsHolder.transform;
                newCell.transform.localPosition = new Vector3(p.x + i * indent.x, p.y + j * indent.y);
                newCell.numValue = takeAwayCellNumber(i,jsonTicket.B[currentCell++]);
                cells[i, j] = newCell;
            }
        }
        isInit = true;
        return true;
    }
    /*bool testClientInit(){
        generateTicket();
        isInit = true;
        return isInit;
    }*/
    /*int getRandomWithOutNumbers(int[] n) // генерирует номера билетов
    {
        int countIter = 0;
        for (;;) {
            int res = Random.Range(1,totalCellsCount);
            bool bRes = true;
            for (int i = 0; i < n.Length; i++) if (res == n[i]) bRes = false;
            if (bRes) return res;
            if (countIter++ > 1000) {
                print("Error![getRandomWithOutNumbers] countIter == 1000!");
                return 0;
            }
        }
    }*/
    /*void generateTicket() // генерирует номера билетов
     {
        int[] horseshoes = new int[totalCellsCount];
        for(int i=0; i< main.countHorseshoeInTicket;i++)
            horseshoes[i] = 0;
        for (int i = 0; i < main.countHorseshoeInTicket; i++)
            horseshoes[i] = getRandomWithOutNumbers(horseshoes);
        //main.countHorseshoeInTicket;
        if (!beforeGenerateInit()) return;
        for (int i = 0; i < MAIN.ticketCountRows; i++){
            for (int j = 0; j < MAIN.ticketCountColumns; j++) {
                TicketCell newCell = Instantiate(tiledCell);
                newCell.transform.parent = cellsHolder.transform;
                newCell.transform.localPosition = new Vector3(p.x + i * indent.x, p.y + j * indent.y);
                //newCell.numValue = Random.Range(1, 75);
                newCell.numValue = takeAwayTicketNumber(i);
                cells[i, j] = newCell;
                // ------------[Horseshoe]------------
                for(int k = 0; k < main.countHorseshoeInTicket; k++) {
                    if (horseshoes[k] == newCell.numValue) {
                        newCell.setHorseshoe(true);
                    }
                }
            }
        }
    }*/
    public void resetTicketNumbers() // сбрасывает закешированые номера билетов, которые были отмеченны функцией ниже, необходимо при вторичных использованиях объектов... (но по моему, сей час, они на каждый розыгрыш удаляются)
    {
        for (int i = 0; i < MAIN.ticketCountColumns; i++) {
            for (int j = 0; j < MAIN.countTicketNumbersInColum; j++) { // rows
                ticketNumbers[j, i] = (j + 1) + i * MAIN.countTicketNumbersInColum;
            }
            ticketNumbers[MAIN.countTicketNumbersInColum, i] = MAIN.countTicketNumbersInColum;
            //Debug.Log("ticketNumbers[" + countTicketNumbersInColum + "," + i + "] == " + countTicketNumbersInColum);
        }
    }
    int takeAwayCellNumber(int columnNum, int numBall = 0) // ███(подключено) (дословно: изъять доступный номер клетки) при генерации билетов, отмечает указанный номер и недопускает его повторного создания, в режиме игры: SERVER перепроверяет правильность билетов, в случае ошибки -- не предвиденная ситуация
    {
        int curCountNumbers = ticketNumbers[MAIN.countTicketNumbersInColum, columnNum];
        int numRow = -1;
        if (numBall == 0)
            numRow = Random.Range(0, curCountNumbers - 1);
        else {
            for (int i = 0; i < curCountNumbers; i++) {
                if (ticketNumbers[i, columnNum] == numBall)
                    numRow = i;
            }
            if ( numRow == -1) { // TODO MAIN
                print("Error! [takeAwayTicketNumber] the ball №" + numBall + " in colum:" + columnNum + " already taken");
                return 0; 
            }
        }
        int res = ticketNumbers[numRow, columnNum];
        ticketNumbers[numRow, columnNum] = ticketNumbers[curCountNumbers - 1, columnNum];
        curCountNumbers = ticketNumbers[MAIN.countTicketNumbersInColum, columnNum]--;
        return res;
    }
}
