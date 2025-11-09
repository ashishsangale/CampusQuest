using UnityEngine;
using TMPro;
using System.Collections;

public class VendingTrigger : MonoBehaviour
{
    public GameObject drink;                // drag your Drink here
    public TextMeshProUGUI messageText;     // drag MessageText here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (drink != null)
            {
                drink.SetActive(true);
            }

            if (messageText != null)
            {
                StartCoroutine(ShowMessage("The player drank the drink!", 3f));
            }

            Debug.Log("The player drank the drink!");
        }
    }

    private IEnumerator ShowMessage(string message, float delay)
    {
        messageText.text = message;
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }
}
