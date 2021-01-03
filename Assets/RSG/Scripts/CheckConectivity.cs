using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class CheckConectivity : MonoBehaviour
{
    public static IEnumerator IsOnline(Action<bool> action)
    {
        string html = string.Empty;

        try
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://us-central1-project-starman.cloudfunctions.net/checkInternetConnectivity");
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                if (isSuccess)
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        char[] cs = new char[80];
                        reader.Read(cs, 0, cs.Length);
                        foreach (char ch in cs)
                        {
                            html += ch;
                        }
                    }
                }
            }
        }
        catch
        {
            action(false);
            yield break;
        }

        action(html.Contains("gravity-flight-success"));
    }
}