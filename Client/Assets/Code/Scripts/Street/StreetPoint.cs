using ScotlandYard.Enums;
using ScotlandYard.Interface;
using ScotlandYard.Scripts.PlayerScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScotlandYard.Scripts.Street
{
    public class StreetPoint : MonoBehaviour
    {
        public new string name;
        [SerializeField] protected TextMeshPro text;
        [SerializeField] protected GameObject highlightMesh;

        [SerializeField] protected int verticesCount = 40;
        [SerializeField] protected float radius = 1f;

        [SerializeField] protected List<IStreet> streetList = new List<IStreet>();

        #region Properties
        private bool _highlighted;
        public bool IsHighlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                highlightMesh.SetActive(_highlighted);
            }
        }

        public float Radius
        {
            get => radius;
            set
            {
                if(radius != value)
                {
                    radius = value;
                }
            }
        }
        #endregion

        public void Init(Material material)
        {
            if (text != null)
            {
                text.text = name;
            }

            GameObject pointObject = new GameObject($"StreetPoint({name})", typeof(MeshFilter), typeof(MeshRenderer));

            List<Vector3> verticies = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();

            //add verticies
            float deltaTheta = (2f * Mathf.PI) / verticesCount;
            float theta = 0f;
            var tran = this.gameObject.transform;

            verticies.Add(tran.position);
            uv.Add(tran.position);

            for (int i = 0; i < verticesCount; i++)
            {
                Vector3 pos = new Vector3(radius * Mathf.Cos(theta), 0f, radius * Mathf.Sin(theta));
                
                verticies.Add(tran.position + pos);
                uv.Add(tran.position + pos);

                theta += deltaTheta;
            }

            for(int i = 1; i < verticies.Count; i++)
            {
                if(i < verticesCount)
                {
                    triangles.Add(i + 1);
                    triangles.Add(i);
                    triangles.Add(0);
                }
                else
                {
                    triangles.Add(1);
                    triangles.Add(i);
                    triangles.Add(0);
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = verticies.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();

            pointObject.GetComponent<MeshFilter>().mesh = mesh;
            pointObject.GetComponent<MeshRenderer>().material = material;
        }

        public List<GameObject> GetStreetTargets(Player player)
        {
            List<GameObject> targets = new List<GameObject>();
            foreach (IStreet street in streetList)
            {
                bool playerHasTicket = false;
                foreach(ETicket ticket in street.ReturnTicketCost())
                {
                    if(player.HasTicket(ticket))
                    {
                        playerHasTicket = true;
                        break;
                    }
                }

                if(playerHasTicket)
                {
                    targets.Add(!street.StartPoint.Equals(this.gameObject) ? street.StartPoint : street.EndPoint);
                }
            }

            return targets;
        }

        internal IStreet GetPathByPosition(GameObject position, GameObject target)
        {
            foreach (IStreet path in streetList)
            {
                if ((position.Equals(path.StartPoint) && target.Equals(path.EndPoint)) || (position.Equals(path.EndPoint) && target.Equals(path.StartPoint)))
                {
                    return path;
                }
            }

            return null;
        }

        public void AddStreet(IStreet path)
        {
            streetList.Add(path);
        }

        public IStreet[] GetStreetArray()
        {
            return streetList.ToArray();
        }

        public GameObject GetGameObject()
        {
            return this.gameObject;
        }

        public override bool Equals(object other)
        {
            StreetPoint pointB = null;
            if (other is StreetPoint streetPoint)
            {
                pointB = streetPoint;
            }
            else if (other is GameObject go)
            {
                pointB = go.GetComponent<StreetPoint>();
            }
            
            if(pointB != null && pointB.name == this.name)
            {
                if(pointB.transform.position == this.transform.position)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }
    }
}

