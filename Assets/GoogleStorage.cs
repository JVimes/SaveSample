#nullable enable

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System.Text;
using System.Threading.Tasks;
using static GooglePlayGames.BasicApi.SavedGame.SavedGameMetadataUpdate;

class GoogleStorage
{
    static readonly PlayGamesPlatform platform = PlayGamesPlatform.Instance;
    readonly ISavedGameClient client;


    GoogleStorage(ISavedGameClient client)
    {
        this.client = client;
    }

    public static async Task<GoogleStorage?> New(bool manualSignIn = false)
    {
        var signedIn = await TrySignIn(manualSignIn);
        if (!signedIn)
            return null;

        var client = platform.SavedGame;

        return client is null ?
               null :
               new GoogleStorage(client);
    }


    public async Task Save(string data)
    {
        var taskSource = new TaskCompletionSource<bool>();

        var metadata = await NewMetadata();
        var metadataUpdate = NewEmptyMetadataUpdate();
        var bytes = GetBytes(data);
        client.CommitUpdate(metadata, metadataUpdate, bytes, OnReturn);

        await taskSource.Task;

        void OnReturn(SavedGameRequestStatus status, ISavedGameMetadata _)
        {
            var success = status is SavedGameRequestStatus.Success;

            if (success)
                taskSource.SetResult(true);
            else
                taskSource.SetException(new StatusException(status));
        }
    }

    public async Task<string> Load()
    {
        var taskSource = new TaskCompletionSource<string>();

        var metadata = await NewMetadata();
        client.ReadBinaryData(metadata, OnReturn);

        return await taskSource.Task;

        void OnReturn(SavedGameRequestStatus status, byte[] bytes)
        {
            var success = status is SavedGameRequestStatus.Success;

            if (success)
            {
                var text = GetString(bytes);
                taskSource.SetResult(text);
            }
            else
                taskSource.SetException(new StatusException(status));
        }
    }


    Task<ISavedGameMetadata> NewMetadata()
    {
        var taskSource = new TaskCompletionSource<ISavedGameMetadata>();

        client.OpenWithAutomaticConflictResolution(
            filename: "state1",
            DataSource.ReadNetworkOnly,
            ConflictResolutionStrategy.UseMostRecentlySaved,
            OnReturn);

        return taskSource.Task;

        void OnReturn(SavedGameRequestStatus status, ISavedGameMetadata metadata)
        {
            var success = status is SavedGameRequestStatus.Success;

            if (success)
                taskSource.SetResult(metadata);
            else
                taskSource.SetException(new StatusException(status));
        }
    }

    static Task<bool> TrySignIn(bool manual)
    {
        var taskSource = new TaskCompletionSource<bool>();

        if (manual)
            platform.ManuallyAuthenticate(OnReturn);
        else
            platform.Authenticate(OnReturn);

        return taskSource.Task;

        void OnReturn(SignInStatus status)
        {
            var success = status is SignInStatus.Success;

            if (success)
                taskSource.SetResult(true);
            else
                taskSource.SetException(new StatusException(status));
        }
    }

    static string GetString(byte[] bytes) => Encoding.UTF8.GetString(bytes);
    static byte[] GetBytes(string text) => Encoding.UTF8.GetBytes(text);
    static SavedGameMetadataUpdate NewEmptyMetadataUpdate() => new Builder().Build();


    public class StatusException : System.Exception
    {
        public StatusException(SavedGameRequestStatus status)
            : base($"Status from google: {status}")
        { }

        public StatusException(SignInStatus status)
            : base($"Status from google: {status}")
        { }
    }
}
#endif