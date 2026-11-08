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
}