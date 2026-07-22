using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    [SerializeField] TMP_Text message;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button LoadButton;
    [SerializeField] Button SaveButton;

    GoogleStorage storage;


    async void Awake()
    {
        storage = await GoogleStorage.New();

        LoadButton.onClick.AddListener(OnLoadClick);
        SaveButton.onClick.AddListener(OnSaveClick);
        Save += SaveEventHandler;
    }

    void OnDestroy()
    {
        LoadButton.onClick.RemoveAllListeners();
        SaveButton.onClick.RemoveAllListeners();
        Save -= SaveEventHandler;
    }


    event Action<string> Save;


    void OnSaveClick()
    {
        try
        {
            message.text = "Saving...";

            var text = inputField.text;
            Save?.Invoke(text);
            Save?.Invoke(text);

            message.text = $"Saved ({DateTimeOffset.Now:T})";
        }
        catch (Exception ex)
        {
            message.text = ex.Message;
            return;
        }
    }

    async void OnLoadClick()
    {
        try
        {
            message.text = "Loading...";

            var text = await storage.Load();
            inputField.SetTextWithoutNotify(text);

            message.text = string.IsNullOrEmpty(text) ?
                           "Nothing to load" :
                           $"Loaded ({DateTimeOffset.Now:T})";
        }
        catch (Exception ex)
        {
            message.text = ex.Message;
            return;
        }
    }

    async void SaveEventHandler(string text)
    {
        await storage.Save(text);
    }
}
