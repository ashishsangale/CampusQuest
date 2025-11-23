using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int coinCount = 0;

    private void Awake()
    {
        instance = this;
    }

    public void AddCoin()
    {
        coinCount++;
    }
}