using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using UnityEngine.Networking;

public class SignalRClient : MonoBehaviour
{
    HubConnection connection;
    [SerializeField]
    string gameSessionId;
    //[SerializeField]
    //string cookie;

    [SerializeField]
    string username;

    [SerializeField]
    string eventName;
    [SerializeField, TextArea]
    string payload;

    IEnumerator Start()
    {
        var www = UnityWebRequest.Get("https://localhost:5003/api/editor/login?username=" + username);
        yield return www.SendWebRequest();
        var cookie = www.GetResponseHeader("set-cookie");
        Debug.Log(cookie);

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5003/game_session", options =>
            {
                //options.Cookies = new System.Net.CookieContainer(cookie.Length);
                options.Cookies.SetCookies(new System.Uri("https://localhost:5003/"), cookie);

                foreach (var _cookie in options.Cookies.GetCookies(new System.Uri("https://localhost:5003/")))
                {
                    Debug.Log(_cookie);
                }
            })
            .Build();

        Debug.Log("Connection Built");

        //connection

        //Microsoft.AspNetCore.SignalR.Client.

        connection.Closed += async (error) =>
        {
            await Task.Delay(new System.Random().Next(0, 5) * 1000);
            await connection.StartAsync();
        };

        connection.On(gameSessionId, (GameMessage payload) =>
        {
            Debug.Log($"received function: {payload.FunctionName}, message: {payload.Payload}");
        });

        Debug.Log("Connection listening");

        StartConnection();

        async void StartConnection()
        {
            Debug.Log("StartConnection");
            await connection.StartAsync();
            Debug.Log("Connection started");

            await connection.InvokeAsync("JoinGameSession", gameSessionId);
            Debug.Log("Joined Game Session call complete.");
        }
    }

    public void OnClick()
    {
        Invoke();

        async void Invoke()
        {
            await connection.InvokeAsync("PlayerMessage", gameSessionId, eventName, payload);
            Debug.Log("Invoked server function.");
        }
    }
}

[System.Serializable]
public class GameMessage
{
    [SerializeField]
    public string FunctionName { get; private set; }
    [SerializeField]
    public string Payload { get; private set; }
}