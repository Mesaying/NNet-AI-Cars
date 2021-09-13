using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newNet", menuName = "SavedNet")]
public class SavedNet : ScriptableObject
{
    public string Net_name = "Empty";

    public bool isFilled = false;

    public List<Matrix<float>> weigths;

    public float avgSpeed;

    public List<float> biases;

    public List<float> floatWeights;

    public int Layers;

    public int Neurons;

    public NNet net;
}
