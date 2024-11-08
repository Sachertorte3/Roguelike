namespace Domain.Model.Character
{
    public static class ElementExtension
    {
        public static string Name(this Element element) => element switch
        {
            Element.Physical => "物理",
            Element.Fire => "火",
            Element.Ice => "氷",
            Element.Thunder => "雷",
            Element.Light => "光",
            Element.Dark => "闇",
        };
    }
}