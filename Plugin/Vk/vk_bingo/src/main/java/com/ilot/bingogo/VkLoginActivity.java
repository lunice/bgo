package com.ilot.bingogo;

import com.vk.sdk.VKSdk;
import com.vk.sdk.api.VKError;
import com.vk.sdk.VKCallback;
import com.vk.sdk.VKAccessToken;
import com.vk.sdk.api.VKApi;
import com.vk.sdk.api.VKRequest;
import com.vk.sdk.api.VKRequest.VKRequestListener;
import com.vk.sdk.api.VKResponse;
import com.vk.sdk.api.VKParameters;
import com.vk.sdk.api.VKApiConst;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.content.res.Configuration;
import android.util.Log;
import android.support.annotation.NonNull;

import org.json.JSONException;
import org.json.JSONObject;

public class VkLoginActivity extends Activity {

    private boolean isResumed = false;
    private static VkLoginActivity self;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);

        Log.i("VkLoginActivity", "Create");
        self = this;

        VKSdk.wakeUpSession(this, new VKCallback<VKSdk.LoginState>() {
            @Override
            public void onResult(VKSdk.LoginState res) {
                Log.i("VkLoginActivity", "onResult");
                if (isResumed) {
                    switch (res) {
                        case LoggedOut:
                            Log.i("VkLoginActivity", "onResult LoggedOut");
                            showLogin();
                            break;
                        case LoggedIn:
                            Log.i("VkLoginActivity", "onResult LoggedIn");
                            completeLogin();
                            finish();
                            break;
                        case Pending:
                            break;
                        case Unknown:
                            break;
                    }
                }
            }

            @Override
            public void onError(VKError error) {
                Log.i("VkLoginActivity", "onResult Auth Error");
                CallBack.Error(error.toString());
                finish();
            }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        isResumed = true;
        if (VKSdk.isLoggedIn()) {
            Log.i("VkLoginActivity", "onResume LoggedIn");
            completeLogin();
            finish();
        } else {
            Log.i("VkLoginActivity", "onResume LoggedOut");
            showLogin();
        }
    }

    @Override
    protected void onPause() {
        isResumed = false;
        Log.i("VkLoginActivity", "onPause");
        super.onPause();
    }

    @Override
    protected void onDestroy() {
        Log.i("VkLoginActivity", "onDestroy");
        super.onDestroy();
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        Log.i("VkLoginActivity", "onConfigurationChanged");
        super.onConfigurationChanged(newConfig);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        Log.i("VkLoginActivity", "onActivityResult");
        VKCallback<VKAccessToken> callback = new VKCallback<VKAccessToken>() {
            @Override
            public void onResult(VKAccessToken res) {
                // User passed Authorization
                Log.i("VkLoginActivity", "Auth Pass");
                completeLogin();
                finish();
            }

            @Override
            public void onError(VKError error) {
                // User didn't pass Authorization
                Log.i("VkLoginActivity", "onActivityResult Auth Error");
                CallBack.Error(error.toString());
                finish();
            }
        };

        if (!VKSdk.onActivityResult(requestCode, resultCode, data, callback)) {
            Log.i("VkLoginActivity", "onActivityResult without result");
            super.onActivityResult(requestCode, resultCode, data);
        }
    }

    private static void sendResult(JSONObject json) {
        String error = null;
        if (json != null && json.has("response")) {
            try {
                if (json.getJSONArray("response").length() == 1) {
                    JSONObject result = json.getJSONArray("response").getJSONObject(0);
                    if (result != null) {
                        CallBack.Complete(result.toString());
                        return;
                    } else {
                        error = "Result obj is null";
                    }
                }
            } catch (JSONException ex) {
                Log.i("getResult", "JSONException: " + ex.getMessage());
                error = ex.getMessage();
            }
        } else {
            error = "response obj not found";
        }
        CallBack.Error(error);
    }

    private static void getProfile() {
        VKRequest request = VKApi.users().get(VKParameters.from(VKApiConst.FIELDS, "sex, bdate, city, home_town, country"));

        //VKRequest wallPost = VKApi.wall().post(VKParameters.from(VKApiConst.OWNER_ID, userId, VKApiConst.MESSAGE, message, VKApiConst.ATTACHMENTS, attachments.toAttachmentsString()));

        request.attempts = 5;

        request.executeWithListener(new VKRequestListener() {
            @Override
            public void onComplete(VKResponse response) {
                //Do complete stuff
                Log.i("getProfile", "executeWithListener: " + response.json.toString());
                sendResult(response.json);
            }

            @Override
            public void onError(VKError error) {
                //Do error stuff
                Log.i("getProfile", "executeWithListener: " + error.toString());
                CallBack.Error(error.toString());
            }

        });
    }

    private void showLogin() {
        VKSdk.login(self, "");
    }

    private void completeLogin() {
        CallBack.Complete("true");
    }

    public static void Login(@NonNull Activity activity, @NonNull String object,
                                  @NonNull String onComplete, @NonNull String onError) {
        Log.i("VkLoginActivity", "Login");

        CallBack.Object = object;
        CallBack.onComplete = onComplete;
        CallBack.onError = onError;

        VKSdk.initialize(activity.getApplicationContext());

        Intent intent = new Intent(activity, VkLoginActivity.class);
        activity.startActivity(intent);
    }

    public static void Logout(@NonNull Activity activity) {
        Log.i("VkLoginActivity", "Logout");

        VKSdk.initialize(activity.getApplicationContext());
        VKSdk.logout();
    }

    public static boolean Status(@NonNull Activity activity) {
        Log.i("VkLoginActivity", "Status");

        VKSdk.initialize(activity.getApplicationContext());
        return VKSdk.isLoggedIn();
    }

    public static void Profile(@NonNull Activity activity, @NonNull String object,
                               @NonNull String onComplete, @NonNull String onError) {
        Log.i("VkLoginActivity", "Profile");

        CallBack.Object = object;
        CallBack.onComplete = onComplete;
        CallBack.onError = onError;

        VKSdk.initialize(activity.getApplicationContext());
        if (VKSdk.isLoggedIn()) {
            getProfile();
        }else {
            CallBack.Error("false");
        }
    }
}
