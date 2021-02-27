using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Timeline;

namespace ScotlandYard.Scripts.Street.BezierCurve
{
    [Serializable]
    public class BezierPath
    {
        [SerializeField, HideInInspector] List<Vector3> points;
        [SerializeField, HideInInspector] bool isClosed;
        [SerializeField, HideInInspector] bool autoSetControlPoints;

        public Vector3 this[int i]
        {
            get => points[i];
        }

        public bool AutoSetControlPoints
        {
            get => autoSetControlPoints;
            set
            {
                if(autoSetControlPoints != value)
                {
                    autoSetControlPoints = value;
                    if(autoSetControlPoints)
                    {
                        AutoSetAllControlPoints();
                    }
                }
            }
        }

        public bool IsClosed
        {
            get => isClosed;
            set
            {
                if(isClosed != value)
                {
                    isClosed = value;

                    if (isClosed)
                    {
                        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                        points.Add(points[0] * 2 - points[1]);
                        if (autoSetControlPoints)
                        {
                            AutoSetAnchorControlPoints(0);
                            AutoSetAnchorControlPoints(points.Count - 3);
                        }
                    }
                    else
                    {
                        points.RemoveRange(points.Count - 2, 2);
                        if (autoSetControlPoints)
                        {
                            AutoSetStartAndEndControls();
                        }
                    }
                }
            }
        }

        public int NumPoints
        {
            get => points.Count;
        }

        public int NumSegments
        {
            get
            {
                return points.Count / 3;
            }
        }

        public BezierPath(Vector3 centre)
        {
            points = new List<Vector3>
            {
                centre + Vector3.left,
                centre + (Vector3.left + Vector3.up) * 0.5f,
                centre + (Vector3.right + Vector3.down) * 0.5f,
                centre + Vector3.right
            };
        }

        public void AddSegment(Vector3 anchorPos)
        {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add((points[points.Count - 1] + anchorPos) / 2);
            points.Add(anchorPos);

            if (autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(points.Count - 1);
            }
        }

        public void SplitSegment(Vector3 anchorPos, int segmentIndex)
        {
            points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
            if(autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
            }
            else
            {
                AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
            }
        }

        public void DeleteSegment(int anchorIndex)
        {
            if (NumSegments > 2 || (!isClosed && NumSegments > 1))
            {
                if (anchorIndex == 0)
                {
                    if (isClosed)
                    {
                        points[points.Count - 1] = points[2];
                    }
                    points.RemoveRange(0, 3);
                }
                else if (anchorIndex == points.Count - 1 && !isClosed)
                {
                    points.RemoveRange(anchorIndex - 2, 3);
                }
                else
                {
                    points.RemoveRange(anchorIndex - 1, 3);
                }
            }
        }

        public Vector3[] GetPointsInSegment(int i)
        {
            return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
        }

        public void MovePoint(int i, Vector3 position)
        {
            Vector3 deltaMove = position - points[i];

            if(i % 3 == 0 || !autoSetControlPoints)
            {
                points[i] = position;
            }
            
            if(autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(i);
            }
            else
            {
                if (i % 3 == 0)
                {
                    if (i + 1 < points.Count || isClosed)
                    {
                        points[LoopIndex(i + 1)] += deltaMove;
                    }

                    if (i - 1 >= 0 || isClosed)
                    {
                        points[LoopIndex(i - 1)] += deltaMove;
                    }
                }
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
                    int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;

                    if ((correspondingControlIndex >= 0 && correspondingControlIndex < points.Count) || isClosed)
                    {
                        float distance = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)]).magnitude;
                        Vector3 direction = (points[LoopIndex(anchorIndex)] - position).normalized;
                        points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)] + direction * distance;
                    }
                }
            }
        }

        public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
        {
            List<Vector3> evenlySpacedPoints = new List<Vector3>();

            evenlySpacedPoints.Add(points[0]);
            Vector3 previousPoint = points[0];
            float distanceSinceLastEvenPoint = 0;

            for (int segementIndex = 0; segementIndex < NumSegments; segementIndex++)
            {
                Vector3[] pointsInSeg = GetPointsInSegment(segementIndex);
                float controlNetLength = Vector3.Distance(pointsInSeg[0], pointsInSeg[1]) + Vector3.Distance(pointsInSeg[1], pointsInSeg[2]) + Vector3.Distance(pointsInSeg[2], pointsInSeg[3]);
                float estimatedCurveLength = Vector3.Distance(pointsInSeg[0], pointsInSeg[3]) + controlNetLength / 2f;
                int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
                float t = 0;
                while(t <= 1)
                {
                    t += 1f/divisions;
                    Vector3 pointOnCurve = Bezier.EvaluateCubic(pointsInSeg[0], pointsInSeg[1], pointsInSeg[2], pointsInSeg[3], t);
                    distanceSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                    while(distanceSinceLastEvenPoint >= spacing)
                    {
                        float difference = distanceSinceLastEvenPoint - spacing;
                        Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * difference;

                        evenlySpacedPoints.Add(newEvenlySpacedPoint);

                        distanceSinceLastEvenPoint = difference;
                        previousPoint = newEvenlySpacedPoint;
                    }

                    previousPoint = pointOnCurve;
                }
            }

            return evenlySpacedPoints.ToArray();
        }

        protected void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
        {
            for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex+3; i+=3)
            {
                if(i >= 0 && i < points.Count || isClosed)
                {
                    AutoSetAnchorControlPoints(LoopIndex(i));
                }
            }

            AutoSetStartAndEndControls();
        }

        protected void AutoSetAllControlPoints()
        {
            for (int i = 0; i < points.Count; i+=3)
            {
                AutoSetAnchorControlPoints(i);
            }
            
            AutoSetStartAndEndControls();
        }

        protected void AutoSetAnchorControlPoints(int anchorIndex)
        {
            Vector3 anchorPos = points[anchorIndex];
            Vector3 direction = Vector3.zero;
            float[] neighbourDistances = new float[2];

            if(anchorIndex - 3 >= 0 || isClosed)
            {
                Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
                direction += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }

            if (anchorIndex + 3 >= 0 || isClosed)
            {
                Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
                direction -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }

            direction.Normalize();

            for (int i = 0; i < 2; i++)
            {
                int controlIndex = anchorIndex + i * 2 - 1;
                if ((controlIndex >= 0 && controlIndex < points.Count) || isClosed)
                {
                    points[LoopIndex(controlIndex)] = anchorPos + direction * neighbourDistances[i] * 0.5f;
                }
            }
        }

        protected void AutoSetStartAndEndControls()
        {
            if(!isClosed)
            {
                points[1] = (points[0] + points[2]) * 0.5f;
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
            }
        }

        protected int LoopIndex(int i)
        {
            return (i + points.Count) % points.Count;
        }
    }
}
