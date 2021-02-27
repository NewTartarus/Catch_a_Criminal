using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScotlandYard.Scripts.Street.BezierCurve
{
    public class PathCreator : MonoBehaviour
    {
        [HideInInspector] protected BezierPath path;
        public BezierPath Path
        {
            get => path;
            set => path = value;
        }

        [SerializeField] protected Color anchorColor = Color.red;
        [SerializeField] protected Color controlColor = Color.white;
        [SerializeField] protected Color segmentColor = Color.green;
        [SerializeField] protected Color selectedSegmentColor = Color.yellow;
        [SerializeField] protected float anchorDiameter = 0.1f;
        [SerializeField] protected float controlDiameter = 0.08f;
        [SerializeField] protected bool displayControlPoints = true;

        public Color AnchorColor => anchorColor;
        public Color ControlColor => controlColor;
        public Color SegmentColor => segmentColor;
        public Color SelectedSegmentColor => selectedSegmentColor;
        public float AnchorDiameter => anchorDiameter;
        public float ControlDiameter => controlDiameter;
        public bool DisplayControlPoints => displayControlPoints;

        public void CreatePath()
        {
            path = new BezierPath(transform.position);
        }

        protected void Reset()
        {
            CreatePath();
        }
    }
}
