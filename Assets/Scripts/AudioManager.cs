using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource _aSrc;
    [SerializeField] private AudioClip _selectSquare, _cardUp, _cardDown, _idolWin, _playerDead,
                                       _slideCounter, _strongAttackHit, _strongAttackMiss, 
                                       _collectIdol, _uiButton;

    public AudioClip SelectSquare { get => _selectSquare; set => _selectSquare = value; }
    public AudioClip CardUp { get => _cardUp; set => _cardUp = value; }
    public AudioClip CardDown { get => _cardDown; set => _cardDown = value; }
    public AudioClip IdolWin { get => _idolWin; set => _idolWin = value; }
    public AudioClip PlayerDead { get => _playerDead; set => _playerDead = value; }
    public AudioClip SlideCounter { get => _slideCounter; set => _slideCounter = value; }
    public AudioClip StrongAttackHit { get => _strongAttackHit; set => _strongAttackHit = value; }
    public AudioClip StrongAttackMiss { get => _strongAttackMiss; set => _strongAttackMiss = value; }
    public AudioClip CollectIdol { get => _collectIdol; set => _collectIdol = value; }
    public AudioClip UIButton { get => _uiButton; set => _uiButton = value; }

    void Start() => _aSrc = GetComponent<AudioSource>();

    public void PlaySound(AudioClip soundToPlay) => _aSrc.PlayOneShot(soundToPlay);
}