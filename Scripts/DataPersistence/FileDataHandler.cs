using System;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";

    private bool useEncryption = false;
    private readonly string encryptionKeyword = "lucy";

    /// <summary>
    ///     Create a new FileDataHandler object. The FileDataHandler can load (Load()) and save (Save()) serializable data to/from Json files.
    /// </summary>
    /// <param name="dataDirPath">The directory path to the Json file.</param>
    /// <param name="dataFileName">The name of the Json file.</param>
    /// <param name="useEncryption">Whether this file should be encrypted or not.</param>
    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    /// <summary>
    ///     Load existing data from a Json file.
    /// </summary>
    /// <typeparam name="T">The serializable class that the file should load the data to.</typeparam>
    /// <returns>The loaded data (as the serialized class) if the file exists, null if the file does not exist.</returns>
    public T Load<T>() where T : SerializableClass
    {
        // Use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        T loadedData = null;

#if UNITY_EDITOR
        Debug.Log("Loading data from " + fullPath);
#endif

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                // Load the serialized data from the file
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // Deserialize the data from JSON back into the C# object
                loadedData = JsonUtility.FromJson<T>(dataToLoad);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError($"Error occured when trying to load data from file: {fullPath + "\n" + e}");
#endif
            }
        }

        return loadedData;
    }

    /// <summary>
    ///     Save data from a serialized class to a Json file.
    /// </summary>
    /// <param name="serializedData">The serialized class whose data should be saved to the Json file.</param>
    public void Save<T>(T serializedData) where T : SerializableClass
    {
        // Use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);

#if UNITY_EDITOR
        Debug.Log("Saving data to " + fullPath);
#endif

        try
        {
            // Create the directory the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize the C# game data object into JSON
            string dataToStore = JsonUtility.ToJson(serializedData, true);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // Write the serialized data to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"Error occured when trying to save data to file: {fullPath + "\n" + e}");
#endif
        }
    }

    /// <summary>
    ///     Encrypt / decrypt data using a simple implementation of XOR encryption.
    /// </summary>
    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";

        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionKeyword[i % encryptionKeyword.Length]);
        }

        return modifiedData;
    }
}
