using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.Code.Scripts.Street.BezierCurve;

[CustomEditor(typeof(StreetCreator))]
public class RoadEditor : Editor
{
    StreetCreator creator;

    private void OnSceneGUI()
    {
        if(creator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creator.UpdateRoad();
        }
    }
    private void OnEnable()
    {
        creator = (StreetCreator)target;
    }
}
