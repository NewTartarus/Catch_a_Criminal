using ScotlandYard.Scripts.Street.BezierCurve;
using UnityEditor;
using UnityEngine;

namespace ScotlandYard.Editor
{
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : UnityEditor.Editor
    {
        PathCreator creator;
        BezierPath Path
        {
            get => creator.Path;
        }

        const float segmentSelectDistanceThreshold = 0.1f;
        int selectedSegmentIndex = -1;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Create new"))
            {
                Undo.RecordObject(creator, "Add new path");
                creator.CreatePath();
            }

            bool isClosed = GUILayout.Toggle(Path.IsClosed, "Closed");
            if (isClosed != Path.IsClosed)
            {
                Undo.RecordObject(creator, "Toggle closed");
                Path.IsClosed = isClosed;
            }

            bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto Set Control Points");
            if(autoSetControlPoints != Path.AutoSetControlPoints)
            {
                Undo.RecordObject(creator, "Toggle auto set controls");
                Path.AutoSetControlPoints = autoSetControlPoints;
            }

            if(EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        protected void OnSceneGUI()
        {
            Input();
            Draw();
        }

        void Input()
        {
            Event guiEvent = Event.current;
            Vector3 mousePos = new Vector3(HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin.x, 0f, HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin.z);

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if(selectedSegmentIndex != -1)
                {
                    Undo.RecordObject(creator, "Split segment");
                    Path.SplitSegment(mousePos, selectedSegmentIndex);
                }
                else if (!Path.IsClosed)
                {
                    Undo.RecordObject(creator, "Add segment");
                    Path.AddSegment(mousePos);
                }
            }

            if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                float minDistanceToAnchor = creator.AnchorDiameter * 0.5f;
                int closestAnchorIndex = -1;

                for (int i = 0; i < Path.NumPoints; i+=3)
                {
                    float distance = Vector3.Distance(mousePos, Path[i]);
                    if(distance < minDistanceToAnchor)
                    {
                        minDistanceToAnchor = distance;
                        closestAnchorIndex = i;
                    }
                }

                if(closestAnchorIndex != -1)
                {
                    Undo.RecordObject(creator, "Delete segment");
                    Path.DeleteSegment(closestAnchorIndex);
                }
            }

            if (guiEvent.type == EventType.MouseMove)
            {
                float minDstToSegment = segmentSelectDistanceThreshold;
                int newSelectedSegmentIndex = -1;

                for (int i = 0; i < Path.NumSegments; i++)
                {
                    Vector3[] points = Path.GetPointsInSegment(i);
                    float distance = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                    if (distance < minDstToSegment)
                    {
                        minDstToSegment = distance;
                        newSelectedSegmentIndex = i;
                    }
                }

                if(newSelectedSegmentIndex != this.selectedSegmentIndex)
                {
                    this.selectedSegmentIndex = newSelectedSegmentIndex;
                    HandleUtility.Repaint();
                }
            }

            HandleUtility.AddDefaultControl(0);
        }

        protected void Draw()
        {
            for (int i = 0; i < Path.NumSegments; i++)
            {
                Vector3[] points = Path.GetPointsInSegment(i);

                if(creator.DisplayControlPoints)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1], points[0]);
                    Handles.DrawLine(points[2], points[3]);
                }

                Color segmentColor = (i == selectedSegmentIndex && Event.current.shift) ? creator.SelectedSegmentColor : creator.SegmentColor;
                Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);
            }

            for (int i = 0; i < Path.NumPoints; i++)
            {
                bool isAnchorPoint = i % 3 == 0;

                if (isAnchorPoint || creator.DisplayControlPoints)
                {
                    Handles.color = isAnchorPoint ? creator.AnchorColor : creator.ControlColor;
                    float diameter = isAnchorPoint ? creator.AnchorDiameter : creator.ControlDiameter;

                    Vector3 newPos = Handles.FreeMoveHandle(Path[i], Quaternion.identity, diameter, Vector3.zero, Handles.CylinderHandleCap);
                    if (Path[i] != newPos)
                    {
                        Undo.RecordObject(creator, "Move point");
                        Path.MovePoint(i, newPos);
                    }
                }
            }
        }

        protected void OnEnable()
        {
            creator = target as PathCreator;
            if(creator.Path == null)
            {
                creator.CreatePath();
            }
        }
    }
}
