using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Room    // перечисление всех комнат должно быть здесь
{
    FIRST = 0,      // здесь перечень комнат
}

// █ ( Класс практически не используется, но многие части подключены ) был набросан, с учётом множетсва комнат
public class Rooms {
    //TemplateData[] serverTemplates = null;
    public TemplateData serverTemplates = null; // █ шаблоны комнатЫ! одной, нужно при добавлении множества комнат, сделать список этих шаблонов, и в зависимости от текущей комнаты, через Get / Set этой переменной, организовать доступ к соответственной комнате из списка
    static Rooms _rooms;        // держатель этого синглтона
    public static Rooms get {
        get {
            if (_rooms == null) {
                _rooms = new Rooms();
                _rooms.init();
            }
            return _rooms;
        }
    }
    //bool isInit = false;
    bool init(){
        ScenesController.getScenesController.subscribeOnLoadScene(onLoadNewScene);
        //isInit = true;
        return true;
    }
    //public Dictionary<Room,RoomInfo> rooms = new Dictionary<Room, RoomInfo>();
    public RoomInfo[] rooms;                    // список доступных комнат (должны получать от сервера) █ // здесь должна быть мапа комнат
    public static int currentIndexRoom = -1;    // индекс списка указывающий на текущую комнату ( -1 значит отсутствует )
    public static int countTickets;             // █ количество доступных билетов (относительно текущей комнаты)
    // возвращает текущую комнату
    public static RoomInfo currentRoom {
        get {
            var rooms = Rooms.get.getRooms();
            if (rooms != null && currentIndexRoom >= 0 && 
                currentIndexRoom < rooms.Length) return rooms[currentIndexRoom];
            return null;
        }
    }
    // ( недоделано ) Выбирает новую комнату
    public static bool chooseRoom(string name){
        switch(name){
            case "playBtn": { currentIndexRoom = 0; } return true;
            default: Errors.showTest("неизвестная комната под кнопкой:\"" + name + "\""); break;
        }
        return false;
    }
    // получить весь список комнат
    public RoomInfo[] getRooms() { return rooms; }
    // ( недоделанно ) при загрузке новой сцены, а именно лобби, предполагалась инициализация, кнопок, которые из них доступные, которые нет.
    void onLoadNewScene(GameScene newGameScene){
        if (newGameScene == GameScene.MAIN_MENU) {
            currentIndexRoom = -1;
        }
    }
    // ( подключено и вызывается из Autorization ) помещаются доступные комнаты в свой внутренний список комнат
    public static void setNewRoomsData(RoomsData loadingRooms){
        var roomsData = Rooms.get;      // пустая операция, для создания синглтона
        roomsData.rooms = loadingRooms.Room;
    }
}
