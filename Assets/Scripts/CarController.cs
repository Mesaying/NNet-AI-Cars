using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{
    public enum SaveSettings { AlwaysOverride, NeverOverride, OverrideSlowest}

    private Vector3 startPos, startRot;
    public Vector3 SensorOffset = Vector3.up;

    private NNet network;

    [Range(-1, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMult = 1.4f;
    public float avgSpeedMult = 0.2f;
    public float sensorMultiplier = 0.1f;
    public float FitnessThreshold20 = 40f;
    public float FitnessSavethreshold = 1000f;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;


    private Vector3 LastPos;
    private float totalDistanceTravelled;
    private float avgSpeed;
    
    private float aSensor, bSensor, cSensor;

    [Header("Saving")]
    public SaveSettings saveSetting;

    public bool shouldSave;
    public List<SavedNet> SavedNets;
    private int saveCounter;
    private GeneticManager genMan;
    private void Awake()
    {
        genMan = FindObjectOfType<GeneticManager>();
        startPos = transform.position;
        startRot = transform.eulerAngles;
        network = GetComponent<NNet>();
    }

    public void ResetWithNetwork(NNet net)
    {
        network = net;
        Reset();
    }

    public void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        LastPos = startPos;
        overallFitness = 0f;
        transform.position = startPos;
        transform.eulerAngles = startRot;
        a = 0;
        t = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collided");
        Death();
    }

    private void FixedUpdate()
    {
        InputSensors();
        LastPos = transform.position;

        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);


        
        MoveCar(a,t);

        timeSinceStart += Time.fixedDeltaTime;
        calculateFitness();

        GetComponent<NNet>().biases = network.biases;

        List<float> debugList = new List<float>();
        debugList.Clear();
        for (int i = 0; i < network.weights.Count; i++)
        {
            for (int row = 0; row < network.weights[i].RowCount; row++)
            {
                for (int col = 0; col < network.weights[i].ColumnCount; col++)
                {
                    debugList.Add(network.weights[i][row, col]);
                }
            }
        }
        GetComponent<NNet>().floatWeights = debugList;

        //t = 0;
        //a = 0;
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }

    private void calculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, LastPos);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMult) + (avgSpeed * avgSpeedMult) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if(timeSinceStart > 20 && overallFitness < FitnessThreshold20)
        {
            Death();
        }

        //if(timeSinceStart > 5 && Vector3.Distance(startPos, transform.position) < 10f)
        //{
        //    Death();
        //}

        if(overallFitness >= FitnessSavethreshold)
        {
            if (shouldSave)
            {
                SaveCurrentNet();
            }
            Death();
        }
    }

    private void InputSensors()
    {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray r = new Ray(transform.position + SensorOffset, a);
        RaycastHit hit;
        if(Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
            //print("A: " + aSensor);
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;
        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            //print("B: " + bSensor);
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            //print("C: " + cSensor);
        }
    }

    [Header("Do not change")]
    private Vector3 inp;
    public float VerticalStrength = 11.4f;
    public float HorizontalStength = 0.2f;
    public void MoveCar(float v, float h)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * VerticalStrength), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, h* 90 * HorizontalStength,0);
    }

    void SaveCurrentNet()
    {
        Debug.Log("Saving");
        if (saveSetting == SaveSettings.NeverOverride)
        {
            SaveNeverOverride();
        }
        else if(saveSetting == SaveSettings.AlwaysOverride)
        {
            SaveAlwaysOverride();
        }
        else if (saveSetting == SaveSettings.OverrideSlowest)
        {
            SaveOverrideSlowest();
        }

    }

    void SaveNeverOverride()
    {
        foreach (SavedNet savedNet in SavedNets)
        {
            if (!savedNet.isFilled)
            {
                savedNet.floatWeights = new List<float>();

                for (int i = 0; i < network.weights.Count; i++)
                {
                    for (int row = 0; row < network.weights[i].RowCount; row++)
                    {
                        for (int col = 0; col < network.weights[i].ColumnCount; col++)
                        {
                            savedNet.floatWeights.Add(network.weights[i][row, col]);
                        }
                    }
                }
                network.floatWeights.Clear();
                //network.floatWeights.AddRange(SavedNet0.floatWeights);
                savedNet.biases = new List<float>();
                savedNet.biases = network.biases;
                savedNet.avgSpeed = avgSpeed;
                savedNet.Layers = LAYERS;
                savedNet.Neurons = NEURONS;
                savedNet.isFilled = true;
                savedNet.Net_name = genMan.currentGeneration + "/" + genMan.currentGenome;
                break;
            }
        }
    }

    void SaveAlwaysOverride()
    {
        foreach (SavedNet savedNet in SavedNets)
        {
            if (!savedNet.isFilled)
            {
                Debug.Log("Saving to: " + SavedNets.IndexOf(savedNet));
                //FLOAT WEIGHTS
                savedNet.floatWeights = new List<float>();

                for (int i = 0; i < network.weights.Count; i++)
                {
                    for (int row = 0; row < network.weights[i].RowCount; row++)
                    {
                        for (int col = 0; col < network.weights[i].ColumnCount; col++)
                        {
                            savedNet.floatWeights.Add(network.weights[i][row, col]);
                        }
                    }
                }
                network.floatWeights.Clear();
                //network.floatWeights.AddRange(SavedNet0.floatWeights);
                //THE REST
                savedNet.biases = new List<float>();
                savedNet.biases = network.biases;
                savedNet.avgSpeed = avgSpeed;
                savedNet.Layers = LAYERS;
                savedNet.Neurons = NEURONS;
                savedNet.Net_name = genMan.currentGeneration + "/" + genMan.currentGenome;
                savedNet.isFilled = true;
                return;
            }
        }

        SavedNets[saveCounter].floatWeights = new List<float>();

        for (int i = 0; i < network.weights.Count; i++)
        {
            for (int row = 0; row < network.weights[i].RowCount; row++)
            {
                for (int col = 0; col < network.weights[i].ColumnCount; col++)
                {
                    SavedNets[saveCounter].floatWeights.Add(network.weights[i][row, col]);
                }
            }
        }
        network.floatWeights.Clear();
        //network.floatWeights.AddRange(SavedNet0.floatWeights);
        SavedNets[saveCounter].biases = new List<float>();
        SavedNets[saveCounter].biases = network.biases;
        SavedNets[saveCounter].avgSpeed = avgSpeed;
        SavedNets[saveCounter].Layers = LAYERS;
        SavedNets[saveCounter].Neurons = NEURONS;
        SavedNets[saveCounter].Net_name = genMan.currentGeneration + "/" + genMan.currentGenome;
        SavedNets[saveCounter].isFilled = true;
        

        //SAVECOUNTER
        if(saveCounter >= SavedNets.Count - 1) { saveCounter = 0; return; }
        saveCounter++;
    }


    void SaveOverrideSlowest()
    {
        float slowestAvgSpeed = Mathf.Infinity;
        int indexOfLowest = 0;
        foreach (SavedNet net in SavedNets)
        {
            if(net.isFilled && net.avgSpeed < slowestAvgSpeed)
            {
                slowestAvgSpeed = net.avgSpeed;
                indexOfLowest = SavedNets.IndexOf(net);
            }
        }



        SavedNets[indexOfLowest].floatWeights = new List<float>();

        for (int i = 0; i < network.weights.Count; i++)
        {
            for (int row = 0; row < network.weights[i].RowCount; row++)
            {
                for (int col = 0; col < network.weights[i].ColumnCount; col++)
                {
                    SavedNets[indexOfLowest].floatWeights.Add(network.weights[i][row, col]);
                }
            }
        }
        network.floatWeights.Clear();

        SavedNets[indexOfLowest].biases = new List<float>();
        SavedNets[indexOfLowest].biases = network.biases;
        SavedNets[indexOfLowest].avgSpeed = avgSpeed;
        SavedNets[indexOfLowest].Layers = LAYERS;
        SavedNets[indexOfLowest].Neurons = NEURONS;
        SavedNets[indexOfLowest].Net_name = genMan.currentGeneration + "/" + genMan.currentGenome;
        SavedNets[indexOfLowest].isFilled = true;
    }
}
