using Towers;

namespace _Core.Turn.Selectors
{
    public interface ISelectionColorSetter
    {
        public void SetColor(int selection);
    }
    
    public class StandardSelectionColor : ISelectionColorSetter
    {
        public void SetColor(int selection)
        {
            AllTowers.GetTower(selection).ToSelectionColor();
        }
    }
    
    public class BpSelectionColor : ISelectionColorSetter
    {
        public void SetColor(int selection)
        {
            AllTowers.GetTower(selection).ToBlueprintColor();
        }
    }
}