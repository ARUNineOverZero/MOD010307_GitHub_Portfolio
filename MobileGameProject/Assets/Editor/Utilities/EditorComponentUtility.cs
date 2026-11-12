using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MobileGamePrefabUtility
{
    public static class EditorComponentUtility
    {
        private class ComponentAndProjectName
        {
            private static ComponentAndProjectName _none = new ComponentAndProjectName() { name = "None"};
            public static ComponentAndProjectName None => _none;

            public string name;
            public Component component;

        }

        private class Cache
        {
            public ComponentAndProjectName[] components;
        }

        public static T DrawComponentsPopup<T>(string label, T current, Predicate<T> predicate = null) where T : Component
        {
            Cache cache = BuildCache<T>(predicate);

            int index = 0;
            if(current != null)
            {
                for(int i =0; i < cache.components.Length; i++)
                {
                    if(cache.components[i].component == current)
                    {
                        index = i;
                        break;
                    }
                }
            }

            int newIndex = EditorGUILayout.Popup(label, index, cache.components.Select(x => x.name).ToArray());
            return newIndex <=0 
                    ? null 
                    : cache.components[newIndex].component as T;
        }

        private static Cache BuildCache<T>(Predicate<T> predicate) where T : Component
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            List<ComponentAndProjectName> list = new List<ComponentAndProjectName>();

            foreach(string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(gameObject == null) continue;

                T component = gameObject.GetComponent<T>();
                if(component == null || (predicate != null && !predicate(component))) continue;

                list.Add(new ComponentAndProjectName() { name = Path.GetFileNameWithoutExtension(path), component = component} );
            }

            list.Sort((x,y) => string.Compare(x.name, y.name, StringComparison.Ordinal));
            list.Insert(0, ComponentAndProjectName.None);
            return new Cache { components = list.ToArray() };
        }
    }
}