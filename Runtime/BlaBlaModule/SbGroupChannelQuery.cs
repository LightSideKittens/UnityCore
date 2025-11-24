/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class SbGroupChannelQuery
{
    private readonly string userId;
    private readonly Dictionary<string, object> query = new();
    private string nextToken;
    private bool hasNext = true;
    
    public bool HasNext => hasNext;

    public SbGroupChannelQuery(string userId = null)
    {
        this.userId = userId;
    }

    public Dictionary<string, object> Query => query;

    public void Set(string key, object value)
    {
        if (value == null)
        {
            query.Remove(key);
            return;
        }

        query[key] = value;
    }

    public bool Remove(string key) => query.Remove(key);

    public void Clear()
    {
        query.Clear();
        nextToken = null;
        hasNext = true;
    }

    public void LoadNextPage(Action<JObject> callback)
    {
        if (!hasNext)
        {
            return;
        }

        string path = userId != null
            ? "/v3/users/" + Uri.EscapeDataString(userId) + "/my_group_channels"
            : "/v3/group_channels";

        string queryString = BuildQueryString();
        string url = "https://api-" + BlaBlaSetting.AppId + ".sendbird.com" + path;

        if (!string.IsNullOrEmpty(queryString))
            url += "?" + queryString;

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Content-Type", "application/json; charset=utf8");
        request.SetRequestHeader("Api-Token", BlaBlaSetting.ApiToken);

        var op = request.SendWebRequest();
        op.completed += x =>
        {
            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception("Sendbird request failed: " + request.error);

            string text = request.downloadHandler.text;
            if (string.IsNullOrEmpty(text))
            {
                hasNext = false;
                nextToken = null;
                return;
            }

            JObject root = JObject.Parse(text);
            string next = root["next"]?.ToString();
            if (string.IsNullOrEmpty(next))
            {
                nextToken = null;
                hasNext = false;
            }
            else
            {
                nextToken = next;
                hasNext = true;
            }
            
            callback(root);
        };
    }

    private string BuildQueryString()
    {
        Dictionary<string, object> working = new Dictionary<string, object>(query);

        if (!string.IsNullOrEmpty(nextToken))
            working["token"] = nextToken;

        if (working.Count == 0)
            return string.Empty;

        List<string> parts = new List<string>();

        foreach (var pair in working)
        {
            string key = pair.Key;
            object value = pair.Value;
            if (value == null)
                continue;

            if (value is IList list)
            {
                List<string> items = new List<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    object item = list[i];
                    if (item == null) continue;
                    items.Add(Uri.EscapeDataString(item.ToString()));
                }

                string joined = string.Join(",", items);
                parts.Add(Uri.EscapeDataString(key) + "=" + joined);
            }
            else
            {
                string encodedValue = Uri.EscapeDataString(value.ToString());
                parts.Add(Uri.EscapeDataString(key) + "=" + encodedValue);
            }
        }

        return string.Join("&", parts);
    }
}
*/
