using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataModels;
using Enums;
using Network;
using UnityEngine;

namespace Blueprint
{
    public class BlueprintManager : MonoBehaviour
    {
        public List<BpType> activeBlueprints = new();
        private BaseBlueprint currentBlueprint;

        private BpHolder bpHolder = new BpHolder();

        public BPSlotHolder slotHolder;
        public BPDataHolder bpDataHolder;
        public BpTrackerList bpTrackerList = new ();

        
        public void Subscribe()
        {
            TurnStatusEvents.OnTurnEnding += UpdateBpTrackers;

            BpEventbus.UIEvents.OnInteraction += StartBpSelection; //todo: Daha sonra, (datadaki değişkenleri ayırdıktan sonra) network obj olarak data gönderilir yaparız
            
            NetworkEventbus.RequestEvents.OnBpSelectionByServer += SetCurrentBpByServer;
            
            NetworkEventbus.RequestEvents.OnBpExecutionBySystem += ExecuteBp;
            BpEventbus.OnBpExecution += ExecuteBp;
            
            BpEventbus.LifespanEvents.OnRestore += RestoreFromBp;
            BpEventbus.LifespanEvents.OnExpiredTracker += RemoveExpiredBp;
            bpTrackerList.Subscribe();
        }

        private void StartBpSelection(BpType type, int level)
        {
            StartCoroutine(BpSelectionDelay(type, level));
        }

        IEnumerator BpSelectionDelay(BpType type, int level) //On Interaction : calls network
        {
            NetworkEventbus.TriggerEvents.OnStateChangeRequestByUser.Invoke(TurnStateType.Intruder);
            yield return new WaitForSeconds(.2f);
            NetworkEventbus.TriggerEvents.OnBpSelectionRequestByUser?.Invoke(type, level);

        }
        
        private void SetCurrentBpByServer(BpType type,int level) //network call
        {
            currentBlueprint = bpHolder.AllBlueprints[type];
            BpEventbus.UIEvents.OnBpInstallBegin?.Invoke(type);

            currentBlueprint.Level = level;
            
            print(currentBlueprint);

            BpEventbus.SelectionEvents.OnCurrentBpSet?.Invoke(currentBlueprint.SelectionType, currentBlueprint.MaxSelectionAmount);
        }

       
        private void UpdateBpTrackers()
        {
            bpTrackerList.ReduceValueForAll();
        }
        private void RemoveExpiredBp(ITrackable lifeTracker)
        {
            bpTrackerList.RemoveFromTrackList(lifeTracker);
        }

        private void ExecuteBp(int[] selectedItems)
        {
            currentBlueprint.TryTakeAction(selectedItems);
            SetTracker(selectedItems);
        }

        void SetTracker(int[] selectedItems)
        {
            if(selectedItems == null) return; //TODO: ya des trackers sans items
            
            foreach (var item in selectedItems)
            {
                var tracker = bpTrackerList.CreateTracker(currentBlueprint.Lifespan, item, currentBlueprint.Type);
                BpEventbus.LifespanEvents.OnTrackerRequest?.Invoke(tracker);
            }
        }

        private void RestoreFromBp(BpType type, int selectedItem)
        {
           bpHolder.AllBlueprints[type].TryRestoreAction(selectedItem); //todo: bug. sadece 3 tane bp var. ama aynı bpnin birden fazla kullanımı olmalı, ve selected itemlerı farklı olmalı
        }

      
        
        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            Subscribe();
            bpHolder.Initialize();
            GetActiveBlueprints();

            slotHolder.Setup(activeBlueprints);
        }

        public void GetActiveBlueprints()
        {
            for (int i = 0; i < 3; i++) //TODO: Temp
            {
                activeBlueprints.Add(bpHolder.AllBlueprints.Keys.ElementAt(i));
            }
        }


        public void Unsubscribe()
        {
            BpEventbus.OnBpExecution -= ExecuteBp;

            BpEventbus.UIEvents.OnInteraction -= StartBpSelection;
            TurnStatusEvents.OnTurnEnding -= UpdateBpTrackers;
            NetworkEventbus.RequestEvents.OnBpSelectionByServer -= SetCurrentBpByServer;
            NetworkEventbus.RequestEvents.OnBpExecutionBySystem -= ExecuteBp;
            BpEventbus.LifespanEvents.OnRestore -= RestoreFromBp;
            BpEventbus.LifespanEvents.OnExpiredTracker -= RemoveExpiredBp;
            bpTrackerList.Unsubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}