using System.Numerics;

namespace RaidBuffTracker.UI.Layout;

public interface ITrackerWidgetLayout
{
    // Token: 0x06000093 RID: 147
    Vector2 GetPos(Vector2 offset = default(Vector2));

    // Token: 0x06000094 RID: 148
    void AdvanceToNextCell();

    // Token: 0x06000095 RID: 149
    void AdvanceToNextSection();

    // Token: 0x06000096 RID: 150
    void StartOver(Vector2 cellSize, Vector2 cellMargin, Vector2 headerSize, Vector2 viewportSize);

    // Token: 0x06000097 RID: 151
    void AdvancePastHeader();
}