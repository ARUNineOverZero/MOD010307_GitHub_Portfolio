using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

public class AssetScatterWindow : EditorWindow
{
    [System.Serializable]
    public class ScatterAsset
    {
        public GameObject prefab;
        [Min(0.1f)] public float radius = 4f;
        [Range(0f, 1f)] public float weight = 1f;
    }

    private class PlacedPoint
    {
        public Vector3 position;
        public float radius;
        public int childrenSpawned;
    }

    public List<ScatterAsset> assets = new List<ScatterAsset>();
    public LayerMask layerMask = 0;
    public LayerMask rejectLayerMask = 0;
    public float raycastHeight = 1000f;
    public float raycastDistance = 2000f;
    public Vector3 areaCenter = Vector3.zero;
    public Vector2 areaSize = new Vector2(40f, 40f);
    public Transform parent;
    public bool randomizeRotation = true;
    public int randomSeed = 0;

    [Min(1)] public int triesPerPoint = 4;
    [Min(1)] public int maxChildrenPerTree = 2;
    public Vector3 startPoint = Vector3.zero;

    private Vector2 _scroll;

    private bool _assetsFoldout = true;

    [MenuItem("Tools/Asset Scatter")]
    public static void Open() => GetWindow<AssetScatterWindow>("Asset Scatter");

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Scatter Settings", EditorStyles.boldLabel);
        areaCenter = EditorGUILayout.Vector3Field("Area Center", areaCenter);
        areaSize = EditorGUILayout.Vector2Field("Area Size (X/Z)", areaSize);
        raycastHeight = EditorGUILayout.FloatField("Raycast Height", raycastHeight);
        raycastDistance = EditorGUILayout.FloatField("Raycast Distance", raycastDistance);

