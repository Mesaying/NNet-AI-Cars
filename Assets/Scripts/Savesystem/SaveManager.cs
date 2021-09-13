using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
public class SaveManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("Save");
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine("Load");
        }
    }

    private IEnumerator Save()
    {
        foreach (State s in GameObject.FindObjectsOfType<State>())
        {
            yield return new WaitForEndOfFrame();
            string json = s.SaveState();

            WriteFileAsync(Application.persistentDataPath + "/" + s.GetUID()+ ".save", json);
            //Debug.Log(Application.persistentDataPath + "/" + s.GetUID() + ".save");

            yield return new WaitForEndOfFrame();
        }
    }

    public async Task WriteFileAsync(string path, string json)
    {
        using(StreamWriter outputFile = new StreamWriter(path))
        {
            await outputFile.WriteAsync(json);
        }
    }

    private void Load()
    {
        foreach (State s in FindObjectsOfType<State>())
        {
            if (s.ShouldLoad())
            {
                string expectedLocation = Application.persistentDataPath + "/" + s.GetUID() + ".save";

                if (File.Exists(expectedLocation))
                {
                    string json = File.ReadAllText(expectedLocation);
                    s.LoadState(json);
                    Debug.Log(expectedLocation);
                }
            }
        }
    }
}
