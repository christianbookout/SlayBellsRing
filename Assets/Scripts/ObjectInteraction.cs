using UnityEngine;
using TMPro;

public class ObjectInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    public TextMeshProUGUI partsFoundText;
    public TextMeshProUGUI pickUpText;

    private int partsFound = 0;
    private int totalParts = 4;  // Adjust this based on the total number of parts in your game

    private void Start()
    {
        UpdatePartsFoundText();
    }

    private void Update()
    {
        // Cast a ray from the camera to detect objects
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Check if the ray hits an object on the interactable layer
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Show the "Pick Up" TextMeshPro UI text
            pickUpText.gameObject.SetActive(true);

            // Check for user input to pick up the object
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                // Perform the pickup action
                PickUpObject(hit.transform.gameObject);
            }
        }
        else
        {
            // Hide the "Pick Up" TextMeshPro UI text if no object is in range
            pickUpText.gameObject.SetActive(false);
        }

        // Check if all parts are found
        if (partsFound == totalParts)
        {
            // Update the "Parts Found" TextMeshPro UI text to show the completion message
            partsFoundText.text = "RETURN TO SLEIGH!";
        }
    }

    private void PickUpObject(GameObject objectToPickUp)
    {
        // Example: Destroy the object when picked up
        Destroy(objectToPickUp);

        // Increment the parts found counter
        partsFound++;

        // Update the "Parts Found" TextMeshPro UI text
        UpdatePartsFoundText();

        // Hide the "Pick Up" TextMeshPro UI text after picking up
        pickUpText.gameObject.SetActive(false);

        // You can add more logic here, such as playing a sound, adding to inventory, etc.
    }

    private void UpdatePartsFoundText()
    {
        // Update the "Parts Found" TextMeshPro UI text to show parts found
        partsFoundText.text = "PARTS FOUND: " + partsFound + "/" + totalParts;
    }
}