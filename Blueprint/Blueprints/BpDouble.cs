using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Blueprint
{
    public class BpDouble : BaseBlueprint, IBpActionProcessor<DoubleAction>
    {
        public override BpType Type { get; set; } = BpType.Double;
        public override SelectionType SelectionType { get; set; } = SelectionType.PlayerOnly;
        public override int Lifespan { get; set; } = 1;
        public override int MaxSelectionAmount { get; set; } = 2;
        public DoubleAction BpAction { get; } = new DoubleAction();
        
        
        public override void TryTakeAction(int[] selectedItems)
        {
            BpAction.Execute(selectedItems);
        }

        public override void TryRestoreAction(int selectedItem)
        {
            BpAction.Restore(selectedItem);
        }
    }

}

