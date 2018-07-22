using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource audioSource;
    [SerializeField] Light thrustLight;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] float rcsThrust = 75;
    [SerializeField] float mainThrust = 2000f;
    enum State { Alive, Dying, Transcending };
    State state = State.Alive;
    bool collisionsEnabled = true;

	// Use this for initialization
	void Start ()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        ProcessInput();
	}

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent further collisions after dying or finishing level
        if(state != State.Alive || collisionsEnabled == false)
        {
            return;
        }

        // Check different types of collisions
        switch(collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                break;
            case "Fuel":
                ReFuel();
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextScene", 2f);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(death);
        deathParticles.Play();
        Invoke("Dead", 2f);
    }

    private void Dead()
    {
        SceneManager.LoadScene(0);
        deathParticles.Stop();
    }

    private void ReFuel()
    {
        print("Re-fueled!");
    }

    private void LoadNextScene()
    {
        // Go to next scene, or to beginning if at the last scene
        int numberOfScenes = SceneManager.sceneCountInBuildSettings;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % numberOfScenes;
        SceneManager.LoadScene(nextSceneIndex);
        successParticles.Stop();
    }

    private void ProcessInput()
    {
        if(state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotatationInput();
            if(Debug.isDebugBuild)
            {
                RespondToDebugKeys();
            }            
        }        
    }

    private void RespondToDebugKeys()
    {
        // Load Next Level
        if(Input.GetKey(KeyCode.L))
        {
            LoadNextScene();
        }
        // Toggle Collisions
        if (Input.GetKey(KeyCode.C))
        {
            if(collisionsEnabled == true)
            {
                collisionsEnabled = false;
            }
            else
            {
                collisionsEnabled = true;
            }
        }
    }

    private void RespondToRotatationInput()
    {
        rigidBody.freezeRotation = true;
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {            
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false;
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
            thrustLight.intensity = 0.1f;
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
            mainEngineParticles.Play();
            thrustLight.intensity = 1f;
        }
    }
}
