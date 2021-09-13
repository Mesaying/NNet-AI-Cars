using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatricesState : State
{
    [System.Serializable]
    public struct NNetData
    {
        //public List<Matrix<float>> weights;
        public float biases;
        //public float Layers, Neurons;
    }

    public NNetData savedData = new NNetData();

    public override string SaveState()
    {
        NNet net = GetComponent<NNet>();
        Debug.Log(net.biases.Count);
        //savedData.weights = net.weights;
        //savedData.biases = net.biases[0];
        //savedData.Layers = net.GetComponent<CarController>().LAYERS;

        return JsonUtility.ToJson(savedData);
    }

    public override void LoadState(string loadedJSON)
    {
        savedData = JsonUtility.FromJson<NNetData>(loadedJSON);

        //GetComponent<NNet>().weights = savedData.weights;
        GetComponent<NNet>().biases[0] = savedData.biases;


    }

    public override bool shouldSave()
    {
        //if (GetComponent<NNet>().weights == savedData.weights &&
        //    transform.eulerAngles == savedState.rotation && transform.localScale == savedState.scale)
        //{
        //    return false;
        //}
        return true;
    }

    public override string GetUID()
    {
        return gameObject.name + FindObjectOfType<GeneticManager>().currentGeneration.ToString() + "/" + FindObjectOfType<GeneticManager>().currentGeneration.ToString() + gameObject.GetInstanceID();
    }

}
