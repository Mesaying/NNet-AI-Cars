using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [HideInInspector]
    public enum LoadSettings
    {
        AllRandom, AllSaved, Random50Loaded50, oneThirdAllSaved
    }

    [Header("Loading options")]
    public LoadSettings LoadSetting;

    [Range(0, 2)]
    public int useSaveIndex;


    [Header("Reference")]
    public CarController controller;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;
    [Range(0.0f, 1.0f)]
    public float mutationAmount = 0.14f;

    [Header("Breeding Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToBreed;

    private List<int> genePool = new List<int>();

    private int naturallySelected;
    private NNet[] population;

    [Header("Debug")]
    public int currentGeneration;
    public int currentGenome = 0;

    private void Start()
    {
        CreatePopulation();
    }

    private void CreatePopulation()
    {

        population = new NNet[initialPopulation];

        
        if(LoadSetting == LoadSettings.AllRandom)
        {
            FillPopulationRandom(population, 0);
        }
        else if (LoadSetting == LoadSettings.AllSaved)
        {
            FillPopulationPreset(population, 0, useSaveIndex);
        }
        else if (LoadSetting == LoadSettings.Random50Loaded50)
        {
            FillPopulationRandom(population, 0);
            FillPopulationPreset(population, population.Length / 2, useSaveIndex);                    
        }
        else if (LoadSetting == LoadSettings.oneThirdAllSaved)
        {
            FillPopulationPreset(population, 0, 0);
            FillPopulationPreset(population, population.Length / 3, 1);
            FillPopulationPreset(population, Mathf.FloorToInt((float)population.Length / 1.5f), 2);
        }
        
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);
    }

    private void FillPopulationRandom(NNet[] newPop, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPop[startingIndex] = new NNet();
            newPop[startingIndex].Initialize(controller.LAYERS, controller.NEURONS);

            startingIndex++;
        }
    }

    private void FillPopulationPreset(NNet[] newPop, int startingIndex, int saveIndex)
    {
        //if(controller.SavedNet0.net == null) { return; }
        while (startingIndex < initialPopulation)
        {
            newPop[startingIndex] = new NNet();
            //Debug.Log("before: " + controller.SavedNet0.weigths.Count);
            newPop[startingIndex].InitializeFromPreset(controller.SavedNets[saveIndex].floatWeights, controller.SavedNets[saveIndex].biases, controller.LAYERS, controller.NEURONS);
            startingIndex++;
        }
    }

    public void Death(float fitness, NNet network)
    {
        if(currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            Repopulate();
        }
    }

    private void Repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();

        NNet[] newPop = PickBestPop();

        Breed(newPop);
        Mutate(newPop);

        FillPopulationRandom(newPop, naturallySelected);

        population = newPop;
        currentGenome = 0;

        ResetToCurrentGenome();
    }

    public void Mutate(NNet[] newPop)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int c = 0; c < newPop[i].weights.Count; c++)
            {
                if(Random.Range(0.0f,1.0f) < mutationRate)
                {
                    newPop[i].weights[c] = MutateMatrix(newPop[i].weights[c]);
                }
            }
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> a)
    {
        int randomPoints = (int)Random.Range(1, (a.RowCount * a.ColumnCount) * mutationAmount);

        Matrix<float> C = a;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp
                (C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;
    }
    public void Breed(NNet[] newPop)
    {
        for (int i = 0; i < numberToBreed ; i+=2)
        {
            int fatherIndex = i;
            int motherIndex = i + 1;

            if(genePool.Count > 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    fatherIndex = genePool[Random.Range(0, genePool.Count)];
                    motherIndex = genePool[Random.Range(0, genePool.Count)];

                    if(fatherIndex != motherIndex)
                    {
                        break;
                    }
                }
            }

            NNet child1 = new NNet();
            NNet child2 = new NNet();

            child1.Initialize(controller.LAYERS, controller.NEURONS);
            child2.Initialize(controller.LAYERS, controller.NEURONS);

            //should be redundant
            child1.fitness = 0;
            child2.fitness = 0;

            for (int f = 0; f < child1.weights.Count; f++)
            {
                if(Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.weights[f] = population[fatherIndex].weights[f];
                    child2.weights[f] = population[motherIndex].weights[f];
                }
                else
                {
                    child1.weights[f] = population[motherIndex].weights[f];
                    child2.weights[f] = population[fatherIndex].weights[f];
                }
            }

            for (int f = 0; f < child1.biases.Count; f++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.biases[f] = population[fatherIndex].biases[f];
                    child2.biases[f] = population[motherIndex].biases[f];
                }
                else
                {
                    child1.biases[f] = population[motherIndex].biases[f];
                    child2.biases[f] = population[fatherIndex].biases[f];
                }
            }

            newPop[naturallySelected] = child1;
            naturallySelected++;
            newPop[naturallySelected] = child2;
            naturallySelected++;
        }
    }

    private NNet[] PickBestPop()
    {
        NNet[] newPop = new NNet[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPop[naturallySelected] = population[i].InitializeClone(controller.LAYERS, controller.NEURONS);
            newPop[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }
        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }
        }

        return newPop;
    }

    //Sort by fitness;
    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if(population[i].fitness < population[j].fitness)
                {
                    NNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }
}
