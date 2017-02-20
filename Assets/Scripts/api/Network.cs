using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum NetworkStatus {
    Error = 1,
    Ok = 2,
    InProgress = 3,
}

class Exchanger {

	protected LinkedList<Api.Message> _queue = new LinkedList<Api.Message>();

	public bool IsReady() {
		return _queue.Count > 0 ? true : false;
	}

	public bool HasMessage(Api.Message msg) {
		return _queue.Contains(msg);
	}

	public bool ReleaseMessage(Api.Message msg) {
        var node = _queue.Find(msg);
        if (node != null) {
            node.Value.Dirty = true;
            return true;
        }
        return false;
	}

	public void Enqueue(Api.Message msg) {
		_queue.AddLast(msg);
	}

	public void Dequeue() {
		_queue.RemoveFirst();
	}

	public Api.Message currentMsg {
		get { return _queue.First.Value; }
	}

}

class Reader {

	private Queue<Api.Message> _queue = new Queue<Api.Message>();

	public Api.Message Queue {
		get { return _queue.Dequeue(); }
		set { _queue.Enqueue(value); }
	}

	public bool isReady() {
		return _queue.Count > 0 ? true : false;
	}
}


class Repeater : Exchanger {
	
}


class Sender : Exchanger {
    //TODO
    // add RepeatQueue

    public class Params {
        public int retry_count = 1;
        public bool in_progress = false;
        public DateTime retry_defer;

        public void Clean() {
            retry_count = 1;
            in_progress = false;
        }
    }

    public Params send = new Params();
    public Params connect = new Params();
    public SendResult result = new SendResult();

	public NetworkStatus Result(NetworkStatus status, Api.EventCmd apiCmd, string error, Repeater repeater) {
        switch (status) {
            case NetworkStatus.Error: {
                    try {
                        apiCmd.GetApiEvent(currentMsg.Cmd).Error(Api.ErrorType.Network, error);
                    } catch (Exception ex) {
                        if (MAIN.IS_TEST) {
                            Debug.Log("Exception: " + ex);
                            Debug.Log("StackTrace: " + ex.StackTrace);
                        }
                    }
                    break;
                }
            case NetworkStatus.Ok: {
                    if (!currentMsg.Dirty) {
                        currentMsg.Seq += 1;
                        currentMsg.Time = DateTime.Now;
                        repeater.Enqueue(currentMsg);
                    }
                    break;
                }
            default:
                break;
        }
			
		Dequeue();
        send.Clean();
        return status;
    }
}

public class SendResult
{
    private bool _sended = false;

    public bool sended {
        get { return _sended; }
    }

    public void isSent(bool result) {
        _sended = result;
    }
}

public class Network
{
    private string ServiceUrl;
    private bool _isConnected = false;
    private string _error = null;

    private WebSocketSharp.WebSocket _socket;
    private Api.EventCmd _apiCmd;
    private Sender _sender = new Sender();
    private Reader _reader = new Reader();
	private Repeater _repeater = new Repeater();

    // repeat delays
	private readonly TimeSpan _repeaterRetryDelay = TimeSpan.FromMilliseconds(7500);
	private readonly TimeSpan _senderRetryDelay = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _connectRetryDelay = TimeSpan.FromMilliseconds(500);

    // network timeouts
    private readonly TimeSpan _connectTimeout = TimeSpan.FromMilliseconds(7500);
    private readonly TimeSpan _sendTimeout = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _waitTimeout = TimeSpan.FromMilliseconds(500);
    private const int _retryCount = 2;

    //TODO 
    // form msg id from app id
    private int _msgId = 0;
    private System.Random rand = new System.Random(DateTime.Now.Millisecond);

    private void message(object sender, WebSocketSharp.MessageEventArgs e) {
        var msg = Api.Proto.ParseMessage(e.RawData);
        _reader.Queue = msg;
    }

    private void open(object sender, EventArgs e) {
        Debug.Log("Connected");
        _sender.connect.Clean();
        _isConnected = true;
    }

    private void close(object sender, WebSocketSharp.CloseEventArgs e) {
        Debug.Log("Closed");
        _error = e.Reason;
        _isConnected = false;
    }

    private void error(object sender, WebSocketSharp.ErrorEventArgs e) {
        _error = e.Message;
    }

