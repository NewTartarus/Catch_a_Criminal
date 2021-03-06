﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScotlandYard.Scripts.Street
{
    public class StreetController : MonoBehaviour
    {
        [SerializeField] protected List<StreetContainer> streetList;
        [SerializeField] protected List<StreetPoint> streetPoints;

        public void Init()
        {
            //Adds IStreet-Object to the corresponding Point-Object
            foreach (StreetContainer container in streetList)
            {
                container.Instance.StartPoint.GetComponent<StreetPoint>()?.AddStreet(container.Instance);
                container.Instance.EndPoint.GetComponent<StreetPoint>()?.AddStreet(container.Instance);
            }
        }

        public GameObject[] GetRandomPositions(int amount)
        {
            GameObject[] positions = new GameObject[amount];
            List<int> pointsAlreadyUsed = new List<int>();

            
            for(int i = 0; i < amount; i++)
            {
                var random = new System.Random();
                pointsAlreadyUsed.Add(GetRandomUniqueInt(pointsAlreadyUsed,random));
                positions[i] = streetPoints[pointsAlreadyUsed[i]].GetGameObject();
            }

            return positions;
        }

        protected virtual int GetRandomUniqueInt(List<int> intsInUse, System.Random random)
        {
            int randomInt = random.Next(0, streetPoints.Count);

            if (!intsInUse.Contains(randomInt))
            {
                return randomInt;
            }

            return GetRandomUniqueInt(intsInUse, random);
        }

        [ContextMenu("AutoFill StreetPoints")]
        protected void AutoFillStreetPoints()
        {
            streetPoints = FindObjectsOfType<StreetPoint>().OrderBy(s => s.name).ToList();
        }

        [ContextMenu("AutoFill Streets")]
        protected void AutoFillStreets()
        {
            streetList = FindObjectsOfType<StreetContainer>().OrderBy(s => s.name).ToList();
        }
    }
}
