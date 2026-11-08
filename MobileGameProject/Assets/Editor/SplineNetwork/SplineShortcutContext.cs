using UnityEditor.ShortcutManagement;

namespace MobileGameProjectEditor.SplineNetwork
{
    public class SplineShortcutContext : IShortcutContext
    {
        public bool active => SplineEditor.IsEditing;
    }
}