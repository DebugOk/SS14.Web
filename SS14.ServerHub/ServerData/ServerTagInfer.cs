﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SS14.ServerHub.ServerData;

/// <summary>
/// Helper code for inferring server tags from other metadata, such as their name.
/// Intended as a stopgap measure before servers properly tag their stuff in the API.
/// </summary>
public static partial class ServerTagInfer
{
    private static readonly Regex TagLikeRegex = MyRegex();

    public static string[] InferTags(string currentName, string[] currentTags)
    {
        var addedTags = new List<string>();

        // no_tag_infer stops all inference logic.
        if (currentTags.Contains(Tags.TagNoTagInfer))
            return Array.Empty<string>();

        // Extract all name [tags] via regex.
        var tagLikes = TagLikeRegex
            .Matches(currentName)
            .Select(x => x.Groups[1].Captures[0].Value)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        // Infer language tags for [EN] and [RU] if there's no language tags.
        // Not adding any other languages, advertise it properly with the API.
        if (!currentTags.Any(t => t.StartsWith(Tags.TagLanguage, StringComparison.OrdinalIgnoreCase)))
        {
            if (tagLikes.Contains("en"))
            {
                addedTags.Add(Tags.TagLanguage + "en");
            }
            else if (tagLikes.Contains("ru") || tagLikes.Contains("rus"))
            {
                addedTags.Add(Tags.TagLanguage + "ru");
            }
        }

        // Infer 18+
        if (!currentTags.Contains(Tags.TagEighteenPlus))
        {
            if (tagLikes.Contains("18+") || tagLikes.Contains("+18") || tagLikes.Contains("18") || tagLikes.Contains("ERP"))
                addedTags.Add(Tags.TagEighteenPlus);
        }

        // Infer NRP/LRP/MRP/HRP if no RP tags.
        if (!currentTags.Any(t => t.StartsWith(Tags.TagRolePlay, StringComparison.OrdinalIgnoreCase)))
        {
            if (tagLikes.Contains("nrp"))
                addedTags.Add(Tags.TagRolePlay + Tags.RolePlayNone);
            else if (tagLikes.Contains("lrp"))
                addedTags.Add(Tags.TagRolePlay + Tags.RolePlayLow);
            else if (tagLikes.Contains("mrp"))
                addedTags.Add(Tags.TagRolePlay + Tags.RolePlayMedium);
            else if (tagLikes.Contains("hrp"))
                addedTags.Add(Tags.TagRolePlay + Tags.RolePlayHigh);
        }

        return addedTags.ToArray();
    }

    [GeneratedRegex("\\[(.*?)\\]")]
    private static partial Regex MyRegex();
}
