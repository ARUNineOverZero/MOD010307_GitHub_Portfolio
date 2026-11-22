using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineShortcutContext : IShortcutContext
    {
        public bool active => SplineEditor.IsEditing;
    }
    
    [InitializeOnLoad]
    public static class SplineShortcutContextRegistrar
    {
        private static readonly SplineShortcutContext Context = new SplineShortcutContext();

        static SplineShortcutContextRegistrar()
        {
            EditorApplication.delayCall += () =>
            {
                ShortcutManager.RegisterContext(Context);
            };
        }
    }

    public class SplineNodePrefabShortcutContext : IShortcutContext
    {
        public bool active => SplineNodePrefabEditor.IsEditing;
    }
    
    [InitializeOnLoad]
    public static class SplineNodePrefabShortcutContextRegistrar
    {
        private static readonly SplineNodePrefabShortcutContext Context = new SplineNodePrefabShortcutContext();

        static SplineNodePrefabShortcutContextRegistrar()
        {
            EditorApplication.delayCall += () =>
            {
                ShortcutManager.RegisterContext(Context);
            };
        }
    }
}