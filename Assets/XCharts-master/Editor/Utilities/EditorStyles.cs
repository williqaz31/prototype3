using UnityEditor;
using UnityEngine;

namespace XCharts.Editor
{
    public class EditorCustomStyles
    {
        private static readonly Color splitterDark = new(0.12f, 0.12f, 0.12f, 0.5f);
        private static readonly Color splitterLight = new(0.6f, 0.6f, 0.6f, 0.5f);

        private static readonly Texture2D paneOptionsIconDark =
            (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");

        private static readonly Texture2D paneOptionsIconLight =
            (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");

        private static readonly Color headerBackgroundDark = new(0.1f, 0.1f, 0.1f, 0.2f);
        private static readonly Color headerBackgroundLight = new(1f, 1f, 1f, 0.2f);

        public static readonly GUIStyle headerStyle = EditorStyles.boldLabel;

        public static readonly GUIStyle foldoutStyle = new(EditorStyles.foldout)
        {
            font = headerStyle.font,
            fontStyle = headerStyle.fontStyle
        };

        public static readonly GUIContent iconAdd = new("+", "Add");
        public static readonly GUIContent iconRemove = new("-", "Remove");
        public static readonly GUIContent iconUp = new("↑", "Up");
        public static readonly GUIContent iconDown = new("↓", "Down");
        public static readonly GUIStyle invisibleButton = "InvisibleButton";
        public static readonly GUIStyle smallTickbox = new("ShurikenToggle");

        public static Color splitter => EditorGUIUtility.isProSkin ? splitterDark : splitterLight;

        public static Texture2D paneOptionsIcon =>
            EditorGUIUtility.isProSkin ? paneOptionsIconDark : paneOptionsIconLight;

        public static Color headerBackground =>
            EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight;
    }
}