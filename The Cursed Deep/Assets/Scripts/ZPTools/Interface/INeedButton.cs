namespace ZPTools.Interface
{
    public interface INeedButton
    {
        System.Collections.Generic.List<(System.Action, string)> GetButtonActions();
    }
}