namespace Domain.Model.Entity
{
    public enum EntityLayer
    {
        Bottom,
        Middle,
        Top,
        /// <summary>床の固定オブジェクト（罠・階段・テレポーター等）。<see cref="Bottom"/> の上に重ねられる。</summary>
        Floor
    }
}