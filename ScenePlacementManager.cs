using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;

public class ObjPlacementManager : MonoBehaviour
{
    public List<GameObject> SpawnableFurniture; // List of spawnable objects
    public XROrigin origin;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public Button backButton;
    public Button deleteButton;

    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private Dictionary<string, GameObject> placedObjects = new Dictionary<string, GameObject>(); // Store placed objects by name
    private GameObject selectedObject = null; // Currently selected object for interaction

    // Dragging
    private bool isDragging = false;

    // Zooming
    private float initialDistance;
    private Vector3 initialScale;

    private void Start()
    {
        // Assign Back Button functionality
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => GoToMainMenu());
        }
        LoadPlacedObjects();
    }

    private void LoadPlacedObjects()
{
    string currentScene = SceneManager.GetActiveScene().name;

    if (GlobalPlacementData.sceneObjects.ContainsKey(currentScene))
    {
        List<PlacedObjectInfo> objectList = GlobalPlacementData.sceneObjects[currentScene];

        foreach (var info in objectList)
        {
            GameObject prefab = SpawnableFurniture.Find(obj => obj.name == info.prefabName);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, info.position, info.rotation);
                placedObjects[info.prefabName] = obj;
            }
        }
    }
}


    private void Update()
    {
        // Object Placement
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !isButtonPressed())
        {
            bool collision = raycastManager.Raycast(Input.GetTouch(0).position, raycastHits, TrackableType.PlaneWithinPolygon);

            if (collision)
            {
                PlaceObjectAtTouchPosition();
            }
        }

        // Handle Dragging
        HandleDragging();

        // Handle Zooming
        HandleZooming();

        // Handle Rotating
        HandleRotating();
    }

    private void PlaceObjectAtTouchPosition()
    {
        // Check if selected object is set and hasn't already been placed
        if (selectedObject != null && !placedObjects.ContainsKey(selectedObject.name))
        {
            GameObject newObject = Instantiate(selectedObject);
            newObject.transform.position = raycastHits[0].pose.position;
            newObject.transform.rotation = raycastHits[0].pose.rotation;
    

            // Add the placed object to the dictionary
            placedObjects.Add(selectedObject.name, newObject);

            // Optionally disable planes
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
            planeManager.enabled = false;
        }
    }

    public bool isButtonPressed()
    {
        return EventSystem.current.currentSelectedGameObject?.GetComponent<Button>() != null;
    }

    public void SwitchFurniture(GameObject furniture)
    {
        // Update the selected object
        selectedObject = furniture;
    }

    public void DeleteObject()
    {
        if (selectedObject != null && placedObjects.ContainsKey(selectedObject.name))
        {
            Destroy(placedObjects[selectedObject.name]);
            placedObjects.Remove(selectedObject.name);

            // Reactivate planes after deleting the object
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(true);
            }
            planeManager.enabled = true;
        }
    }

    private void HandleDragging()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            if (selectedObject != null)
            {
                Vector2 touchPosition = Input.GetTouch(0).position;
                if (raycastManager.Raycast(touchPosition, raycastHits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = raycastHits[0].pose;
                    selectedObject.transform.position = hitPose.position;
                }
            }
        }
    }

    public void StartDragging(GameObject obj)
    {
        selectedObject = obj;
        isDragging = true;
    }

    public void StopDragging()
    {
        isDragging = false;
        selectedObject = null;
    }

    private void HandleZooming()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                if (initialDistance == 0)
                {
                    initialDistance = currentDistance;
                    if (selectedObject != null)
                    {
                        initialScale = selectedObject.transform.localScale;
                    }
                }

                if (selectedObject != null)
                {
                    float scaleFactor = currentDistance / initialDistance;
                    selectedObject.transform.localScale = initialScale * scaleFactor;
                }
            }
        }
        else
        {
            initialDistance = 0; // Reset when touch ends
        }
    }

    private void HandleRotating()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved && !isDragging)
        {
            if (selectedObject != null)
            {
                Touch touch = Input.GetTouch(0);
                selectedObject.transform.Rotate(0f, touch.deltaPosition.x * 0.5f, 0f);
            }
        }
    }

    public void SelectObject(GameObject obj)
    {
        selectedObject = obj;
    }

    private void SavePlacedObjects()
{
    string currentScene = SceneManager.GetActiveScene().name;

    List<PlacedObjectInfo> placedInfoList = new List<PlacedObjectInfo>();

    foreach (var kvp in placedObjects)
    {
        GameObject obj = kvp.Value;

        PlacedObjectInfo info = new PlacedObjectInfo
        {
            prefabName = kvp.Key,
            position = obj.transform.position,
            rotation = obj.transform.rotation
        };

        placedInfoList.Add(info);
    }

    if (GlobalPlacementData.sceneObjects.ContainsKey(currentScene))
    {
        GlobalPlacementData.sceneObjects[currentScene] = placedInfoList;
    }
    else
    {
        GlobalPlacementData.sceneObjects.Add(currentScene, placedInfoList);
    }
}


    public void GoToMainMenu()
    {
        SavePlacedObjects();
        SceneManager.LoadScene("MainMenu");
    }
}
