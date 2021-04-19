package com.metalpopgames.checkaccessibility;

/**
 * Created by Michelle.Martin on 3/7/2017.
 * Copyright (c)2017 MetalPop Games
 */

import java.util.ArrayList;
import java.util.List;

import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ResolveInfo;
import android.database.Cursor;
import android.net.Uri;
import android.os.Handler;
import android.provider.Settings;
import android.provider.Settings.SettingNotFoundException;
import android.accessibilityservice.AccessibilityServiceInfo;
import android.view.accessibility.AccessibilityManager;
import android.widget.Toast;
import android.annotation.TargetApi;

import static android.content.Context.ACCESSIBILITY_SERVICE;

public class CheckAccessibilityState
{
    public static boolean isAccessibilityEnabled(Context context) {

        AccessibilityManager am = (AccessibilityManager) context.getSystemService(ACCESSIBILITY_SERVICE);

        boolean isAccessibilityEnabled_flag = am.isEnabled();
        boolean isExploreByTouchEnabled_flag = false;
        isExploreByTouchEnabled_flag = isScreenReaderActive_pre_and_postAPI14(context);

        return (isAccessibilityEnabled_flag && isExploreByTouchEnabled_flag);

    }

    //----------
    // API 14 has method to detect TalkBack .. but for pre-API 14 will use some methods suggested in stackoverflow
    // http://stackoverflow.com/questions/11831666/how-to-check-if-talkback-is-active-in-jellybean
    //    https://code.google.com/p/eyes-free/source/browse/trunk/shell/src/com/google/marvin/shell/HomeLauncher.java?spec=svn623&r=623

    private final static String SCREENREADER_INTENT_ACTION = "android.accessibilityservice.AccessibilityService";
    private final static String SCREENREADER_INTENT_CATEGORY = "android.accessibilityservice.category.FEEDBACK_SPOKEN";


    // http://stackoverflow.com/questions/11831666/how-to-check-if-talkback-is-active-in-jellybean
    //    this seems to be working on HTC Desire (Android 2.3.3)
    //        and Samsung Galaxy Note (Android 4.0.3) - API 15
    //
    private static boolean isScreenReaderActive_pre_and_postAPI14(Context context) {

        Intent screenReaderIntent = new Intent(SCREENREADER_INTENT_ACTION);
        screenReaderIntent.addCategory(SCREENREADER_INTENT_CATEGORY);

        AccessibilityManager am = (AccessibilityManager) context.getSystemService(ACCESSIBILITY_SERVICE);

        List<AccessibilityServiceInfo> enabledServices = am.getEnabledAccessibilityServiceList (AccessibilityServiceInfo.FEEDBACK_SPOKEN);
        return !enabledServices.isEmpty();
    }


}
