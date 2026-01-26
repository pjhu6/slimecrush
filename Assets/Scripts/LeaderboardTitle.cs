using UnityEngine;
using TMPro;

public class LeaderboardTitle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;

    public void Initialize(string title)
    {
        titleText.text = title;
    }
}