    private NetworkStatus send() {

        switch (connect()) {
            case NetworkStatus.Error: {
				return _sender.Result(NetworkStatus.Error, _apiCmd, _error, _repeater);
                }
            case NetworkStatus.Ok: {

                    if (_sender.send.in_progress) {
                        if (!_sender.result.sended && _error == null) {
                            if (DateTime.Now.Subtract(_sender.send.retry_defer) > _sendTimeout) {
                                _sender.result = new SendResult();
                                _sender.send.retry_defer = DateTime.MaxValue;
                                _error = "Timeout, while sending message";
                            }
                            return NetworkStatus.InProgress;
                        } else {
                            if (_error != null) {
                                Debug.Log("Send Error: " + _sender.send.retry_count.ToString() + "  " + _error);
                                _sender.send.in_progress = false;
                                if (_sender.send.retry_count >= _retryCount) {
                                    Debug.Log("Send Exit");
                                    unregister();
                                    return _sender.Result(NetworkStatus.Error, _apiCmd, _error, _repeater);
                                } else {
                                    Debug.Log("Send Retry: " + _sender.send.retry_count.ToString());
                                    _sender.send.retry_count += 1;
                                    _sender.send.retry_defer = DateTime.Now;
                                    return NetworkStatus.InProgress;
                                }
                            } else {
                                if (_sender.result.sended) {
                                    //Debug.Log("Send OK");
                                    return _sender.Result(NetworkStatus.Ok, _apiCmd, _error, _repeater);
                                } else {
                                    Debug.Log("Send ERROR");
                                    unregister();
                                    return _sender.Result(NetworkStatus.Error, _apiCmd, _error, _repeater);
                                }
                            }
                        }
                    }

                    if (_sender.send.retry_count == 1 || DateTime.Now.Subtract(_sender.send.retry_defer) > _senderRetryDelay) {
                        if (!_sender.currentMsg.Dirty) {
                            Debug.Log("Send Send: " + _sender.send.retry_count + ", id: " + _sender.currentMsg.Id + ", cmd: " + _sender.currentMsg.Cmd);
                            _error = null;
                            _sender.result = new SendResult();
                            _sender.send.in_progress = true;
                            _sender.send.retry_defer = DateTime.Now;
                            _socket.SendAsync(_sender.currentMsg.GetString(), _sender.result.isSent);
                            return NetworkStatus.InProgress;
                        } else {
                            Debug.Log("Send Already received: " + _sender.send.retry_count + ", id: " + _sender.currentMsg.Id + ", cmd: " + _sender.currentMsg.Cmd);
                            return _sender.Result(NetworkStatus.Ok, _apiCmd, _error, _repeater);
                        }
                    }
                    break;
                }
            case NetworkStatus.InProgress: {
                    break;
                }
        }

        return NetworkStatus.InProgress;
    }

    private NetworkStatus connect() {

        if (_sender.connect.in_progress) {
            if (!_isConnected && _error == null) {
                if (DateTime.Now.Subtract(_sender.connect.retry_defer) > _connectTimeout) {
                    unregister();
                    _error = "Timeout, the connection will be dropped";
                    _sender.connect.retry_defer = DateTime.MaxValue;
                }
                return NetworkStatus.InProgress;
            } else {
                if (_error != null) {
                    Debug.Log("Connect Error: " + _sender.connect.retry_count.ToString() + "  " + _error);
                    _sender.connect.in_progress = false;
                    if (_sender.connect.retry_count >= _retryCount) {
                        Debug.Log("Connect Exit");
                        unregister();
                        _sender.connect.Clean();
                        return NetworkStatus.Error;
                    } else {
                        _sender.connect.retry_count += 1;
                        _sender.connect.retry_defer = DateTime.Now;
                        Debug.Log("Connect Retry: " + _sender.connect.retry_count.ToString());
                        return NetworkStatus.InProgress;
                    }
                } else {
                    if (_isConnected) {
                        _sender.connect.Clean();
                        Debug.Log("Connect OK");
                        return NetworkStatus.Ok;
                    } else {
                        _sender.connect.Clean();
                        unregister();
                        Debug.Log("Connect ERROR");
                        return NetworkStatus.Error;
                    }
                }
            }
        }

        if (!_isConnected) {
            if (_sender.connect.retry_count == 1 || DateTime.Now.Subtract(_sender.connect.retry_defer) > _connectRetryDelay) {
                Debug.Log("Connect Connecting: " + _sender.connect.retry_count);
                register();
                _sender.connect.in_progress = true;
                _sender.connect.retry_defer = DateTime.Now;
                _socket.ConnectAsync();
            }
        } else {
            //_sender.connect.Clean();
            //Debug.Log("Connected OK");
            return NetworkStatus.Ok;
        }

        return NetworkStatus.InProgress;
    }

