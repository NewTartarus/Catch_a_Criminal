using ScotlandYard.Enums;
using ScotlandYard.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScotlandYard.Scripts.Street
{
    public class StreetContainer : MonoBehaviour
    {
        //IStreet
        [SerializeField] protected GameObject startPoint;
        [SerializeField] protected GameObject endPoint;
        [SerializeField] protected ETicket[] cost;


        //StreetPath
        [Range(0, 5)]
        [SerializeField] protected float width = 2;
        [SerializeField] protected Color waypointColor;


        //Container
        protected IStreet instance;
        public IStreet Instance
        {
            get
            {
                if(this.instance == null)
                {
                    InitInstance();
                }

                return this.instance;
            }
        }
        [SerializeField] protected EStreetType type;

        protected virtual void InitInstance()
        {
            
            switch (type)
            {
                case EStreetType.ROUTE:
                    instance = gameObject.AddComponent<Route>();
                    break;
                case EStreetType.STREET:
                    instance = gameObject.AddComponent<StreetPath>();
                    break;
                default:
                    instance = gameObject.AddComponent<StreetPath>();
                    break;
            }

            this.Instance.EndPoint = this.endPoint;
            this.Instance.StartPoint = this.startPoint;
            this.Instance.Costs = this.cost;

            if (this.Instance is StreetPath street)
            {
                street.Width = width;
            }
        }


#if UNITY_EDITOR
        protected virtual Transform GetWaypoint(int i)
        {
            if (this.transform.childCount == 0 && startPoint == null && endPoint == null)
            {
                return null;
            }

            if (i == -1)
            {
                return startPoint.transform;
            }
            else if (i == this.transform.childCount)
            {
                return endPoint.transform;
            }

            Transform trans = this.transform.GetChild(i);
            return trans.CompareTag("WayPoint") ? trans : null;
        }

        protected virtual void OnDrawGizmos()
        {
            if(type == EStreetType.STREET)
            {
                int childCount = this.transform.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    Transform wp = GetWaypoint(i);

                    if (wp != null)
                    {
                        Vector3 vectorRed = wp.position + (wp.right * width * 0.5f);
                        Vector3 vectorGreen = wp.position - (wp.right * width * 0.5f);

                        #region Draw Start- and EndPoint
                        if (i == 0)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(startPoint.transform.position, wp.position);

                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(vectorRed, GetVerticiesOfPoint(startPoint.GetComponent<StreetPoint>())[0]);
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(vectorGreen, GetVerticiesOfPoint(startPoint.GetComponent<StreetPoint>())[1]);

                            Gizmos.color = new Color32(200, 0, 0, 170);
                            Gizmos.DrawCube(startPoint.transform.position, Vector3.one * 0.5f);
                        }
                        else if (i == childCount - 1)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(GetWaypoint(i).position, endPoint.transform.position);

                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(vectorRed, GetVerticiesOfPoint(endPoint.GetComponent<StreetPoint>())[0]);
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(vectorGreen, GetVerticiesOfPoint(endPoint.GetComponent<StreetPoint>())[1]);

                            Gizmos.color = new Color32(200, 0, 0, 170);
                            Gizmos.DrawCube(endPoint.transform.position, Vector3.one * 0.5f);
                        }
                        #endregion

                        if (i < childCount - 1 && GetWaypoint(i + 1) is Transform posB)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(wp.position, posB.position);

                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(vectorRed, posB.position + (posB.right * width * 0.5f));
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(vectorGreen, posB.position - (posB.right * width * 0.5f));
                        }
                        Gizmos.color = Color.black;
                        Gizmos.DrawLine(vectorRed, vectorGreen);

                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(vectorRed, 0.1f);
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(vectorGreen, 0.1f);

                        Gizmos.color = waypointColor;
                        Gizmos.DrawSphere(wp.position, 0.2f);
                    }
                }

                Vector3[] vectors = GetVerticiesOfPoint(startPoint.GetComponent<StreetPoint>());

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(vectors[0], 0.1f);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(vectors[1], 0.1f);

                vectors = GetVerticiesOfPoint(endPoint.GetComponent<StreetPoint>());

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(vectors[0], 0.1f);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(vectors[1], 0.1f);
            }
        }

        protected virtual Vector3[] GetVerticiesOfPoint(StreetPoint streetPoint)
        {
            Vector3[] verticies = new Vector3[2];

            float radius = streetPoint?.Radius ?? 1f;
            Transform wp = streetPoint.Equals(startPoint) ? GetWaypoint(0) : GetWaypoint(this.transform.childCount-1);
            var pointAngle = streetPoint.transform.eulerAngles.y;
            float upperAngle = (wp.eulerAngles.y - pointAngle) * Mathf.PI / 180;
            float lowerAngle = (wp.eulerAngles.y + 180 - pointAngle) * Mathf.PI / 180;

            if ((streetPoint.transform.position.x < wp.position.x && streetPoint.transform.position.z < wp.position.z)
                || (streetPoint.transform.position.x > wp.position.x && streetPoint.transform.position.z > wp.position.z))
            {
                verticies[0] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(upperAngle), 0, radius * Mathf.Sin(upperAngle));
                verticies[1] = streetPoint.transform.position + new Vector3(radius * Mathf.Cos(lowerAngle), 0, radius * Mathf.Sin(lowerAngle));
            }
            else
            {
                verticies[0] = streetPoint.transform.position + new Vector3(radius* Mathf.Cos(lowerAngle), 0, radius* Mathf.Sin(lowerAngle));
                verticies[1] = streetPoint.transform.position + new Vector3(radius* Mathf.Cos(upperAngle), 0, radius* Mathf.Sin(upperAngle));
            }

            return verticies;
        }
#endif
    }
}
