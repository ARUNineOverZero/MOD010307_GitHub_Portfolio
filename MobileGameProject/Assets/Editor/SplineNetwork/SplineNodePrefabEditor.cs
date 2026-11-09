

using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common;
using MobileGameProject.SplineNetwork;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MobileGameProjectEditor.SplineNetwork
{
    [CustomEditor(typeof(SplineNodePrefab))]
    public class SplineNodePrefabEditor : Editor
    {
        #region Fields

        private SplineNodePrefab _splineNodePrefab;
        private SplineNodePrefabDrawer _drawer;
        private SplineNodePrefabSelector _selector;
        private SplineNodePrefabManipulator _manipulator;
        private bool _isEditing;
        private bool _createHeld;

        #endregion Fields

        #region Unity Callbacks

        public void OnEnable()
        {
            _splineNodePrefab = (SplineNodePrefab)target;
            _selector = new SplineNodePrefabSelector(_splineNodePrefab);
            _manipulator = new SplineNodePrefabManipulator(_splineNodePrefab, _selector);
            _drawer = new SplineNodePrefabDrawer(_splineNodePrefab, _selector);
        }

        private void OnDisable()
        {
            if (_isEditing)
                Tools.hidden = false;
        }

        private void OnSceneGUI()
        {
            if (_splineNodePrefab == null) return;

            Event e = Event.current;
            _selector.UpdateSocketHover();
            _drawer.DrawSceneGUI();

            if (_isEditing)
                EditSceneGUI(e);
            else
                Tools.hidden = false;

            _drawer.RequestRepaint(e);
        }

        private void EditSceneGUI(Event e)
        {
            Tools.hidden = true;

            if (!_createHeld)
            {
                ManipulateSceneGUI();
                SelectionSceneGUI(e);
            }
        }

        private void ManipulateSceneGUI()
        {
            switch (Tools.current)
            {
                case Tool.Move:
                    _manipulator.HandleMovement();
                    break;
                case Tool.Rotate:
                    _manipulator.HandleRotation();
                    break;
            }
        }

        private void SelectionSceneGUI(Event e)
        {
            int prevCount = _selector.SelectedCount;
            var prevNodes = new HashSet<int>(_selector.SelectedSocketIndices);

            _selector.HandleSelectionInput(e);
            _manipulator.RefreshPivot();

            if (_selector.SelectedCount != prevCount || prevNodes.SetEquals(_selector.SelectedSocketIndices))
                Repaint();
        }

        public override void OnInspectorGUI()
        {
            SplineEditorSettings.DrawSplineNodePrefabSettingsGUI();
            DrawDefaultInspector();
            EditorGUILayout.Space(8);
            DrawEditGUI();
        }

        private void DrawEditGUI()
        {
            GUI.backgroundColor = _isEditing ? Color.green : Color.white;
            if (GUILayout.Button(_isEditing ? "Exit Edit Mode" : "Enter Edit Mode", GUILayout.Height(30)))
            {
                _isEditing = !_isEditing;
                Tools.hidden = _isEditing;
                _selector.ClearSelection();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Set Pivot", GUILayout.Height(30)))
            {
                ResetPivot();
                SceneView.RepaintAll();
            }
        }

        private void ResetPivot()
        {
            if (_selector.SelectedCount != 1) return;

            var socket = _splineNodePrefab.sockets[_selector.SelectedSocketIndices.First()];
            if (socket.localPosition == Vector3.zero && socket.localRotation == Quaternion.identity) return;

            var mf = _splineNodePrefab.GetComponent<MeshFilter>();
            if (mf?.sharedMesh == null) return;

            var mesh = mf.sharedMesh;
            var invMatrix = Matrix4x4.TRS(socket.localPosition, socket.localRotation, Vector3.one).inverse;
            var invRot = Quaternion.Inverse(socket.localRotation);

            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = invMatrix.MultiplyPoint3x4(vertices[i]);
            mesh.vertices = vertices;

            var normals = mesh.normals;
            if (normals is { Length: > 0 })
            {
                for (int i = 0; i < normals.Length; i++)
                    normals[i] = invRot * normals[i];
                mesh.normals = normals;
            }

            var tangents = mesh.tangents;
            if (tangents is { Length: > 0 })
            {
                for (int i = 0; i < tangents.Length; i++)
                {
                    var t = invRot * (Vector3)tangents[i];
                    tangents[i] = new Vector4(t.x, t.y, t.z, tangents[i].w);
                }
                mesh.tangents = tangents;
            }

            mesh.RecalculateBounds();

            var tform = _splineNodePrefab.transform;
            tform.position = tform.TransformPoint(socket.localPosition);
            tform.rotation *= socket.localRotation;

            socket.localPosition = Vector3.zero;
            socket.localRotation = Quaternion.identity;

            var mc = _splineNodePrefab.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.sharedMesh = null;
                mc.sharedMesh = mesh;
            }
        }

        #endregion Unity Callbacks

        #region Static


        #endregion Static
    }
}