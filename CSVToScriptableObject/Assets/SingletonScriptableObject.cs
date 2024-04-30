using UnityEngine;

public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T[] asset = Resources.LoadAll<T>("");

                if (asset == null || asset.Length < 1)
                {
                    throw new System.Exception($"{nameof(SingletonScriptableObject<T>)} : Not Exist");
                }
                else if (asset.Length > 1)
                {
                    Debug.LogWarning($"{nameof(SingletonScriptableObject<T>)} : More then One");
                }

                instance = asset[0];
            }

            return instance;
        }
    }
}
