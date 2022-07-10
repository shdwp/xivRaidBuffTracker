using System.Numerics;

namespace RaidBuffTracker.UI.Layout;

public sealed class VerticalTrackerWidgetLayout : ITrackerWidgetLayout
{
    // Token: 0x060000A1 RID: 161 RVA: 0x00004B54 File Offset: 0x00002D54
    public void StartOver(Vector2 cellSize, Vector2 cellMargin, Vector2 headerSize, Vector2 viewportSize)
    {
        _cellSize = cellSize;
        _cellMargin = cellMargin;
        _headerSize = headerSize;
        _viewportSize = viewportSize;
        bool reverseAlignment = _reverseAlignment;
        if (reverseAlignment)
        {
            _pos = new Vector2(cellMargin.X, viewportSize.Y - cellSize.Y - cellMargin.Y);
        }
        else
        {
            _pos = new Vector2(cellMargin.X, cellMargin.Y + 25f);
        }
    }

    // Token: 0x060000A2 RID: 162 RVA: 0x00004BD4 File Offset: 0x00002DD4
    public Vector2 GetPos(Vector2 offset = default(Vector2))
    {
        return _pos + offset;
    }

    // Token: 0x060000A3 RID: 163 RVA: 0x00004BF4 File Offset: 0x00002DF4
    public void AdvanceToNextCell()
    {
        float nextCellRequiredWidth = _cellSize.Y * 2f + _cellMargin.Y * 2f;
        bool reverseAlignment = _reverseAlignment;
        if (reverseAlignment)
        {
            bool flag = _pos.Y - nextCellRequiredWidth < 25f;
            if (flag)
            {
                _pos.Y = _viewportSize.Y - _cellSize.Y - _cellMargin.Y;
                _pos.X = _pos.X + (_cellSize.X + _cellMargin.X);
            }
            else
            {
                _pos.Y = _pos.Y - (_cellSize.Y + _cellMargin.Y);
            }
        }
        else
        {
            bool flag2 = _pos.Y + nextCellRequiredWidth > _viewportSize.Y;
            if (flag2)
            {
                _pos.Y = _cellMargin.Y + 25f;
                _pos.X = _pos.X + (_cellSize.X + _cellMargin.X);
            }
            else
            {
                _pos.Y = _pos.Y + (_cellSize.Y + _cellMargin.Y);
            }
        }
    }

    // Token: 0x060000A4 RID: 164 RVA: 0x00004D60 File Offset: 0x00002F60
    public void AdvancePastHeader()
    {
        bool reverseAlignment = _reverseAlignment;
        if (reverseAlignment)
        {
            _pos.Y = _pos.Y - (_cellSize.Y / 2f + _headerSize.Y / 2f + _cellMargin.X);
        }
        else
        {
            _pos.Y = _pos.Y + (_cellSize.Y / 2f + _headerSize.Y / 2f + _cellMargin.X);
        }
    }

    // Token: 0x060000A5 RID: 165 RVA: 0x00004DFA File Offset: 0x00002FFA
    public void AdvanceToNextSection()
    {
    }

    // Token: 0x040000B5 RID: 181
    private Vector2 _pos;

    // Token: 0x040000B6 RID: 182
    private Vector2 _cellSize;

    // Token: 0x040000B7 RID: 183
    private Vector2 _cellMargin;

    // Token: 0x040000B8 RID: 184
    private Vector2 _headerSize;

    // Token: 0x040000B9 RID: 185
    private Vector2 _viewportSize;

    // Token: 0x040000BA RID: 186
    private bool _reverseAlignment = false;
}