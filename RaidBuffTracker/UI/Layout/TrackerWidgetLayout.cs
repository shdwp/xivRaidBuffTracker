using System;
using System.Collections.Generic;

namespace RaidBuffTracker.UI.Layout;

public static class TrackerWidgetLayout
{
    // Token: 0x06000098 RID: 152 RVA: 0x000048F9 File Offset: 0x00002AF9
    public static IEnumerable<string> Enumerate()
    {
        yield return Vertical;
        yield return Horizontal;
        yield return VerticalCategoryTable;
        yield return HorizontalCategoryTable;
    }

    // Token: 0x06000099 RID: 153 RVA: 0x00004904 File Offset: 0x00002B04
    public static string GetLayoutName(string identifier)
    {
        bool flag = identifier == Vertical;
        string result;
        if (flag)
        {
            result = "Vertical";
        }
        else
        {
            bool flag2 = identifier == Horizontal;
            if (flag2)
            {
                result = "Horizontal";
            }
            else
            {
                bool flag3 = identifier == VerticalCategoryTable;
                if (flag3)
                {
                    result = "Vertical Category Table";
                }
                else
                {
                    bool flag4 = identifier == HorizontalCategoryTable;
                    if (!flag4)
                    {
                        throw new ArgumentException("Unknown layout identifier " + identifier);
                    }

                    result = "Horizontal Category Table";
                }
            }
        }

        return result;
    }

    // Token: 0x040000AC RID: 172
    public static string VerticalCategoryTable = "verticalCategoryTable";

    // Token: 0x040000AD RID: 173
    public static string HorizontalCategoryTable = "horizontalCategoryTable";

    // Token: 0x040000AE RID: 174
    public static string Vertical = "vertical";

    // Token: 0x040000AF RID: 175
    public static string Horizontal = "horizontal";
}