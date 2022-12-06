﻿using Json.More;
using Json.Schema;

using Microsoft.JSInterop;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlazorTest.Pages;

public partial class JsonEditor
{
	private readonly DotNetObjectReference<JsonEditor> _DotNetReference;
	private readonly JsonSerializerOptions _Options = new()
	{
		WriteIndented = true,
	};
	private JsonNode? _Default;
	public string? Errors { get; set; }
	public string? Json { get; set; }

	public JsonEditor()
	{
		_DotNetReference = DotNetObjectReference.Create(this);
	}

	public static bool RemoveDefaults(JsonNode? @default, JsonNode? node)
	{
		// node == defaults even if both ar enull
		if (@default is null)
		{
			return node is null;
		}
		// default has a value, node is null, user cannot provide a null value
		// means to remove this value
		else if (node is null)
		{
			return true;
		}
		else if (@default.GetType() != node.GetType())
		{
			throw new InvalidOperationException(
				$"Defaults and current JSON node are not the same type at '{node.GetPath()}'.");
		}

		var isDefaultValue = true;
		// loop through all array values
		if (@default is JsonArray jDefaultArray && node is JsonArray jArray)
		{
			isDefaultValue = jArray.Count == jDefaultArray.Count;
			for (var i = jDefaultArray.Count - 1; i >= 0; --i)
			{
				if (RemoveDefaults(jDefaultArray[i], jArray[i]))
				{
					jArray.RemoveAt(i);
				}
				else
				{
					isDefaultValue = false;
				}
			}
		}
		// loop through all properties
		else if (@default is JsonObject jDefaultObj && node is JsonObject jObj)
		{
			isDefaultValue = jObj.All(x => jDefaultObj.ContainsKey(x.Key));
			foreach (var (propertyName, jDefaultProperty) in jDefaultObj)
			{
				if (RemoveDefaults(jDefaultProperty, jObj[propertyName]))
				{
					jObj.Remove(propertyName);
				}
				else
				{
					isDefaultValue = false;
				}
			}
		}
		// check if json strings are the same
		else if (@default is JsonValue jDefaultValue && node is JsonValue jValue)
		{
			var jDefault = jDefaultValue.AsJsonString();
			var jString = jValue.AsJsonString();
			Console.WriteLine($"{jDefault}/{jString}");
			isDefaultValue = jDefault == jString;
		}
		else
		{
			throw new ArgumentException("Not a valid JSON type.", nameof(node));
		}

		return isDefaultValue;
	}

	[JSInvokable]
	public Task OnJsonEditorChanged(JsonElement obj, JsonErrors[] errors)
	{
		Errors = JsonSerializer.Serialize(errors, _Options);

		if (errors.Length == 0)
		{
			var node = obj.AsNode()!;
			RemoveDefaults(_Default, node);
			Json = JsonSerializer.Serialize(node, _Options);
		}
		else
		{
			Json = "Invalid";
		}

		StateHasChanged();
		return Task.CompletedTask;
	}

	[JSInvokable]
	public Task OnJsonEditorInstantiated(JsonElement obj)
	{
		_Default = obj.AsNode();
		Json = "{}";
		return Task.CompletedTask;
	}

	protected override async Task OnInitializedAsync()
	{
		var schema = (await Http.GetFromJsonAsync<JsonSchema>(
			requestUri: "sample-data/sampleschema4.json"
		).ConfigureAwait(false))!;

		var module = await JS.InvokeAsync<IJSObjectReference>(
			identifier: "import",
			args: "./Pages/JsonEditor.razor.ts"
		).ConfigureAwait(false);

		await module.InvokeVoidAsync(
			identifier: JSMethods.InitJsonEditor,
			args: new object[]
			{
				_DotNetReference,
				IDs.JsonEditor.Query(),
				schema,
				new
				{
					location = new
					{
						city = "Petaluma"
					},
				},
			}
		).ConfigureAwait(false);
	}

	public record JsonErrors(
		string Path,
		string Property,
		string Message,
		int ErrorCount
	);
}