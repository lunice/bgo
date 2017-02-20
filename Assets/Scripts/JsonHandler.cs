using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
// █ Класс для работы с JSON структурами, много функционала предназначалось для тестовой работой с подгрузкой розыгрышей из файла
// █ Который работает только для режимов JSON_FILE или JSON_FILE_IN_ANDROID
// █ Испольузется только ниже описаные структуры.
public class JsonHandler : MonoBehaviour {
    [System.Serializable]
    public class PaysJSON {
        public int C;  // current_ball
        public int N;  // newx_ball
    }

    [System.Serializable]
    public class BallJSON {
        public int N; // numberBall
        public WinTickets[] T; // wintickets
    }

    [System.Serializable]
    public class Win {
        public int C;   // num category
        public int T;   // num template
        public int W;   // reward
        public int S;   // stars
    }

    [System.Serializable]
    public class PreWin {
        public int C;   // num category
        public int T;   // num template
        public int W;   // reward
        public int P;   // prewin num
    }

    [System.Serializable]
    public class WinTickets {
        public int N;//T; // numberTicket
        public Win[] W;
        public PreWin[] P;
    }

    [System.Serializable]
    public class TicketJSON // T
    {
        public int[] B; // NumBalls
        public int N;   // TicketNumber
    }

    [System.Serializable]
    public class RaffleJSON{
        public BallJSON[] B;    // Ball
        public TicketJSON[] T;  // Ticket
        public PaysJSON P;      // Price
        public int W;           // Win
        public int S;           // Stars
    }

    RaffleJSON[] raffleIterations;  // (не актуально) масивы розыгрышей, прочтёных из файла
    RaffleJSON currentRaffle;       // текущий розыгрышь
    int currentRaffleNum;           // номер текущего розыгрша из списка
    string[] raffleLines;           // масив розыгрыша в виде неразпарсенной строки

    List<string> _raffleLines;      // этот же список в другом виде
    int currentLine = 1;            // текущая линия
    bool isLoadJSONFile = false;    // ранее служила для подгрузки розыгрышей из файла, когда не работал сервер. Для разработки иных частей игры, при не работающем сервере...
	//StreamReader streamReader;      // необходимая переменная для парсинга

    void Awake()  {
        MAIN.getMain.jsonHandler = this; // многие основные системыне модули при инициализации прописывали(ют) себя в класе MAIN
    }

    void Start () { init(); }
	
    public void loadRaffleLine(string jsonLine) // Загрузка JSON розыгрыша из строки
    {
        _raffleLines.Add(jsonLine);
    }
    bool loadJSONFile() // (не актуально) загрузка JSON розыгрышей из файла
    {
        if (isLoadJSONFile) return true;
        MAIN main = MAIN.getMain;
        try
        {
            using (StreamReader sr = new StreamReader(Application.dataPath + "/Resources/combinations.txt") ) {
                string line;
                _raffleLines = new List<string>();
                while ((line = sr.ReadLine()) != null) {
                    //print(line);
                    _raffleLines.Add(line);
                }
                //Errors.show("_raffleLines.Count == " + _raffleLines.Count.ToString().ToString());
                //print("_raffleLines.Count == " + _raffleLines.Count.ToString().ToString());
            }
        } catch (System.IO.IOException e) {
            Errors.showTest(e.Message);
            print("The file could not be read:");
            print(e.Message);
        }
        /*
        streamReader = new StreamReader(MAIN.jsonFileFilePath);
        _raffleLines = new List<string>();
        while (!streamReader.EndOfStream)
        {
            string s = streamReader.ReadLine();
            if (s.Length > 2 && s[2] == 'b')
            {
                _raffleLines.Add(s);
            }
        }*/
        
        raffleLines = new string[_raffleLines.Count];
        for (int i = 0; i < _raffleLines.Count; i++)
            raffleLines[i] = _raffleLines[i];
            
        loadNextRaffle();
        isLoadJSONFile = true;
        return true;
    }
    bool loadFromJSONFileInAndroid() // (рабочее но не актуально)
    {
        MAIN main = MAIN.getMain;
        return false;               // █ заглушка!

        StreamReader sr = new StreamReader(Application.dataPath + "/Resources/combinations.txt");
        string line;
        _raffleLines = new List<string>();
        while ((line = sr.ReadLine()) != null) {
            _raffleLines.Add(line);
        }
        loadNextRaffle();
        return true;
    }
    bool init() // █ работает только для режимов JSON_FILE или JSON_FILE_IN_ANDROID
    {
        MAIN main = MAIN.getMain;
        if ( main.gameMode == GameMode.JSON_FILE ) {
            isLoadJSONFile = loadJSONFile();
            //isInit = loadFromJSONFileInAndroid();
        } else if (main.gameMode == GameMode.JSON_FILE_IN_ANDROID ) {
            isLoadJSONFile = loadFromJSONFileInAndroid();
        } else {
            //print("Error! [init] unknown GameMode == " + main.gameMode);
            return false;       // █ выход
        }
        return isLoadJSONFile;
    }
    string[] getRaffleIterationsFromStringLine(string rafflesIterationsLine) // █ велосипед... (не актуально) своя логика чтения JSON структуры
    {
        if ( rafflesIterationsLine == "" ) {
            print("Error![getRaffleIterationsFromStringLine] rafflesIterationsLine == \" \"");
        } else if (rafflesIterationsLine[0] == '[' ) {
            int currentLevel = 0;
            int startPosCopy = 0;
            int len = 0;
            List<string> lines = new List<string>();
            for(int i = 0; i < rafflesIterationsLine.Length; i++) {
                if (rafflesIterationsLine[i] == '{' ) {
                    if (currentLevel == 0) {
                        len = 0;
                        startPosCopy = i;
                    }
                    currentLevel++;
                } else if (rafflesIterationsLine[i] == '}') {
                    if (currentLevel == 1) {
                        //string newLine = rafflesIterationsLine.Substring(startPosCopy, len+1);
                        //print(newLine);
                        lines.Add(rafflesIterationsLine.Substring(startPosCopy, len+1));

                    }
                    currentLevel--;
                } //else if (rafflesIterationsLine[i] == ',') {}
                len++;
            }
            string[] res = new string[lines.Count];
            for (int j = 0; j < lines.Count; j++)
                res[j] = lines[j];
            return res;
        } else if (rafflesIterationsLine[0] == '{') {
            return new string[] { rafflesIterationsLine };
        } else {
            print("Error! [getRaffleIterationsFromStringLine] unknown raffle line: \""+ rafflesIterationsLine + "\"");
        }
        return new string[0];
    }
    bool loadRaffleByNum(int num) // (для розыгрышы из файла) парсинг строки по укзаному номеру
    {
        try {
            string[] raffleIterationsLines = getRaffleIterationsFromStringLine(raffleLines[num]);
            raffleIterations = new RaffleJSON[raffleIterationsLines.Length];
            for (int i=0; i< raffleIterationsLines.Length; i++) {
                raffleIterations[i] = jsonParce(raffleIterationsLines[i]);
            }
            currentRaffleNum = 0;
            currentRaffle = raffleIterations[currentRaffleNum];
            currentLine = num;
        }
        catch (System.IO.IOException e){
            Debug.Log("Exception: " + e);
            Debug.Log("StackTrace: " + e.StackTrace);
            print("Error reading from {0}. Message = {1}"+e.Message);
            print("line[" + num + "]: " + raffleLines[num]);
            return false;
        }
        finally {
            //print("succses:" + currentRaffle.tickets.Length);
        }
        return true;
    }

