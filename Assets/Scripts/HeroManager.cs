using UnityEngine;

public class HeroManager : MonoBehaviour
{
    [SerializeField] Hero _blueHero, _redHero;
    [SerializeField] string _heroName, _card1, _card2 ,_card3, _card4, _card5, _card6, _card7, _card8;
    [SerializeField] int _speed;

    void Awake() 
    {
        GetMetaDataFromHeroNFT();
        AssignValuesToHero(_blueHero);
        AssignValuesToHero(_redHero);
    }

    void GetMetaDataFromHeroNFT() { // hardcoded for now
        _heroName = "Bob";
        // image?
        _speed = 100;
        _card1 = "1";
        _card2 = "2";
        _card3 = "4";
        _card4 = "5";
        _card5 = "A";
        _card6 = "B";
        _card7 = "C";
        _card8 = "D";
    }

    void AssignValuesToHero(Hero hero)//should some of these be in Hero?
    {
        hero.HeroName = _heroName;
        hero.Speed = _speed;
    }
}
