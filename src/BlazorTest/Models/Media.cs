﻿using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record Media(
	[property: JsonPropertyName("id")]
	int Id,
	[property: JsonPropertyName("title")]
	Title Title,
	[property: JsonPropertyName("status")]
	MediaStatus? Status,
	[property: JsonPropertyName("format")]
	MediaFormat? Format,
	[property: JsonPropertyName("episodes")]
	int? Episodes,
	[property: JsonPropertyName("nextAiringEpisode")]
	NextAiringEpisode? NextAiringEpisode,
	[property: JsonPropertyName("duration")]
	int? Duration,
	[property: JsonPropertyName("averageScore")]
	int? AverageScore,
	[property: JsonPropertyName("popularity")]
	int Popularity,
	[property: JsonPropertyName("startDate")]
	StartDate? StartDate,
	[property: JsonPropertyName("genres")]
	IReadOnlyList<string> Genres,
	[property: JsonPropertyName("tags")]
	IReadOnlyList<MediaTag> Tags
)
{
	[JsonIgnore]
	public bool IsGenresExpanded { get; set; }
	[JsonIgnore]
	public bool IsTagsExpanded { get; set; }
	[JsonIgnore]
	public bool IsEntryVisible { get; set; } = true;
}

public static class MediaUtils
{
	public const string NO_VALUE = "N/A";

	public static string DisplayDuration(this Media media)
	{
		var duration = media.GetTotalDuration();
		if (!duration.HasValue)
		{
			return NO_VALUE;
		}
		return $"{duration} minute{(duration == 1 ? "" : "s")}";
	}

	public static string DisplayEpisodeCount(this Media media)
	{
		var count = media.GetHighestEpisode();
		if (!count.HasValue)
		{
			return NO_VALUE;
		}
		return $"{count} episode{(count == 1 ? "" : "s")}";
	}

	public static string DisplayExpanderButton<T>(
		this IReadOnlyList<T> list,
		bool expanded)
	{
		if (expanded)
		{
			return "🗙";
		}
		return $"+{list.Count - 1}";
	}

	public static string DisplayFormat(this Media media)
		=> media.Format?.ToString() ?? NO_VALUE;

	public static string DisplayGenres(this Media media, bool expanded)
	{
		if (media.Genres.Count == 0)
		{
			return NO_VALUE;
		}

		if (expanded)
		{
			return string.Join(Environment.NewLine, media.Genres);
		}
		return media.Genres[0];
	}

	public static string DisplayScore(this Media media)
	{
		var score = media.AverageScore;
		if (!score.HasValue)
		{
			return NO_VALUE;
		}
		return $"{score}%";
	}

	public static string DisplayTag(this MediaTag tag)
	{
		var name = tag.Name;
		if (name is "Cute Girls Doing Cute Things")
		{
			name = "CGDCT";
		}
		return $"{name} ({tag.Rank}%)";
	}

	public static string DisplayTags(this Media media, bool expanded)
	{
		if (media.Tags.Count == 0)
		{
			return NO_VALUE;
		}

		if (expanded)
		{
			var selected = media.Tags.Select(x => x.DisplayTag());
			return string.Join(Environment.NewLine, selected);
		}
		return media.Tags[0].DisplayTag();
	}

	public static string DisplayYear(this Media media)
	{
		var year = media.StartDate?.Year;
		if (!year.HasValue)
		{
			return NO_VALUE;
		}
		return year.Value.ToString();
	}

	public static int? GetHighestEpisode(this Media media)
		=> media.Episodes ?? media.NextAiringEpisode?.Episode;

	public static int? GetTotalDuration(this Media media)
		=> media.GetHighestEpisode() * media.Duration;

	public static string GetUrl(this Media media)
		=> $"https://anilist.co/anime/{media.Id}/";

	public static void ToggleGenresExpanded(this Media media)
		=> media.IsGenresExpanded = !media.IsGenresExpanded;

	public static void ToggleTagsExpanded(this Media media)
		=> media.IsTagsExpanded = !media.IsTagsExpanded;
}