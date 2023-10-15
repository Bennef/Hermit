using UnityEngine;

public class HCTManager : MonoBehaviour
{
    public static HCTManager Instance;

    [Header("Blue Player")]
    [SerializeField] int _blueSpeed;
    GameObject[] _blueCards = new GameObject[8];

    [Header("Red Player")]
    [SerializeField] int _redSpeed;
    GameObject[] _redCards = new GameObject[8];

    [Header("Game")]
    [SerializeField] GameObject _selectedImage;
    [SerializeField] string idolPos = "";
    [SerializeField] HCTSlot _selectedSlot;

    public GameObject SelectedImage
    {
        get { return _selectedImage; }
        set { _selectedImage = value; }
    }

    public HCTSlot SelectedSlot
    {
        get { return _selectedSlot; }
        set { _selectedSlot = value; }
    }

    public GameObject[] BlueCards { get { return _blueCards; }}
    public GameObject[] RedCards { get { return _redCards; } }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach(GameObject obj in _blueCards)
            {
                print(obj);
            }
        }
    }

    public void InsertCardToList(int index, GameObject card, GameObject[] cardArray)
    {
        cardArray[index] = card;
    }
}
