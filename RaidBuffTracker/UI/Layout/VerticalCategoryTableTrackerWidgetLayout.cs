using System.Numerics;

namespace RaidBuffTracker.UI.Layout;

public sealed class VerticalCategoryTableTrackerWidgetLayout : ITrackerWidgetLayout
{
    // Token: 0x0600009B RID: 155 RVA: 0x000049B1 File Offset: 0x00002BB1
    public void StartOver(Vector2 cellSize, Vector2 cellMargin, Vector2 headerSize, Vector2 viewportSize)
    {
        _cellSize = cellSize;
        _cellMargin = cellMargin;
        _headerSize = headerSize;
        _viewportSize = viewportSize;
        _pos = new Vector2(cellMargin.X, cellMargin.Y);
    }

    // Token: 0x0600009C RID: 156 RVA: 0x000049E8 File Offset: 0x00002BE8
    public Vector2 GetPos(Vector2 offset = default(Vector2))
    {
        return _pos + offset;
    }

    // Token: 0x0600009D RID: 157 RVA: 0x00004A08 File Offset: 0x00002C08
    public void AdvanceToNextCell()
    {
        bool flag = _pos.Y + _cellSize.Y * 2f + _cellMargin.Y * 2f > _viewportSize.Y;
        if (flag)
        {
            _pos.Y = _cellMargin.X;
            _pos.X = _pos.X + (_cellSize.X + _cellMargin.X);
        }
        else
        {
            _pos.Y = _pos.Y + (_cellSize.Y + _cellMargin.Y);
        }
    }

    // Token: 0x0600009E RID: 158 RVA: 0x00004AC0 File Offset: 0x00002CC0
    public void AdvancePastHeader()
    {
        _pos.Y = _pos.Y + (_cellSize.Y / 2f + _headerSize.Y / 2f + _cellMargin.X);
    }

    // Token: 0x0600009F RID: 159 RVA: 0x00004B0C File Offset: 0x00002D0C
    public void AdvanceToNextSection()
    {
        _pos.Y = _cellMargin.Y;
        _pos.X = _pos.X + (_cellSize.X + _cellMargin.X);
    }

    // Token: 0x040000B0 RID: 176
    private Vector2 _pos;

    // Token: 0x040000B1 RID: 177
    private Vector2 _cellSize;

    // Token: 0x040000B2 RID: 178
    private Vector2 _cellMargin;

    // Token: 0x040000B3 RID: 179
    private Vector2 _headerSize;

    // Token: 0x040000B4 RID: 180
    private Vector2 _viewportSize;
}