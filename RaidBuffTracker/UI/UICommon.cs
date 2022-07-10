using System;
using RaidBuffTracker.Tracker.Library;

namespace RaidBuffTracker.UI;

public sealed class UICommon
{
    public static string GetActionCategoryName(ActionCategory category)
    {
        string result;
        switch (category)
        {
            case ActionCategory.OffensivePrimary:
                result = "Offensive Primary";
                break;
            case ActionCategory.MitigationPrimary:
                result = "Mitigation Primary";
                break;
            case ActionCategory.OffensiveSecondary:
                result = "Offensive Secondary";
                break;
            case ActionCategory.MitigationSecondary:
                result = "Mitigation Secondary";
                break;
            case ActionCategory.Utility:
                result = "Utility";
                break;
            default:
                throw new ArgumentException(string.Format("Unknown category: {0}!", category));
        }

        return result;
    }

    public static uint GetActionCategoryIcon(ActionCategory category)
    {
        uint result;
        switch (category)
        {
            case ActionCategory.OffensivePrimary:
                result = 15633U;
                break;
            case ActionCategory.MitigationPrimary:
                result = 15047U;
                break;
            case ActionCategory.OffensiveSecondary:
                result = 15050U;
                break;
            case ActionCategory.MitigationSecondary:
                result = 15052U;
                break;
            case ActionCategory.Utility:
                result = 15030U;
                break;
            default:
                throw new ArgumentOutOfRangeException("Unknown category!");
        }

        return result;
    }
}