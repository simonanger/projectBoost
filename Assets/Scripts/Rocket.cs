using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Rocket : MonoBehaviour 
{

    [SerializeField] float rcsThrust = 150f;
    [SerializeField] float mainThrust = 600f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip success;
    [SerializeField] float levelLoadDelay = 3f;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem successParticles;

    [SerializeField] Text textBox;

    static int levelNumber = 0;
    static int lives = 5;

    Rigidbody rigidBody;
    AudioSource audio;

    enum State { Alive, Dying, Transcending };
    State state = State.Alive;

	// Use this for initialization
	void Start () {
        print(SceneManager.sceneCountInBuildSettings);
        rigidBody = GetComponent<Rigidbody>();
        audio = GetComponent<AudioSource>();
        textBox = GetComponent<Text>();
        textBox.text = "Lives: " + lives.ToString();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Return) && levelNumber < SceneManager.sceneCountInBuildSettings -1)
        {
            levelNumber++;
            LoadNextLevel();
        }
        if (Input.GetKey(KeyCode.Return) && levelNumber == SceneManager.sceneCountInBuildSettings -1)
        {
            levelNumber = 1;
            lives = 5;
            LoadNextLevel();
        }

        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                break;
            case "Finish":
                levelNumber++;
                StartSuccessSequence();
                break;
            default:
                lives--;
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audio.Stop();
        audio.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextLevel", 1f);
        //LoadNextLevel();
    }

    private void StartDeathSequence()
    {
        if (lives > 0){
            state = State.Dying;
            audio.Stop();
            audio.PlayOneShot(death);
            deathParticles.Play();
            Invoke("LoadNextLevel", 1f);
        }
        if (lives == 0)
        {
            state = State.Dying;
            audio.Stop();
            audio.PlayOneShot(death);
            deathParticles.Play();
            Invoke("LoadFirstLevel", 1f);
            //LoadFirstLevel();
        }
    }

    private void LoadFirstLevel()
    {
        lives = 5;
        levelNumber = 0;
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(levelNumber);
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audio.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audio.isPlaying) //so it doesnt layer
        {
            audio.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true; // take manual control of rotation

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // resume physics control of rotation
    }


}