	private NetworkStatus repeat() {

        if (_repeater.currentMsg.Dirty) {
            _repeater.Dequeue();
            return NetworkStatus.InProgress;
        }

		if (DateTime.Now.Subtract(_repeater.currentMsg.Time) > _repeaterRetryDelay) {
			Debug.Log("Repeat Send id: " + _repeater.currentMsg.Id + ", cmd: " + _repeater.currentMsg.Cmd);

            // back msg to send queue 
            if (_repeater.currentMsg.Seq <= _retryCount) {
                _sender.Enqueue(_repeater.currentMsg);
            } else {
                Debug.Log("Timeout, id: " + _repeater.currentMsg.Id + ", seq: " + _repeater.currentMsg.Seq + ", cmd: " + _repeater.currentMsg.Cmd);
                try {
                    apiCmd.GetApiEvent(_repeater.currentMsg.Cmd).Error(Api.ErrorType.Timeout, "Timeout due to waiting for a response from the service");
                } catch (Exception ex) {
                    if (MAIN.IS_TEST) {
                        Debug.Log("Exception: " + ex);
                        Debug.Log("StackTrace: " + ex.StackTrace);
                    }
                }
            }

			_repeater.Dequeue();
		}
		return NetworkStatus.Ok;
	}

	private NetworkStatus read() {
		var msg = _reader.Queue;

		if (_sender.ReleaseMessage(msg) || _repeater.ReleaseMessage(msg)) {
            try {
                _apiCmd.GetApiEvent(msg.Cmd).Respond(msg.Payload);
            } catch (Exception ex) {
                if (MAIN.IS_TEST) {
                    Debug.Log("Exception: " + ex);
                    Debug.Log("StackTrace: " + ex.StackTrace);
                }
            }
        } else {
            if (MAIN.IS_TEST)
                Debug.Log("Received expired or unexpected response :" + msg.GetString());
		}
		return NetworkStatus.Ok;
	}

    private void register()
    {
        _error = null;
        _isConnected = false;

        _socket = new WebSocketSharp.WebSocket(ServiceUrl);
        _socket.Compression = WebSocketSharp.CompressionMethod.Deflate;
        _socket.OnOpen += open;
        _socket.OnClose += close;
        _socket.OnError += error;
        _socket.OnMessage += message;

        if (MAIN.IS_TEST) {
            _socket.Log.Output += (data, path) => Debug.Log(data.ToString());
            _socket.Log.Level = WebSocketSharp.LogLevel.Trace;
        }
        _socket.WaitTime = _waitTimeout;
        _socket.ConnTime = _connectTimeout;
    }

    private void unregister() {

        if (_socket != null)
        {
            _socket.OnOpen -= open;
            _socket.OnClose -= close;
            _socket.OnError -= error;
            _socket.OnMessage -= message;

            _socket.CloseAsync(WebSocketSharp.CloseStatusCode.Undefined, null);
            _socket = null;
        }
    }

#region Public Methods

    public Api.EventCmd apiCmd {
        get { return _apiCmd; }
    }

    public void Exchange() {

        if (_sender.IsReady()) {
            send();
        }

        if (_reader.isReady()) {
            read();
        }

        if (_repeater.IsReady()) {
            repeat();
        }
    }

    public void Init()
    {
        _apiCmd.Init();

#if UNITY_EDITOR
        //ServiceUrl = "ws://192.168.53.1:8080/ws";   // local dev
        ServiceUrl = "wss://dev.bingo.emict.net/ws-bingo/";    // DEV
#else
    if (MAIN.IS_TEST)
        ServiceUrl = "wss://test.bingo.emict.net/ws-bingo/";   // TEST
    else
        ServiceUrl = "wss://mobile.msl.ua/ws-bingogo/";    // PROD
#endif
}

public bool ApiRequest(string cmd, string payload) {

        ulong id = 0;
        if (MAIN.getMain.applicationID != "") {
            id = Api.Proto.RequestId(Api.Proto.LowerPartOfRequestId(MAIN.getMain.applicationID), ++_msgId, rand);
            if (id == 0) {
                Debug.Log("Error: request id is null");
                return false;
            }
        }

        var msg = Api.Proto.RequestMessage(cmd, id, payload);
        if (msg != null) {
            if (!_sender.HasMessage(msg)) {
                _sender.Enqueue(msg);
                WaitingServerAnsver.show(cmd);
                return true;
            }
            Debug.Log("Error: message in queue");
            return false;
        }
        Debug.Log("Error: message is null");
        return false;
    }

#endregion
}