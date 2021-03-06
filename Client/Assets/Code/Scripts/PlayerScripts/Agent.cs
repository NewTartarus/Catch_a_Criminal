﻿using ScotlandYard.Enums;
using ScotlandYard.Events;
using ScotlandYard.Interface;
using ScotlandYard.Scripts.Street;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScotlandYard.Scripts.PlayerScripts
{
    public abstract class Agent : MonoBehaviour
    {
        //Fields
        [SerializeField] protected PlayerData data;
        [SerializeField] protected float speed;
        protected GameObject position;
        protected IStreet streetPath;
        protected bool isMoving = false;
        [SerializeField] protected MeshRenderer indicator;

        //Properties
        public virtual PlayerData Data
        {
            get
            {
                if(data == null)
                {
                    data = new PlayerData();
                }

                return data;
            }
        }

        public virtual IStreet StreetPath
        {
            get => streetPath;
            set => streetPath = value;
        }

        public virtual GameObject Position
        {
            get => position;
            set
            {
                if(value != position)
                {
                    if(Data.PlayerType != EPlayerType.MISTERX)
                    {
                        if(position != null)
                        {
                            position.GetComponent<StreetPoint>().IsOccupied = false;
                        }
                        value.GetComponent<StreetPoint>().IsOccupied = true;
                    }

                    position = value;
                    Data.CurrentPosition = position.GetComponent<StreetPoint>();
                }
            }
        }

        public virtual void Init()
        {
            GeneratePlayerId();
            indicator.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            indicator.material.SetColor("_BaseColor", Data.PlayerColor);
        }

        public virtual void GeneratePlayerId()
        {
            int id = int.Parse(Math.Abs(name.GetHashCode() * DateTime.Now.Millisecond).ToString().Substring(0, 5));
            this.Data.ID = $"{this.Data.AgentName}#{id}";
        }

        public virtual void BeginRound()
        {
            throw new NotImplementedException("This Agent has nothing to do in this Round. Maybe implement the Method.");
        }

        #region Movement
        public virtual IEnumerator Move(IStreet path)
        {
            if (isMoving == false)
            {
                isMoving = true;
                StopCoroutine(nameof(MoveForwards));
                StopCoroutine(nameof(MoveBackwards));

                if (this.Position.Equals(path.StartPoint))
                {
                    yield return StartCoroutine(nameof(MoveForwards), path);
                    this.Position = path.EndPoint;
                }
                else if (this.Position.Equals(path.EndPoint))
                {
                    yield return StartCoroutine(nameof(MoveBackwards), path);
                    this.Position = path.StartPoint;
                }
                transform.rotation = Quaternion.AngleAxis(0f, Vector3.down);

                yield return new WaitForSeconds(0.2f);

                Debug.Log($"{Data.AgentName} reached the Destination {this.Position.GetComponent<StreetPoint>().name}\n"
                    + $"Tickets left: Taxi = {Data.Tickets[ETicket.TAXI]}, Bus = {Data.Tickets[ETicket.BUS]}, Underground = {Data.Tickets[ETicket.UNDERGROUND]}");
                this.StreetPath = null;

                isMoving = false;
                GameEvents.Current.PlayerMoveFinished(this, new PlayerEventArgs(this.Data));
            }

        }

        protected IEnumerator MoveForwards(IStreet path)
        {
            for (int i = -1; i <= path.GetNumberOfWaypoints(); i++)
            {
                while (Vector3.Distance(transform.position, path.GetWaypoint(i).position) > 0.05f)
                {
                    MoveOneStep(path, i);

                    yield return null;
                }
            }
        }

        protected IEnumerator MoveBackwards(IStreet path)
        {
            for (int i = path.GetNumberOfWaypoints(); i >= -1; i--)
            {
                while (Vector3.Distance(transform.position, path.GetWaypoint(i).position) > 0.05f)
                {
                    MoveOneStep(path, i);

                    yield return null;
                }
            }
        }

        protected void MoveOneStep(IStreet path, int index)
        {
            transform.position = Vector3.MoveTowards(transform.position, path.GetWaypoint(index).position, speed * Time.deltaTime);

            Vector3 dir = path.GetWaypoint(index).position - transform.position;
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.down);
        }
        #endregion

        // Tickets
        public virtual Dictionary<ETicket, int> GetTickets()
        {
            return this.Data.Tickets;
        }

        public int GetTicketCount(ETicket ticket)
        {
            if (HasTicket(ticket))
            {
                return this.Data.Tickets[ticket];
            }

            return 0;
        }

        public virtual void AddTickets(ETicket ticket, int amount)
        {
            if (Data.Tickets.ContainsKey(ticket))
            {
                Data.Tickets[ticket] += amount;
            }
            else
            {
                Data.Tickets.Add(ticket, amount);
            }
        }

        public virtual bool HasTicket(ETicket ticket)
        {
            if (Data.Tickets.ContainsKey(ticket) && Data.Tickets[ticket] > 0)
            {
                return true;
            }

            return false;
        }

        public virtual void RemoveTicket(ETicket ticket)
        {
            if (HasTicket(ticket))
            {
                Data.Tickets[ticket]--;

                if (Data.PlayerType == EPlayerType.DETECTIVE)
                {
                    GameEvents.Current.DetectiveTicketRemoved(this, ticket);
                }
                else if (Data.PlayerType == EPlayerType.MISTERX)
                {
                    GameEvents.Current.TicketUpdated(this, new TicketUpdateEventArgs(this.Data));
                }
            }
        }
    }
}
