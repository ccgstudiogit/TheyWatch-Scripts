using UnityEngine;

/// <summary>
///     Any class that wants to save to a Json file should inherit from this class, as it Serializable.
/// </summary>
[System.Serializable]
public abstract class SerializableClass
{
    private FileDataHandler dataHandler;
    protected string fileName; // This is protected so inherited scripts can reference the file name in Debug.Log statements
    private bool useEncryption;

    public SerializableClass(string fileName, bool useEncryption)
    {
        this.fileName = fileName;
        this.useEncryption = useEncryption;
        CreateFileDataHandler();
    }

    /// <summary>
    ///     Load the serialized class data from a Json file.
    /// </summary>
    /// <returns>The SerializableClass of type T if the Json file exists, null if not.</returns>
    protected T Load<T>() where T : SerializableClass
    {
        if (dataHandler == null)
        {
            CreateFileDataHandler();
        }

        return dataHandler.Load<T>();
    }

    /// <summary>
    ///     Save this serializable class object to a Json file.
    /// </summary>
    protected void Save()
    {
        if (dataHandler == null)
        {
            CreateFileDataHandler();
        }

        dataHandler.Save(this);
    }

    /// <summary>
    ///     Create the FileDataHandler object using Application.persistentDataPath and fileName.
    /// </summary>
    private void CreateFileDataHandler()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }
}
