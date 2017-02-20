using System;
using System.Collections.Generic;
using UnityEngine;


namespace Api {

    public class CmdName {
        public const string Buy = "buy";
        public const string Play = "play";
        public const string Ball = "ball";
        public const string Auth = "auth";
        public const string Rooms = "rooms";
        public const string Templates = "templates";
        public const string Account = "account";
        public const string Market = "market";
        public const string Exchange = "exchange";
        public const string Register = "register";
        public const string Marathon = "marathon";
    }

    public enum ServerErrors {
         E_OK = 0,
         E_UNDEFINE = 1,        // ужастно неопределённая
         E_PENDING = 2,         // x
         E_TEMP_ERROR = 3,      // x
         E_ERROR = 4,           // неопределённая
         E_SESSION = 5,         // неизвестная ошибка сессии
         E_SESSION_ID = 6,      // неудалось преобразовать сессию в набор доступных данных (непонятная сессия вообще) чушь в общем :)
         E_SESSION_EXPIRED = 7, // сессия устарела или неверная (нужно сделать полную авторизацию)
         E_VERSION_ERROR = 101, // + // текущая версия не поддерживается сервером,   нужно обновиться
         E_REQUEST_PARAMS = 102,// в запросе какая-то лажа. 
         E_DB_ERROR = 103,      // ошибка базы данных, ничего не поделаешь
         E_DRAWING_END = 104,   //+ (выход в меню) // только в розыгрыше, игра уже окончина, а игрок пытается запросить что-то из законченой игры. + закончить текущий розыгрышь
         E_NOT_ENOUGH = 105,     //+недостаточно чего-то... золота, кристалов... 
         E_APP_NOT_FOUND = 106,  // appId не найден на сервере, необходимо заново запустить регистрацию
         E_SKIP_PURCHASE = 107,  // закончить тарнзакнию покупки из-за ошибки 
         E_MARATHON_LEVEL = 108,  // неверно указан уровень
         E_MARATHON_TIME = 109,  // еще не пришло время для выбачи награды
    }

    public enum ErrorType {
        Timeout = 1,        // ошибка связи
        Network = 2,        // ошибка клиента
    }

    public class Event {
        
        private string _name;

        #region Public Events

        public event EventHandler<RespondEventArgs> OnRespond;
        public event EventHandler<ErrorEventArgs> OnError;

        #endregion

        void debugPrint(string s) {
            MAIN.getMain.setMessage(s);
        }

        public void Respond(string payload) {
            if (payload == null) { debugPrint("[Respond] payload == null"); }
            if (OnRespond == null) { debugPrint("[Respond] OnRespond == null"); }
            WaitingServerAnsver.hide();
            OnRespond(this, new RespondEventArgs(payload));
        }

        public void Error(ErrorType type, string msg) {
            WaitingServerAnsver.hide(true);
            this.OnError(this, new ErrorEventArgs(type, msg));
            //Errors.showError(Errors.TypeError.ES_CONNECT_ERROR);
        }

        public Event(string name) {
            _name = name;
        }

        public string Name {
            get { return _name; }
        }
    }

    public class RespondEventArgs : EventArgs {
        private string _payload;

        public RespondEventArgs(string payload) {
            _payload = payload;
        }

        public string Payload {
            get { return _payload; }
        }
    }

    public class ErrorEventArgs : EventArgs {
        private ErrorType _type;
        private string _message;

        public ErrorEventArgs(ErrorType type, string message) {
            _type = type;
            _message = message;
        }

        public ErrorType Type {
            get { return _type; }
        }
        public string Message {
            get { return _message; }
        }
    }

    public struct EventCmd {
        
        private Dictionary<string, Event> _cmd;

        public Event GetApiEvent(string name) {
            try {
                return _cmd[name];
            } catch (KeyNotFoundException ex) {
                Debug.Log("Exception: " + ex);
                Debug.Log("StackTrace: " + ex.StackTrace);
                return null;
            }
        }

        public void Init() {
            _cmd = new Dictionary<string, Event>();
            _cmd.Add(CmdName.Buy, new Event(CmdName.Buy));
            _cmd.Add(CmdName.Play, new Event(CmdName.Play));
            _cmd.Add(CmdName.Ball, new Event(CmdName.Ball));
            _cmd.Add(CmdName.Auth, new Event(CmdName.Auth));
            _cmd.Add(CmdName.Rooms, new Event(CmdName.Rooms));
            _cmd.Add(CmdName.Templates, new Event(CmdName.Templates));
            _cmd.Add(CmdName.Exchange, new Event(CmdName.Exchange));
            _cmd.Add(CmdName.Market, new Event(CmdName.Market));
            _cmd.Add(CmdName.Account, new Event(CmdName.Account));
            _cmd.Add(CmdName.Register, new Event(CmdName.Register));
            _cmd.Add(CmdName.Marathon, new Event(CmdName.Marathon));
        }

    }

}