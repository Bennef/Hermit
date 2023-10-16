using TMPro;
using UnityEngine;

public class HCTManager : MonoBehaviour
{
    public static HCTManager Instance;

    [Header("Blue Player")]
    [SerializeField] int _blueSpeed;
    [SerializeField] TMP_InputField _blueSpeedInput;
    GameObject[] _blueCards = new GameObject[8];

    [Header("Red Player")]
    [SerializeField] int _redSpeed;
    [SerializeField] TextMeshProUGUI _redSpeedInput;
    GameObject[] _redCards = new GameObject[8];

    [Header("Game")]
    [SerializeField] GameObject _selectedImage;
    [SerializeField] int _idolPosInt;
    [SerializeField] GameObject _idolImage;
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
    public int BlueSpeed { get { return _blueSpeed; }}
    public GameObject[] RedCards { get { return _redCards; } }
    public int RedSpeed { get { return _redSpeed; } }
    public int IdolPosInt{ get { return _idolPosInt; } }

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

    void Start()
    {
        SetDefaults();
    }

    public void UpdateIntFromInputField(TMP_InputField inputField)
    {
        if (inputField == _blueSpeedInput)
            _blueSpeed = int.Parse(inputField.text);
        else
            _redSpeed = int.Parse(inputField.text);
    }

    void SetDefaults()
    {
        // Speed
        _blueSpeed = 100;
        _redSpeed = 90;

        // Cards

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

    public void IdolClickableSquareClicked(Transform clickableSquare)
    {
        _idolImage.SetActive(true);
        _idolImage.transform.position = clickableSquare.position;
        _idolPosInt = int.Parse(clickableSquare.name.Substring(clickableSquare.name.Length - 2));
    }
}
