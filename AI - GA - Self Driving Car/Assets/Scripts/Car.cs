using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    private float carSpeed = 20.0f;
    public float fitness;
    private bool initilized = false;
    private NeuralNetwork net;
    private float inputSensorL = 0.0f;
    private float inputSensorR = 0.0f;
    private float inputSensorF = 0.0f;
    private float inputSensorFL = 0.0f;
    private float inputSensorFR = 0.0f;
    [SerializeField] private float sensorlength = 10.0f;
    private float raycastlength = 0.0f;
    private RaycastHit hitF;
    private RaycastHit hitR;
    private RaycastHit hitL;
    private RaycastHit hitFR;
    private RaycastHit hitFL;
    [SerializeField] private float outputTurnValue = 0f;
    [SerializeField] private float outputSpeedValue = 0f;
    [SerializeField] private float timer = 0f;
    [SerializeField] private float totalDistanceFromLastPos = 0f;
    private Vector3 lastPosition;
    public bool isHit = false;
    private Renderer cubeRenderer;

    void Start()
    {
        raycastlength = Mathf.Sqrt((sensorlength * sensorlength) + (sensorlength * sensorlength));
        lastPosition = transform.position;

        
        cubeRenderer = this.GetComponent<Renderer>();

        defaultColor();
    }

    public void changeColor()
    {
        cubeRenderer.material.SetColor("_Color", Color.yellow);
    }

    public void defaultColor()
    {
        cubeRenderer.material.SetColor("_Color", Color.white);
    }
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        float distanceFromLastPos = Vector3.Distance( lastPosition, transform.position ) ;
        totalDistanceFromLastPos += distanceFromLastPos ;
        lastPosition = transform.position ;

        if(isHit == false)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(0, 0, 1) * sensorlength, Color.green);
            Debug.DrawRay(transform.position, transform.TransformDirection(1, 0, 0) * sensorlength, Color.green);
            Debug.DrawRay(transform.position, transform.TransformDirection(-1, 0, 0) * sensorlength, Color.green);
            Debug.DrawRay(transform.position, transform.TransformDirection(1,0,1) * sensorlength, Color.green);
            Debug.DrawRay(transform.position, transform.TransformDirection(-1,0,1) * sensorlength, Color.green);

            Physics.Raycast(transform.position, transform.TransformDirection(0, 0, 1), out hitF, 10.0f);
            inputSensorF = hitF.distance;
            Physics.Raycast(transform.position, transform.TransformDirection(1, 0, 0), out hitL, 10.0f);
            inputSensorL = hitL.distance;
            Physics.Raycast(transform.position, transform.TransformDirection(-1, 0, 0), out hitR, 10.0f);
            inputSensorR = hitR.distance;
            Physics.Raycast(transform.position, transform.TransformDirection(1,0,1), out hitFR, raycastlength);
            inputSensorFR = hitFR.distance;
            Physics.Raycast(transform.position, transform.TransformDirection(-1,0,1), out hitFL, raycastlength);
            inputSensorFL = hitFL.distance;
        }

        if (initilized == true && isHit == false)
        {
            float[] inputs = new float[5];
			inputs[0] = inputSensorF;
            inputs[1] = inputSensorL;
            inputs[2] = inputSensorR;
            inputs[3] = inputSensorFL;
            inputs[4] = inputSensorFR;
            float[] output = net.FeedForward(inputs);
    
            outputTurnValue = output[0];
            outputSpeedValue = output[1];
            transform.Rotate(new Vector3(0, outputTurnValue * 15.0f, 0) * Time.fixedDeltaTime * carSpeed);
            transform.position = transform.position + transform.TransformDirection(Vector3.forward) * Time.fixedDeltaTime * carSpeed * Mathf.Abs(outputSpeedValue * 1.5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            fitness = totalDistanceFromLastPos/1000;
            net.SetFitness(fitness);
            //Debug.Log(fitness);
            carSpeed = 0.0f;
            isHit = true;
            Values.isCollide += 1;
        }
    }
    public void Init(NeuralNetwork net)
    {
        this.net = net;
        initilized = true;
    }
}
