using UnityEngine;
using System.Collections;
//////////////////////////////////////////////////////////////////////
// █ это перегруженный скрипт, в котором описано несколько классов:
// - SoundsSystem
// - BingogoAudio

// Список доступных игровых звуков и музыки
public enum Sound {
    S_UNDEFINE = 0,
    S_MUSICK,
    S_LOGO,
    S_BUTTON,
    S_RADIO_BUTTON,
    S_CHECK_BUTTON,
    S_COIN,
    S_DISAPEAR,
    S_DRAGON,
    S_DRAGON_FULL,
    S_PRE_WIN,
    S_WIN,
    S_BIG_WIN,
    S_TICKET_TURN,
    S_STAR_TRANSFORM,
    S_BALL_KICK,
    S_GOLD_BUY,
    S_RUBINS_BUY,
    S_NEED_MORE_FUNDS,
    S_ERROR
}
// Этот класс(синглтон) отвечает за все игровые звуки и музыку в игре
public class SoundsSystem {
    //public bool musikMute = false;
    //public bool soundsMute = false;
    bool isInit = false;
    const int limitCoinsSounds = 15;                // лимит звуков для падающих монет
    const int limitBallsKick = 10;                  // лимит для соударов шаров
    private static SoundsSystem soundSystem = null; // ссылка на себя для удобного доступа
    public static SoundsSystem getSoundSystem {
        get {
            if (soundSystem == null) soundSystem = new SoundsSystem();
            if (!soundSystem.isInit) soundSystem.isInit = soundSystem.init();
            return soundSystem;
        }
    }
    public AudioSource musik = null;                // сюда помещается музыка из RESOURCES

