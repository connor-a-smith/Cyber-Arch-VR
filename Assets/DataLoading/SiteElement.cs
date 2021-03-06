﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public abstract class SiteElement : MonoBehaviour
{
    //public GameObject caveLogo;

    // Variables to keep track of the loading status.
    public bool loaded = false;
    public bool failed = false;

    // Which site this element is part of.
    public Site parentSite;

    // The serializable data (from JSON) associated with this site element.
    protected SerializableSiteElement siteData;

    // The Coroutines that need to be implemented in children.
    protected abstract IEnumerator LoadCoroutine();
    protected abstract IEnumerator UnloadCoroutine();
    protected abstract IEnumerator ActivateCoroutine();
    protected abstract IEnumerator DeactivateCoroutine();

    // Initializes this site element by setting the parent site and site data.
    public void Initialize(SerializableSiteElement siteData, Site parentSite)
    {
        this.parentSite = parentSite;
        this.siteData = siteData;
    }

    // Activate function that returns an Acitvate coroutine.
    public Coroutine Activate()
    {

        // Hide the Catalyst logo.
        Logo.Hide();

        // Hide the Earth
        CatalystEarth.Hide();

        // Load the Data Scene
        SceneManager.LoadScene("DataScene");

        // Show the panel for CAVE Cams
        ControlPanel.SetCaveCamPanel();

        // Start the activation coroutine (load first if needed)
        Coroutine activeCoroutine = StartCoroutine(LoadThenActivate());

        // Mark the active element as this element.
        SiteManager.activeSiteElement = this;

        // Return the coroutine.
        return activeCoroutine;
    }

    // Deactivate this site element to clean it up.
    public Coroutine Deactivate()
    {

        // Start a coroutine to deactivate the object.
        Coroutine deactivateCoroutine = StartCoroutine(DeactivateCoroutine());

        // If this is the active site elmeent, mark the active site element as null.
        if (SiteManager.activeSiteElement == this)
        {
            SiteManager.activeSiteElement = null;
        }

        // Return the deactivation coroutine.
        return deactivateCoroutine;
    }

    // Function that starts the WaitForLoad coroutine and returns it.
    public Coroutine Load()
    {
 
        // Start WaitForLoad() and return it, so it can be waited for.
        return StartCoroutine(WaitForLoad());

    }

    // Unloads this object, by starting the Unload coroutine and returning it.
    public Coroutine Unload()
    {
        // Start WaitForUnload(), and return it so it can be waited for.
        return StartCoroutine(WaitForUnload());
    }

    // Wait for the object to load. Adds a layer of abstraction.
    private IEnumerator WaitForLoad()
    {

        // Don't try to load two different elements at once. Could get messy.
        while (SiteManager.loading)
        {
            yield return null;
        }

        // Lock user input so they can't interact while an object is loading.
        GamepadInput.LockInput(true);
        SiteManager.loading = true;

        // Show the loading status.
        StatusText.Show();

        // If there's no specified scene name in the JSON, then we just load the data normally.
        if (string.IsNullOrEmpty(siteData.sceneName))
        {

            yield return StartCoroutine(LoadCoroutine());
        }

        // Otherwise, we just load the scene specified in JSON.
        else
        {
            // Variable to keep track if we actually found the scene or not.
            bool foundScene = false;

            // Iterate through all existing scenes.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {

                // Get the scene at specified index.
                Scene scene = SceneManager.GetSceneAt(i);

                // Check if the names match.
                if (scene.name.Equals(siteData.sceneName))
                {

                    // If they match, scene was found. Load the scene.
                    foundScene = true;
                    AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(siteData.sceneName);

                    // Load the scene specified in JSON and wait for it to finish loading.
                    while (!sceneLoad.isDone)
                    {
                        yield return null;
                    }

                    // Break, since we found the scene.
                    break;
                }
            }

            // If we didn't find the scene, log an error and try to load normally... which probably won't work.
            if (!foundScene)
            {
                Debug.LogErrorFormat("Could not find scene name '{0}', which was provided in JSON data file. Ensure this scene exists and is added to build settings.");
                yield return StartCoroutine(LoadCoroutine());
            }

        }

        // Everything is now loaded.
        loaded = true;

        // Hide the status text.
        StatusText.Hide();

        // Unlock input and mark scene as not loading anymore.
        GamepadInput.LockInput(false);
        SiteManager.loading = false;
    }

    // Coroutine to wait for the unload coroutine.
    private IEnumerator WaitForUnload()
    {
        // Waits for unload, then marks it as unloaded afterwards.
        yield return StartCoroutine(UnloadCoroutine());
        loaded = false;
    }

    // Wait for this element to load, then activate it.
    private IEnumerator LoadThenActivate()
    {

        // If this object is not already loaded, then load it and wait for it to finish.
        if (!loaded)
        {
            Debug.LogFormat(gameObject, "Site element {0} not yet loaded. Will activate when done loading.", gameObject.name);
            yield return Load();
        }

        // Activate the element now that it's guaranteed to have loaded.
        yield return StartCoroutine(ActivateCoroutine());

    }
    
    // Print an incorrect type error.
    protected void PrintIncorrectTypeError(string siteName, string dataType)
    {
        Debug.LogErrorFormat("Could not load site element {0} at site {1}: Incorrect data passed to Activate method", dataType, siteName);
    }

    // Get an absolute path of data on the computer.
    public string GetAbsolutePath(string relativeDataPath)
    {

        // Create the full path.
        string filePath = "/" + SiteManager.pathToDataFolder + "/" + relativeDataPath;

        // Ensure the file actually exists
        if (!File.Exists(SiteManager.pathToDataFolder + "/" + "config_3.json"))
        {
            Debug.LogWarning("Could not find config file");
        }

        // Ensure the full filepath exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("Could not find data file at: " + filePath);
        }

        // Return the absolute path.
        return filePath;

    }
}

[System.Serializable]
public abstract class SerializableSiteElement
{

    public int id;
    public string name;
    public string description;

    // IF the scene string is present, Unity will just load the scene with the specified name, ignoring all other JSON elements.
    public string sceneName;

}