        int mask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask);
        mask = EditorGUILayout.MaskField("Layer Mask", mask, InternalEditorUtility.layers);
        layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(mask);

        mask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(rejectLayerMask);
        mask = EditorGUILayout.MaskField("Reject Layer Mask", mask, InternalEditorUtility.layers);
        rejectLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(mask);

        triesPerPoint = EditorGUILayout.IntField("Tries Per Tree", triesPerPoint);
        maxChildrenPerTree = EditorGUILayout.IntField("Max Children Per Tree", maxChildrenPerTree);

        EditorGUILayout.BeginHorizontal();
        parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);
        if (GUILayout.Button("Use Selected", GUILayout.Width(100)))
        {
            if (Selection.activeTransform != null)
                parent = Selection.activeTransform;
            else
                Debug.LogWarning("No object selected in the Hierarchy.");
        }
        EditorGUILayout.EndHorizontal();
        randomizeRotation = EditorGUILayout.Toggle("Random Y Rotation", randomizeRotation);
        randomSeed = EditorGUILayout.IntField("Random Seed (0 = random)", randomSeed);
        EditorGUILayout.BeginHorizontal();
        randomSeed = EditorGUILayout.IntField("Random Seed (0 = random)", randomSeed);
        if (GUILayout.Button("Randomize Seed", GUILayout.Width(120)))
            randomSeed = Random.Range(1, 999999);

        EditorGUILayout.EndHorizontal();
        startPoint = EditorGUILayout.Vector3Field("Start Point", startPoint);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);
        _assetsFoldout = EditorGUILayout.Foldout(_assetsFoldout, "Assets", true, EditorStyles.foldoutHeader);

        if (_assetsFoldout)
        {

            for (int i = 0; i < assets.Count; i++)
            {
                var a = assets[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                a.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", a.prefab, typeof(GameObject), false);
                a.radius = EditorGUILayout.FloatField("Radius", a.radius);
                a.weight = EditorGUILayout.Slider("Weight", a.weight, 0f, 1f);
                if (GUILayout.Button("Remove")) assets.RemoveAt(i--);
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Asset"))
                assets.Add(new ScatterAsset());
        }

        EditorGUILayout.Space(15);
        if (GUILayout.Button("Scatter Assets", GUILayout.Height(32)))
            Scatter();

        EditorGUILayout.EndScrollView();
    }

    private void Scatter()
    {
        if (assets.Count == 0 || assets.TrueForAll(a => a.prefab == null)) return;

        if (randomSeed != 0) Random.InitState(randomSeed);
        else Random.InitState(System.Environment.TickCount);

        Undo.SetCurrentGroupName("Asset Scatter");
        int undoGroup = Undo.GetCurrentGroup();

        var allPoints = new List<PlacedPoint>();
        var active = new List<PlacedPoint>();
        int totalPlaced = 0;

        Vector3 basePos = parent != null ? parent.position : Vector3.zero;
        Vector2 start = new Vector2(basePos.x + startPoint.x, basePos.z + startPoint.z);
        var seedAsset = PickWeightedAsset();

        if (seedAsset != null && TryPlace(start, seedAsset, allPoints, out Vector3 seedPos))
        {
            var seed = new PlacedPoint { position = seedPos, radius = seedAsset.radius, childrenSpawned = 0 };
            allPoints.Add(seed);
            active.Add(seed);
            totalPlaced++;
        }

        while (active.Count > 0 && totalPlaced < 10000)
        {
            int idx = Random.Range(0, active.Count);
            var parentPoint = active[idx];

            if (parentPoint.childrenSpawned >= maxChildrenPerTree)
            {
                active.RemoveAt(idx);
                continue;
            }

            bool spawned = false;
            for (int t = 0; t < triesPerPoint; t++)
            {
                var childAsset = PickWeightedAsset();
                if (childAsset == null) continue;

                float angle = Random.Range(0f, Mathf.PI * 2f);
                float dist = parentPoint.radius + childAsset.radius;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = new Vector2(parentPoint.position.x, parentPoint.position.z) + dir * dist;

                if (TryPlace(candidate, childAsset, allPoints, out Vector3 childPos))
                {
                    var child = new PlacedPoint { position = childPos, radius = childAsset.radius, childrenSpawned = 0 };
                    allPoints.Add(child);
                    active.Add(child);
                    parentPoint.childrenSpawned++;
                    totalPlaced++;
                    spawned = true;
                    break;
                }
            }

            if (!spawned || parentPoint.childrenSpawned >= maxChildrenPerTree)
                active.RemoveAt(idx);
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.DisplayDialog("Asset Scatter", $"Placed {totalPlaced} objects.", "OK");
    }

    private ScatterAsset PickWeightedAsset()
    {
        float total = 0f;
        foreach (var a in assets)
            if (a.prefab != null && a.weight > 0f) total += a.weight;
        if (total <= 0f) return null;

        float r = Random.Range(0f, total);
        foreach (var a in assets)
        {
            if (a.prefab == null || a.weight <= 0f) continue;
            r -= a.weight;
            if (r <= 0f) return a;
        }
        return null;
    }

    private bool TryPlace(Vector2 xz, ScatterAsset asset, List<PlacedPoint> existing, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        Vector3 origin = new Vector3(xz.x, areaCenter.y + raycastHeight, xz.y);

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, layerMask))
            return false;

        if (((1 << hit.collider.gameObject.layer) & rejectLayerMask) != 0)
            return false;

        worldPosition = hit.point;

        Vector3 flatNew = new Vector3(worldPosition.x, 0f, worldPosition.z);
        foreach (var p in existing)
        {
            Vector3 flatExisting = new Vector3(p.position.x, 0f, p.position.z);
            if (Vector3.Distance(flatNew, flatExisting) < asset.radius + p.radius)
                return false;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset.prefab);
        if (instance == null) return false;

        Undo.RegisterCreatedObjectUndo(instance, "Scatter Prefab");
        instance.transform.position = worldPosition;
        if (randomizeRotation)
            instance.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        if (parent != null)
            instance.transform.SetParent(parent, true);

        return true;
    }
}