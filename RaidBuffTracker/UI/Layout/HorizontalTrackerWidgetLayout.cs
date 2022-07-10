using System.Numerics;

namespace RaidBuffTracker.UI.Layout;

public sealed class HorizontalTrackerWidgetLayout : ITrackerWidgetLayout
{
    // Token: 0x0600008D RID: 141 RVA: 0x00004660 File Offset: 0x00002860
    public void StartOver(Vector2 cellSize, Vector2 cellMargin, Vector2 headerSize, Vector2 viewportSize)
    {
        _cellSize = cellSize;
        _cellMargin = cellMargin;
        _headerSize = headerSize;
        _viewportSize = viewportSize;
        bool rightToLeft = _rightToLeft;
        if (rightToLeft)
        {
            _pos = new Vector2(viewportSize.X - cellMargin.X * 2f - cellSize.X, cellMargin.Y + 25f);
        }
        else
        {
            _pos = new Vector2(cellMargin.X, cellMargin.Y + 25f);
        }
    }

    // Token: 0x0600008E RID: 142 RVA: 0x000046EC File Offset: 0x000028EC
    public Vector2 GetPos(Vector2 offset = default(Vector2))
    {
        return _pos + offset;
    }

    // Token: 0x0600008F RID: 143 RVA: 0x0000470C File Offset: 0x0000290C
    public void AdvanceToNextCell()
    {
        float nextCellRequiredWidth = _cellSize.X * 2f + _cellMargin.X * 2f;
        bool rightToLeft = _rightToLeft;
        if (rightToLeft)
        {
            bool flag = _pos.X - nextCellRequiredWidth < 0f;
            if (flag)
            {
                _pos.X = _viewportSize.X - _cellMargin.X * 2f - _cellSize.X;
                _pos.Y = _pos.Y + (_cellSize.Y + _cellMargin.Y);
            }
            else
            {
                _pos.X = _pos.X - (_cellSize.X + _cellMargin.X);
            }
        }
        else
        {
            bool flag2 = _pos.X + nextCellRequiredWidth > _viewportSize.X;
            if (flag2)
            {
                _pos.X = _cellMargin.X;
                _pos.Y = _pos.Y + (_cellSize.Y + _cellMargin.Y);
            }
            else
            {
                _pos.X = _pos.X + (_cellSize.X + _cellMargin.X);
            }
        }
    }

    // Token: 0x06000090 RID: 144 RVA: 0x00004878 File Offset: 0x00002A78
    public void AdvancePastHeader()
    {
        float headerWidth = _cellSize.X / 2f + _headerSize.X / 2f + _cellMargin.X;
        bool rightToLeft = _rightToLeft;
        if (rightToLeft)
        {
            _pos.X = _pos.X - headerWidth;
        }
        else
        {
            _pos.X = _pos.X + headerWidth;
        }
    }

    // Token: 0x06000091 RID: 145 RVA: 0x000048E6 File Offset: 0x00002AE6
    public void AdvanceToNextSection()
    {
    }

    // Token: 0x040000A6 RID: 166
    private Vector2 _pos;

    // Token: 0x040000A7 RID: 167
    private Vector2 _cellSize;

    // Token: 0x040000A8 RID: 168
    private Vector2 _cellMargin;

    // Token: 0x040000A9 RID: 169
    private Vector2 _headerSize;

    // Token: 0x040000AA RID: 170
    private Vector2 _viewportSize;

    // Token: 0x040000AB RID: 171
    private bool _rightToLeft = false;
}