    public TicketJSON[] getTickets() // (для розыгрышы из файла) получить билеты, текущего розыгрыша 
    {
        try {
            for(int i = 0; i<currentRaffle.T.Length; i++) {
                print(i);
                string s = "Cells:";
                if (currentRaffle.T[i].B != null)
                    for (int j = 0; j < currentRaffle.T[i].B.Length; j++)
                        s += currentRaffle.T[i].B[j]+" ";
                print(s);
            }
        } catch (System.IO.IOException e){
            Debug.Log("Exception: " + e);
            Debug.Log("StackTrace: " + e.StackTrace);
        }
        return currentRaffle.T;
    }
    public int getNumberBall(int num) // (для розыгрышы из файла) получить шар за порядковым номером, текущего розыгрыша 
    {
        return currentRaffle.B[num].N;
    }
    public bool getNextRuffle() // (для розыгрышы из файла) загрузить следущющий розыгрышь 
    {
        if (raffleLines == null) {
            print("[getNextRuffle] raffleLines == null");
            return false;
        }
        while (currentLine < raffleLines.Length ) {
            if (loadNextRaffle()) return true;
        }
        return false;
    }
    bool loadNextRaffle() // (для розыгрышы из файла) загрузить следущющий розыгрышь 
    {
        return loadRaffleByNum(++currentLine);
    }
    public void loadServerRaffleFromFile(int numRaffle) // (для розыгрышы из файла) загрузить розыгрышь за номером из списка строк в файле
    {
        print("l===========oadServerRaffleFromFile");
        loadJSONFile();
        MAIN main = MAIN.getMain;
        main.changeNameBtnOn("Restart", "Start");
        main.setEnableBtn("Start", true);
        main.setEnableBtn("BuyTicket", true);
        main.onBuyTicket();
        MAIN.getMain.handlerServerData.setRaffle( JsonUtility.FromJson<RaffleJSON>(raffleLines[numRaffle]) );
    }
    public void loadServerTempaltesFromFile() // (не рабочее) загрузить серверные шаблоны из файла, уже точно не помню что это, но похоже была идея/попытка заранее сделать систему кэширования в файл полученых от сервера шаблонов
    {
        loadJSONFile();
        //MAIN.getMain.templatesHolder.setServerTemplates(JsonUtility.FromJson<TemplateData>(raffleLines[0]) );
    }
    //============================[ PARCE LOGIC ]============================
    List<string> listStrs = new List<string>();
    string[] getSringsOfStructsInArrays(string str) {
        string[] resStr;
        int countStrs = 0;
        listStrs.Clear();
        int startTicketsParceFrom = -1;
        int lenStr = 0;
        int _countOpen = 0;

        for (int i = 0; i < str.Length; i++)
        {
            if (i != str.Length - 1 && str[i] == '{') {
                _countOpen++;
                if (_countOpen == 2 && startTicketsParceFrom == -1)
                    startTicketsParceFrom = i; // если количество 2 начинаем считать, сюда не заходим.               
            }

            if (startTicketsParceFrom!=-1)
                lenStr++;

            if (str[i] == '}') {
                if (_countOpen == 2 && startTicketsParceFrom != -1) {
                    listStrs.Add(str.Substring(startTicketsParceFrom, lenStr));
                    countStrs++;
                    lenStr = 0;
                    startTicketsParceFrom = -1;
                }
                _countOpen--;
            }
            
        }

        if (listStrs.Count > 0)
        {
            resStr = new string[listStrs.Count];
            for (int i = 0; i < listStrs.Count; i++)
            {
                resStr[i] = listStrs[i];
                //print("str["+ i + "]: "+listStrs[i]);
            }
        } else return new string[0];
        return resStr;
    }
    RaffleJSON jsonParce(string str){
        RaffleJSON res = JsonUtility.FromJson<RaffleJSON>(str);
        return res;
    }
}
