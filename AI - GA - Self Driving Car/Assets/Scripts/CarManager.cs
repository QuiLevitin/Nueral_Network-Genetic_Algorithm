using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class CarManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gen;
    [SerializeField] private TextMeshProUGUI nn;
    [SerializeField] private Button terminateB;
    [SerializeField] private Button switchB;
    [SerializeField] private Button save0;
    [SerializeField] private Button load0;
    [SerializeField] private TextMeshProUGUI Gnum0;
    [SerializeField] private Button save1;
    [SerializeField] private Button load1;
    [SerializeField] private TextMeshProUGUI Gnum1;
    [SerializeField] private Button save2;
    [SerializeField] private Button load2;
    [SerializeField] private TextMeshProUGUI Gnum2;
    [SerializeField] private Button save3;
    [SerializeField] private Button load3;
    [SerializeField] private TextMeshProUGUI Gnum3;
    [SerializeField] private Button save4;
    [SerializeField] private Button load5;
    [SerializeField] private TextMeshProUGUI Gnum4;
    [SerializeField] private GameObject inputLayerNode;
    [SerializeField] private GameObject hiddenLayerNode;
    [SerializeField] private GameObject outputLayerNode;
    [SerializeField] private GameObject spawnNet;

    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject spawnPoint;

    [SerializeField] private bool isTraning = false;
    [SerializeField] private int populationSize = 100;
    [SerializeField] private int generationNumber = 0;
    private int[] layers = new int[] { 5, 5, 5, 2}; 
    [SerializeField] private List<NeuralNetwork> nets;
    [SerializeField] public List<Car> carList = null;
    [SerializeField] private int collided = 0;
    [SerializeField] private float prevBestFitness = 0f;
    [SerializeField] private int BestFitnessNet = 0;
    private NeuralNetwork bestNet;
    [SerializeField] private bool terminateTrain = false;
    [SerializeField] private bool switchCar = false;
    [SerializeField] private List<LineRenderer> lines;
    [SerializeField] private LineRenderer line;
    [SerializeField] private List<Vector3> nodes;
    private float nodesDistanceHorizontal = 10f;
    private float nodesDistanceVertical = 15f;
    private int counter = 0;
    private int counter2 = 0;
    
    void Start()
    {
        int numOfLayer = layers.Length;
        int numOfInputNodes = layers[0];
        int numOfOutputNodes = layers[numOfLayer - 1];
        int numOfHiddenLayers = numOfLayer - 2;

        for(int i = 0; i < numOfInputNodes; i++)
        {
            Instantiate(inputLayerNode, spawnNet.transform.position + new Vector3(0, 0, i * -nodesDistanceVertical), spawnNet.transform.rotation);
            inputLayerNode.name = "ILN: " + i.ToString();
            nodes.Add(spawnNet.transform.position + new Vector3(0, 0, i * -nodesDistanceVertical));
        }
        for(int i = 0; i < numOfHiddenLayers; i++)
        {        
            for(int j = 0; j < layers[i]; j++)
            {
                Instantiate(hiddenLayerNode, spawnNet.transform.position + new Vector3((i + 1) * nodesDistanceHorizontal, 0, j * -nodesDistanceVertical), spawnNet.transform.rotation);
                hiddenLayerNode.name = "HLN: " + j.ToString() + i.ToString();
                nodes.Add(spawnNet.transform.position + new Vector3((i + 1) * nodesDistanceHorizontal, 0, j * -nodesDistanceVertical));
            }
        }
        for(int i = 0; i < numOfOutputNodes; i++)
        {
            Instantiate(outputLayerNode, spawnNet.transform.position + new Vector3((numOfHiddenLayers + 1) * nodesDistanceHorizontal, 0, i * -nodesDistanceVertical), spawnNet.transform.rotation);
            outputLayerNode.name = "OLN: " + i.ToString();
            nodes.Add(spawnNet.transform.position + new Vector3((numOfHiddenLayers + 1) * nodesDistanceHorizontal, 0, i * -nodesDistanceVertical));
        }

        //proceed to 2nd layer of layers
        for(int i = 1; i < layers.Length; i++)
        {
            //number of nodes per layer
            for(int j = 0; j < layers[i]; j++)
            {
                //number of nodes from the previous layer per nodes
                for(int k = 0; k < layers[i-1]; k++)
                {
                    line.positionCount = 2;
                    line = new GameObject().AddComponent<LineRenderer>();
                    lines.Add(line);
                    line.SetPosition (0, nodes[counter + k]);
                    line.SetPosition (1, nodes[counter + j + (layers[i-1])]);
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.startColor = new Color(0f, 10f,130f, 1f);
                    line.endColor = new Color(0f, 10f, 30f, 1f);
                    line.startWidth = 0.2f;
                    line.endWidth = 0.2f;

                }
            }
                counter += layers[i-1];
        }
    }

    public void CloseApp()
    {
        Application.Quit();
    }
    private void resetTraining()
    {
        if(nets.Count != 0) nets.Clear();
        if(lines.Count != 0) lines.Clear();
        if(nodes.Count != 0) nodes.Clear();
        generationNumber = 0;
        Values.isCollide = 0;
        BestFitnessNet = 0;
        prevBestFitness = 0f;
        counter = 0;
        counter2 = 0;
        isTraning = false;
        if (carList != null)
        {
            for (int i = 0; i < carList.Count; i++)
            {
                GameObject.Destroy(carList[i].gameObject);
            }
            carList.Clear();
        }
        
    }

	void FixedUpdate ()
    {
        gen.text = "Generation#["+ generationNumber.ToString() +"]";
        collided = Values.isCollide;
        if (isTraning == false)
        {
            if (generationNumber == 0)
            {
                InitBoomerangNeuralNetworks();
            }
            generationNumber++;
            isTraning = true;
            CreateBoomerangBodies();
        }
        if (Values.isCollide >= populationSize || terminateTrain == true)
        {
            for (int i = 0; i < populationSize; i++)
            {
                if(carList[i].fitness > prevBestFitness)
                {
                    prevBestFitness = carList[i].fitness;
                    nets[0] = new NeuralNetwork(nets[i]);
                }
            }
            for (int i = 1; i < populationSize; i++)
            {
                nets[i] = new NeuralNetwork(nets[0]);
                nets[i].Mutate();
            }

            isTraning = false;
            terminateTrain = false;
            Values.isCollide = 0;
            BestFitnessNet = 0;
        }
        
        if(carList[BestFitnessNet].isHit == true || switchCar == true)
        {
            carList[BestFitnessNet].defaultColor();
            BestFitnessNet = UnityEngine.Random.Range(0, populationSize - 1);
            carList[BestFitnessNet].changeColor();
            switchCar = false;
        }

        DisplayNeuralNetwork(BestFitnessNet);
    }

    private void DisplayNeuralNetwork(int a)
    {
        nn.text = "Neural Network of Car#["+ a.ToString() +"]";
        counter2 = 0;
        for (int i = 1; i < nets[a].neurons.Length; i++)
        {
            for (int j = 0; j < nets[a].neurons[i].Length; j++)
            {
                for (int k = 0; k < nets[a].neurons[i - 1].Length; k++)
                {
                    float value = 0f;
                    value = nets[a].neurons[i - 1][k] * nets[a].neurons[i][j]; 
                    
                    lines[counter2].startWidth = (float)Math.Tanh(value);
                    lines[counter2].endWidth = (float)Math.Tanh(value);

                    counter2++;
                }
            }
        }
    }
    public void Terminate()
    {
        terminateTrain = true;
    }

    public void SwitchCar()
    {
        switchCar = true;
    }
    public void SaveGene1()
    {
        for (int i = 0; i < nets[0].weights.Length; i++)
        {
            for (int j = 0; j < nets[0].weights[i].Length; j++)
            {
                for (int k = 0; k < nets[0].weights[i][j].Length; k++)
                {
                    PlayerPrefs.SetFloat("Weight1[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]", nets[0].weights[i][j][k]);
                }
            }
        }
        PlayerPrefs.SetInt("Gene1:", generationNumber);
    }

    public void SaveGene2()
    {
        for (int i = 0; i < nets[0].weights.Length; i++)
        {
            for (int j = 0; j < nets[0].weights[i].Length; j++)
            {
                for (int k = 0; k < nets[0].weights[i][j].Length; k++)
                {
                    PlayerPrefs.SetFloat("Weight2[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]", nets[0].weights[i][j][k]);
                }
            }
        }
        PlayerPrefs.SetInt("Gene2:", generationNumber);
    }

    public void SaveGene3()
    {
        for (int i = 0; i < nets[0].weights.Length; i++)
        {
            for (int j = 0; j < nets[0].weights[i].Length; j++)
            {
                for (int k = 0; k < nets[0].weights[i][j].Length; k++)
                {
                    PlayerPrefs.SetFloat("Weight3[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]", nets[0].weights[i][j][k]);
                }
            }
        }
        PlayerPrefs.SetInt("Gene3:", generationNumber);
    }

    public void SaveGene4()
    {
        for (int i = 0; i < nets[0].weights.Length; i++)
        {
            for (int j = 0; j < nets[0].weights[i].Length; j++)
            {
                for (int k = 0; k < nets[0].weights[i][j].Length; k++)
                {
                    PlayerPrefs.SetFloat("Weight4[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]", nets[0].weights[i][j][k]);
                }
            }
        }
        PlayerPrefs.SetInt("Gene4:", generationNumber);
    }

    public void SaveGene5()
    {
        for (int i = 0; i < nets[0].weights.Length; i++)
        {
            for (int j = 0; j < nets[0].weights[i].Length; j++)
            {
                for (int k = 0; k < nets[0].weights[i][j].Length; k++)
                {
                    PlayerPrefs.SetFloat("Weight[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]", nets[0].weights[i][j][k]);
                }
            }
        }
        PlayerPrefs.SetInt("Gene5:", generationNumber);
    }

    public void LoadGene1()
    {
        resetTraining();
        for (int i = 0; i < nets[0].weights.Length; i++)
        {
            for (int j = 0; j < nets[0].weights[i].Length; j++)
            {
                for (int k = 0; k < nets[0].weights[i][j].Length; k++)
                {
                    PlayerPrefs.SetFloat("Weight1[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]", nets[0].weights[i][j][k]);
                }
            }
        }
        PlayerPrefs.SetInt("Gene1:", generationNumber);
        //PlayerPrefs.GetFloat(("Weight[" + i.ToString() + "][" + j.ToString() + "][" + k.ToString() + "]").ToString());
    }

    private void CreateBoomerangBodies()
    {
        if (carList != null)
        {
            for (int i = 0; i < carList.Count; i++)
            {
                GameObject.Destroy(carList[i].gameObject);
            }

        }

        carList = new List<Car>();

        for (int i = 0; i < populationSize; i++)
        {
            Car car = ((GameObject)(Instantiate(carPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation))).GetComponent<Car>();
            car.Init(nets[i]);
            carList.Add(car);
        }

    }

    void InitBoomerangNeuralNetworks()
    {
        nets = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            nets.Add(net);
        }
    }
}
