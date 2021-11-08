using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointAudioPlayer : MonoBehaviour
{
    public ArticulationBody[] jointRoots;
    public AudioClip audioClip;

    private ArticulationBody[] currentList;
    private ArticulationBody[] allJointsList;
    private AudioSource jointAudio;

    public float lowerFilter = 0.5f;

    public float volume = 0.2f;
    private float inSpeed = 5f;
    private float fadeSpeed = 10f;

    public bool changePitch = true;
    public float minimumPitch = 0.0f;
    public float maximumPitch = 1.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (ArticulationBody jointRoot in jointRoots)
        {
            currentList = jointRoot.GetComponentsInChildren<ArticulationBody>();
            currentList = currentList.Where(joint => joint.jointType 
                          != ArticulationJointType.FixedJoint).ToArray();
            AddToJointList(currentList);
        }
        
        // Initialize audio sourse component for all joints
        foreach (ArticulationBody joint in allJointsList)
        {
            jointAudio = joint.gameObject.AddComponent<AudioSource>();
            jointAudio.clip = audioClip;
            jointAudio.loop = true;
            jointAudio.volume = 0f;
            jointAudio.pitch = maximumPitch;

            if (changePitch)
                jointAudio.pitch = minimumPitch;

            jointAudio.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (ArticulationBody joint in allJointsList)
        {
            // Get joint audio component
            jointAudio = joint.gameObject.GetComponent<AudioSource>();
            float speed = Mathf.Abs(joint.jointVelocity[0]);

            // Gradually in and fade audio
            if (speed < lowerFilter && jointAudio.volume != 0f)
            {
                jointAudio.volume = Mathf.Lerp(jointAudio.volume, 0f, 
                                                Time.deltaTime * fadeSpeed);
            }
            else if (speed > lowerFilter && jointAudio.volume != volume)
            {
                jointAudio.volume = Mathf.Lerp(jointAudio.volume, volume, 
                                                Time.deltaTime * inSpeed);
            }

            // If pitch needs to be changed
            if (changePitch)
            {
                speed /= 25f;
                if (speed < minimumPitch)
                    jointAudio.pitch = minimumPitch;
                else if(speed > maximumPitch)
                    jointAudio.pitch = maximumPitch;
                else
                    jointAudio.pitch = speed;
            }
        }
    }

    private void AddToJointList(ArticulationBody[] newList)
    {
        if (allJointsList == null)
            allJointsList = newList;
        else
        {
            ArticulationBody[] resultList = 
                new ArticulationBody[allJointsList.Length + newList.Length];
            allJointsList.CopyTo(resultList, 0);
            newList.CopyTo(resultList, allJointsList.Length);

            allJointsList = resultList;
        }
    }
}
