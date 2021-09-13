using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;

using Random = UnityEngine.Random;
public class NNet : MonoBehaviour
{
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public List<float> floatWeights = new List<float>();

    public float fitness;


    public void Initialize(int hiddenLayerCount, int hiddenNeuronCount)
    {

        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        biases.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);

            hiddenLayers.Add(f);

            
            biases.Add(Random.Range(-1f, 1f));

            //weights

            if(i == 0)
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);
                weights.Add(inputToH1);
            }

            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);
        }

        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);
        biases.Add(Random.Range(-1f, 1f));
        RandomWeights();
        //Debug.Log(biases.Count);
    }

    public NNet InitializeClone(int hiddenLayerCount, int hiddenNeuronCount)
    {
        NNet n = new NNet();

        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = 
                Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for (int x = 0; x < currentWeight.RowCount; x++)
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }
            newWeights.Add(currentWeight);
        }
        

        List<float> newBiases = new List<float>();
        newBiases.AddRange(biases);

        n.weights = newWeights;
        n.biases = newBiases;

        n.InitializeHidden(hiddenLayerCount, hiddenNeuronCount);
        return n;
    }

    public void InitializeFromPreset(List<float> _floatWeights, List<float> _biases, int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        biases.Clear();
        weights.Clear();
        floatWeights.Clear();

        floatWeights.AddRange(_floatWeights);

        Debug.Log("_weight has count of: " + _floatWeights.Count);

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);

            hiddenLayers.Add(f);
            

            biases.Add(_biases[i]);

            //weights

            if (i == 0)
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);
                weights.Add(inputToH1);
            }
            

            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);
        }

        
        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);
        biases.Add(_biases[_biases.Count -1]);

        Debug.Log(weights.Count);

        

        for (int i = 0; i < weights.Count; i++)
        {
            int floatsToSkip = 0;

            for (int h = 0; h < i; h++)
            {
                floatsToSkip += weights[h].RowCount * weights[h].ColumnCount;
            }

            for (int row = 0; row < weights[i].RowCount; row++)
            {
                for (int col = 0; col < weights[i].ColumnCount; col++)
                {
                        weights[i][row, col] = _floatWeights[row * weights[i].ColumnCount + col + floatsToSkip];
                //f += weights[i].RowCount * weights[i].ColumnCount;
                    //Debug.Log(weights[i][row, col] + i);
                }
            }
        }
        InitializeHidden(hiddenLayerCount, hiddenNeuronCount);

        for (int i = 0; i < weights.Count; i++)
        {
            for (int row = 0; row < weights[i].RowCount; row++)
            {
                for (int col = 0; col < weights[i].ColumnCount; col++)
                {
                    Debug.Log(weights[i][row, col] + "index: " + i);
                }
            }
        }
    }

    void InitializeHidden(int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }

    void RandomWeights()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);
                }
            }
        }

        floatWeights = new List<float>();
        floatWeights.Clear();
        for (int i = 0; i < weights.Count; i++)
        {
            for (int row = 0; row < weights[i].RowCount; row++)
            {
                for (int col = 0; col < weights[i].ColumnCount; col++)
                {
                    floatWeights.Add(weights[i][row, col]);
                }
            }
        }
    }

    public (float, float) RunNetwork(float a, float b , float c)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;

        inputLayer = inputLayer.PointwiseTanh();

        hiddenLayers[0] = ((inputLayer * weights[0] + biases[0])).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i-1] * weights[i] + biases[i])).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1] + biases[biases.Count - 1])).PointwiseTanh();

        //First output == accel second output == steer
        return (Sigmoid(outputLayer[0,0]) , (float)Math.Tanh(outputLayer[0,1]) );
    }

    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }
}
