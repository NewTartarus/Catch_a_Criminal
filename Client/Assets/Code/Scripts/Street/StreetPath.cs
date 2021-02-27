using ScotlandYard.Enums;
using ScotlandYard.Interface;
using ScotlandYard.Scripts.Street.BezierCurve;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScotlandYard.Scripts.Street
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class StreetPath : PathCreator, IStreet
    {
        [SerializeField] protected GameObject startPoint;
        [SerializeField] protected GameObject endPoint;
        [Range(0, 5)]
        [SerializeField] protected float width = 2;
        [SerializeField] protected int additionalRows = 3;

        public GameObject StartPoint { get => startPoint; set => startPoint = value; }
        public GameObject EndPoint { get => endPoint; set => endPoint = value; }

        public float Width { get => width; set => width = value; }
        public int AdditionalRows { get => additionalRows; set => additionalRows = value; }

        [SerializeField] public ETicket[] Costs { get; set; }

        public void Init(Material material)
        {
            GameObject streetObject = new GameObject("StreetMesh", typeof(MeshFilter), typeof(MeshRenderer));

            List<Vector3> verticies = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();

            //add verticies and uv
            //verticies.AddRange(GetVerticiesOfPoint(StartPoint.GetComponent<StreetPoint>()));
            //uv.AddRange(GetUVOfPoint(StartPoint.GetComponent<StreetPoint>()));

            for (int i = 0; i < GetNumberOfWaypoints(); i++)
            {
                Transform wp = GetWaypoint(i);
                if (wp != null)
                {
                    Vector3 rightVert = wp.position + (wp.right * width * 0.5f);
                    Vector3 leftVert = wp.position - (wp.right * width * 0.5f);
                    Vector3 summandVector = (leftVert - rightVert) / (AdditionalRows + 1);

                    verticies.Add(rightVert);
                    uv.Add(rightVert);

                    for (int j = 0; j < AdditionalRows; j++)
                    {
                        verticies.Add(rightVert + ((j + 1) * summandVector));
                        uv.Add(rightVert + ((j + 1) * summandVector));
                    }

                    verticies.Add(leftVert);
                    uv.Add(leftVert);
                }
            }

            verticies.AddRange(GetVerticiesOfPoint(EndPoint.GetComponent<StreetPoint>()));
            //uv.AddRange(GetUVOfPoint(EndPoint.GetComponent<StreetPoint>()));

            //add triangles
            for (int i = 0; i < GetNumberOfWaypoints() - 1; i++)
            {
                int nextRec = i * (2 + AdditionalRows);
                for(int j = 0; j <= AdditionalRows; j++)
                {
                    //1, 0, 2
                    triangles.Add(1 + j + nextRec);
                    triangles.Add(0 + j + nextRec);
                    triangles.Add(2 + j + AdditionalRows + nextRec);

                    //1, 2, 3
                    triangles.Add(1 + j + nextRec);
                    triangles.Add(2 + j + AdditionalRows + nextRec);
                    triangles.Add(3 + j + AdditionalRows + nextRec);
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = verticies.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();

            streetObject.GetComponent<MeshFilter>().mesh = mesh;
            streetObject.GetComponent<MeshRenderer>().material = material;
        }

        protected virtual Vector3[] GetVerticiesOfPoint(StreetPoint streetPoint)
        {
            Vector3[] verticies = new Vector3[2];
            
            float radius = streetPoint?.Radius ?? 1f;
            Transform wp = streetPoint == StartPoint ? GetWaypoint(0) : GetWaypoint(GetNumberOfWaypoints()-1);
            float upperAngle = wp.eulerAngles.y * Mathf.PI / 180;
            float lowerAngle = (wp.eulerAngles.y + 180) * Mathf.PI / 180;
            if(streetPoint.transform.position.x >= wp.position.x)
            {
                verticies[0] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(upperAngle), 0, radius * Mathf.Sin(upperAngle));
                verticies[1] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(lowerAngle), 0, radius * Mathf.Sin(lowerAngle));
            }
            else
            {
                verticies[0] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(lowerAngle), 0, radius * Mathf.Sin(lowerAngle));
                verticies[1] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(upperAngle), 0, radius * Mathf.Sin(upperAngle));
            }

            return verticies;
        }

        protected virtual Vector2[] GetUVOfPoint(StreetPoint streetPoint)
        {
            Vector2[] uv = new Vector2[2];

            float radius = streetPoint?.Radius ?? 1f;
            Transform wp = streetPoint == StartPoint ? GetWaypoint(0) : GetWaypoint(GetNumberOfWaypoints()-1);
            float upperAngle = (wp.eulerAngles.y - 90)* Mathf.PI / 180;
            float lowerAngle = (wp.eulerAngles.y + 90) * Mathf.PI / 180;

            if (streetPoint.transform.position.x >= wp.position.x)
            {
                uv[0] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(upperAngle), 0, radius * Mathf.Sin(upperAngle));
                uv[1] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(lowerAngle), 0, radius * Mathf.Sin(lowerAngle));
            }
            else
            {
                uv[0] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(lowerAngle), 0, radius * Mathf.Sin(lowerAngle));
                uv[1] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(upperAngle), 0, radius * Mathf.Sin(upperAngle));
            }

            return uv;
        }

        public virtual int GetNumberOfWaypoints()
        {
            return this.transform.childCount;
        }

        public virtual Transform GetWaypoint(int i)
        {
            if (this.transform.childCount == 0 && StartPoint == null && EndPoint == null)
            {
                return null;
            }

            if (i == -1)
            {
                return StartPoint.transform;
            }
            else if (i == this.transform.childCount)
            {
                return EndPoint.transform;
            }

            Transform trans = this.transform.GetChild(i);
            return trans.CompareTag("WayPoint") ? trans : null;
        }

        public virtual Transform GetPathsTransform()
        {
            return this.transform; // for getting childs: foreach(Transform trans in this.transform)
        }

        public virtual ETicket[] ReturnTicketCost()
        {
            return Costs;
        }
    }
}