    public static bool soundOn;                     // включены ли звки
    public static bool ballsSoundOn;                // включены ли звуки соударов шаров
    public static bool musikOn;                     // включена ли музыка
    //public static float soundVolume = 0.0f;
    //public static float musikVolume = 0.0f;
    // При инициализациия производится считывания из фала пользователя, его настройки, помещаются в соответственные перменные, из которых черпает для себя данные и само окно настроек ( когда оно будет создаваться )
    public bool init() {
        if (isInit) return true;
        //Debug.Log("musikOn? "+PlayerPrefs.GetInt("musikOn", 1));
        musikOn = PlayerPrefs.GetInt("musikOn", 1) != 0;
        soundOn = PlayerPrefs.GetInt("soundOn", 1) != 0;
        ballsSoundOn = PlayerPrefs.GetInt("ballsSoundOn", 1) != 0;
        //soundVolume = PlayerPrefs.GetFloat("soundVolume", 1.0f);
        //musikVolume = PlayerPrefs.GetFloat("musikVolume", 1.0f);
        isInit = true;
        return isInit;
    }
    // При изменение чего либо в настройках, событие приходит сюда. Подписывание на него производистя там же в окне настроек при его создании
    // При включении или отключении звуков, соответственно производтся операции по пересмотру проигрывающих звуков, и соответственном включении отключения мута в них
    public static void onCheckBoxValueChange(BaseController baseController, BaseController.TypeEvent typeEvent) {
        if (typeEvent != BaseController.TypeEvent.ON_MOUSE_DOWN) return;
        CheckButton checkBox = (CheckButton)baseController;
        switch ( checkBox.name ){
            case "gameMusicBtn": {
                    musikOn = checkBox.value;
                    PlayerPrefs.SetInt("musikOn", musikOn ? 1 : 0);
                    var musikGO = GameObject.Find("backGroundMusik");
                    if (musikGO != null) {
                        var ba = musikGO.GetComponent<BingogoAudio>();
                        ba.mute(!musikOn);
                    } else if (musikOn ) play(Sound.S_MUSICK);
                } break;
            case "ballsSoundBtn": {
                    ballsSoundOn = checkBox.value;
                    PlayerPrefs.SetInt("ballsSoundOn", ballsSoundOn ? 1 : 0);
                } break;
            case "gameSoundBtn": {
                    soundOn = checkBox.value;
                    PlayerPrefs.SetInt("soundOn", soundOn ? 1 : 0);
                    var sounds = GameObject.FindGameObjectsWithTag("BingogoSound");
                    for(int i = 0; i<sounds.Length; i++) {
                        if (sounds[i].name != "backGroundMusik") { 
                            var ba = sounds[i].GetComponent<BingogoAudio>();
                            if (ba != null) ba.mute(soundOn);
                        }
                    }
                } break;
        }
    }
    // Ищет и возвращает найденные звуки, по указаному имени
    public static BingogoAudio[] getSoundsByName(params string[] name) {
        var soundsGO = GameObject.FindGameObjectsWithTag("BingogoSound");
        if (soundsGO.Length == 0) return null;
        int count = 0;
        for (int i = 0; i < soundsGO.Length; i++)
            for(int j = 0; j < name.Length; j++)
            if (soundsGO[i].name == name[j])
                soundsGO[count++] = soundsGO[i];
            BingogoAudio[] res = new BingogoAudio[count];
        for(int i=0; i < count; i++) {
            res[i] = soundsGO[i].GetComponent<BingogoAudio>();
            //Debug.Log("█ find winSound: " + res[i].name);
            if (res[i] == null) Errors.showTest("При поиске проигрывающихся аудио, возникла ошибка!");
        }
        //Debug.Log("█ count: " + count);
        return res;
    }
    // Останавливает проигрование указаного(ных) звук(ов). Точнее изменяет их мут.
    public static void stop(params BingogoAudio[] audio) {
        for (int i=0; i<audio.Length; i++) { 
            //if (audio[i] != null) { 
                var aS = audio[i].GetComponent<BingogoAudio>();
                aS.stop();
            //}
        }
    }
    // Отдельно отведённая функция для воспроизведения звуков выиграшей. Если просиходит множественная череда проигрования, то сначала, производится поиск на наличие уже существующих проигрышвающих звуков. И удаление если они есть
    static void stopWinsSoundsPlay() {
        var winSounds = getSoundsByName("S_WIN", "S_BIG_WIN");
        stop(winSounds);
    }
    // Функции запуска прогирования звуков / музыки
    public static BingogoAudio play(Sound sound) { return play(sound, Vector3.zero); }
    public static BingogoAudio play(Sound sound, Vector3 pos) {
        //Debug.Log("████ "+ sound);
        AudioClip audioClip = null;

        if (sound == Sound.S_LOGO) { 
            var logoSound = Resources.Load<AudioClip>("Audio/sounds/Bingogo_Logo");
            audioClip = logoSound;
        } else { 
            var r = MAIN.getMain.getResources();
            if (r == null) {
                Errors.showTest("при попытке воспросизвести звук: resources == null");
                return null;
            }
            if (sound == Sound.S_MUSICK) {
                var musikGO = GameObject.Find("backGroundMusik");
                if (musikGO != null) return musikGO.GetComponent<BingogoAudio>(); // если музыка уже играет, ничего не делаем...
                var ba = playAudio(r.musik, pos);
                ba.gameObject.name = "backGroundMusik";
                getSoundSystem.musik = ba.GetComponent<AudioSource>();
                ba.loop = true;
                ba.destroyOnEnd = false;
                return ba;
            } else if (SoundsSystem.soundOn)
                switch (sound){
                    case Sound.S_BUTTON: audioClip = r.buttonPushSound; break;
                    case Sound.S_RADIO_BUTTON: audioClip = r.radioBtn; break;
                    case Sound.S_CHECK_BUTTON: audioClip = r.checkBtn; break;
                    case Sound.S_COIN: {
                            var coinSounds = getSoundsByName("S_COIN");
                            if (coinSounds == null || coinSounds.Length < SoundsSystem.limitCoinsSounds)
                            audioClip = r.coinDropSound;
                        } break;
                    case Sound.S_DISAPEAR: audioClip = r.starsDisapear; break;
                    case Sound.S_DRAGON: audioClip = r.dragon; break;
                    case Sound.S_DRAGON_FULL: audioClip = r.dragonFull; break;
                    case Sound.S_PRE_WIN: { audioClip = r.prewin; } break;
                    case Sound.S_WIN: { stopWinsSoundsPlay(); audioClip = r.winSound; } break;
                    case Sound.S_BIG_WIN: { stopWinsSoundsPlay(); audioClip = r.bigWinSound; } break;
                    case Sound.S_TICKET_TURN: audioClip = r.ticketTurn; break;
                    case Sound.S_STAR_TRANSFORM: audioClip = r.startTransform; break;
                    case Sound.S_BALL_KICK: {
                        var bk = getSoundsByName("S_BALL_KICK");
                        if (SoundsSystem.ballsSoundOn && (bk == null ||
                            bk.Length < SoundsSystem.limitBallsKick))
                            audioClip = r.ballKickSound; } break;
                    case Sound.S_GOLD_BUY: audioClip = r.goldBuy; break;
                    case Sound.S_RUBINS_BUY: audioClip = r.rubinsBuy; break;
                    case Sound.S_NEED_MORE_FUNDS: audioClip = r.needMoreFunds; break;
                    case Sound.S_ERROR: audioClip = r.errorSound; break;
                }
            else return null;
        }
        
        if (audioClip == null) return null;
        var res = playAudio(audioClip, pos);
        res.name = sound.ToString();
        return res;
    }
    public static BingogoAudio playAudio(AudioClip audioClip) {
        return playAudio(audioClip, Vector3.zero);
    }
    public static BingogoAudio playAudio(AudioClip audioClip,Vector3 pos){
        if (audioClip == null) return null;
        var go = new GameObject();
        go.tag = "BingogoSound";
        //go.name = go.tag;
        go.transform.position = pos;
        var aSource = go.AddComponent<AudioSource>();
        aSource.clip = audioClip;
        aSource.playOnAwake = true;
        //aSource.minDistance = 20.0f;
        //aSource.maxDistance = 120.0f;
        //aSource.Play();
        var bAudio = go.AddComponent<BingogoAudio>();
        bAudio.subscribeOnAudioFinish(onAudioPlayFinish);
        return bAudio;
    }
    // ...
    static void onAudioPlayFinish(BingogoAudio audio){}
}
// Этот класс является обвёртной класса AudioSource практически повторяющий все его основные функции...
public class BingogoAudio : MonoBehaviour{
    public AudioSource m_audioSource;               // сам AudioSource внутри
    public Sound soundType;                         // тип
    public delegate void OnAudioFinish(BingogoAudio audio); // █ при окончании проигрования. Как такого события внутри Unity нету, потому берётся длина трека по времени, и по истичению этого времени(при проигровании) создаётся событие
    private OnAudioFinish callBack;
    public void subscribeOnAudioFinish(OnAudioFinish newCallBackFunction) { callBack = newCallBackFunction; }
    public bool destroyOnEnd = true;                // удалить ли по окончанию прогирования.
    public bool loop = false;                       // повторяющийся ли звук, если да, то будет повторятся постоянно
    //float volumeForMute = 1.0f;
    float lenAudio;                                 // длина звука (в секундах)
    // Автоматически запускается проигрование
    void Start(){
        m_audioSource = GetComponent<AudioSource>();
        lenAudio = m_audioSource.clip.length;
        m_audioSource.Play();
    }
    // установка мута (звук проигрывается но его не слышно, НЕ НА ПАУЗЕ)
    public void mute(bool val = true){ m_audioSource.mute = val; }
    // Удаление при остановке...
    public void stop() { Destroy(gameObject); }
    // здесь производится лишь отстлеживание его окончания проигрования
    void Update(){
        if ((lenAudio < 0f)){
            if (callBack != null) callBack(this);
            if (loop) Start();
            else if (destroyOnEnd) Destroy(gameObject);
        } else lenAudio -= Time.deltaTime;
    }
}
