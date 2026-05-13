using UnityEditor;
using UnityEngine;
using XCharts.Runtime;
#if dUI_TextMeshPro
using TMPro;
#endif

namespace XCharts.Editor
{
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : UnityEditor.Editor
    {
        private Theme m_Theme;

        private void OnEnable()
        {
            m_Theme = target as Theme;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button(Styles.btnReset))
                if (EditorUtility.DisplayDialog(Styles.btnReset.text, Styles.btnReset.tooltip, "Yes", "Cancel"))
                {
                    m_Theme.ResetTheme();
                    Debug.Log("XCharts: Reset Finish.");
                }

            if (GUILayout.Button(Styles.btnSyncFontFromSetting))
                if (EditorUtility.DisplayDialog(Styles.btnSyncFontFromSetting.text,
                        Styles.btnSyncFontFromSetting.tooltip, "Yes", "Cancel"))
                {
                    m_Theme.common.font = XCSettings.font;
                    m_Theme.SyncFontToSubComponent();
#if dUI_TextMeshPro
                    m_Theme.common.tmpFont = XCSettings.tmpFont;
                    m_Theme.SyncTMPFontToSubComponent();
#endif
                    Debug.Log("XCharts: Sync Finish.");
                }

            if (GUILayout.Button(Styles.btnSyncFontToSubTheme))
                if (EditorUtility.DisplayDialog(Styles.btnSyncFontToSubTheme.text, Styles.btnSyncFontToSubTheme.tooltip,
                        "Yes", "Cancel"))
                {
                    m_Theme.SyncFontToSubComponent();
#if dUI_TextMeshPro
                    m_Theme.SyncTMPFontToSubComponent();
#endif
                    Debug.Log("XCharts: Sync Finish.");
                }
        }

        private static class Styles
        {
            internal static readonly GUIContent btnReset = new("Reset to Default", "Reset to default theme");

            internal static readonly GUIContent btnSyncFontToSubTheme =
                new("Sync Font to Sub Theme", "Sync main theme font to sub theme font");

            internal static readonly GUIContent btnSyncFontFromSetting = new("Sync Font from Setting",
                "Sync main theme font and sub theme font from XCSetting font");
        }
    }
}