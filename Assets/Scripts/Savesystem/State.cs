using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class State : MonoBehaviour
{
    public virtual string SaveState()
    {
        return null;
    }

    public virtual void LoadState(string loadedJSON)
    {

    }

    public virtual bool shouldSave()
    {
        return true;
    }

    public virtual string GetUID()
    {
        return (gameObject.scene.name + "_" + gameObject.name + "_" + (this.GetType()));
    }

    public virtual bool ShouldLoad()
    {
        return true;
    }
}
