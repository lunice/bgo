package com.ilot.bingogo;
import com.unity3d.player.UnityPlayer;

public class CallBack {

    public static String Object;
    public static String onComplete;
    public static String onError;

    public static void Complete(String msg) {
        if (CallBack.Object != null && CallBack.onComplete != null) {
            UnityPlayer.UnitySendMessage(CallBack.Object, CallBack.onComplete, msg);
        }
    }

    public static void Error(String msg) {
        if (CallBack.Object != null && CallBack.onError != null) {
            UnityPlayer.UnitySendMessage(CallBack.Object, CallBack.onError, msg);
        }
    }
}
