using System.Reflection;
using Android.Content.Res;

namespace Maui.ContentButton
{
    internal static class ColorStateListExtensions
    {
        static Type? platformInteropType;

        static MethodInfo GetMethod(string name)
        {
            platformInteropType ??= 
                typeof(MauiDrawable).Assembly.GetType("Microsoft.Maui.PlatformInterop");
            
            var m = platformInteropType.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            return m;
        }

        static ColorStateList Invoke(string method, params object[] args)
        {
            var methodInfo = GetMethod(method);
            return methodInfo.Invoke(null, args) as ColorStateList;
        }

        // public static ColorStateList CreateDefault(int color) =>
        //
        //     Invoke("GetDefaultColorStateList", color);

        // public static ColorStateList CreateEditText(int enabled, int disabled) =>
        //     Invoke("GetEditTextColorStateList", enabled, disabled);
        //
        // public static ColorStateList CreateCheckBox(int all) =>
        //     CreateCheckBox(all, all, all, all);
        //
        // public static ColorStateList CreateCheckBox(int enabledChecked, int enabledUnchecked, int disabledChecked,
        //     int disabledUnchecked) =>
        //     Invoke("GetCheckBoxColorStateList",
        //         enabledChecked, enabledUnchecked, disabledChecked, disabledUnchecked);
        //
        // public static ColorStateList CreateSwitch(int all) =>
        //     CreateSwitch(all, all, all);
        //
        // public static ColorStateList CreateSwitch(int disabled, int on, int normal) =>
        //     
        //     Invoke("GetSwitchColorStateList", disabled, on, normal);

        public static ColorStateList CreateButton(int all) =>
            CreateButton(all, all, all, all);

        public static ColorStateList CreateButton(int enabled, int disabled, int off, int pressed) =>
            new ColorStateList(new int[][] {
                new int[] {  Android.Resource.Attribute.StateEnabled },
                new int[] { -Android.Resource.Attribute.StateEnabled },
                new int[] { -Android.Resource.Attribute.StateChecked },
                new int[] {  Android.Resource.Attribute.StatePressed },
            }, new[] { enabled, disabled, off, pressed });
    }
}