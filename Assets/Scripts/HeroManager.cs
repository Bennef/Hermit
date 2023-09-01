using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public Hero blueHero, redHero;
    public string heroName, card1, card2 ,card3, card4, card5, card6, card7, card8;
    public int speed;

    void Awake() {
        GetMetaDataFromHeroNFT();
        AssignValuesToHero(blueHero);
        AssignValuesToHero(redHero);
    }

    void GetMetaDataFromHeroNFT() { // hardcoded for now
        heroName = "Bob";
        // image?
        speed = 100;
        card1 = "1";
        card2 = "2";
        card3 = "4";
        card4 = "5";
        card5 = "A";
        card6 = "B";
        card7 = "C";
        card8 = "D";
    }

    void AssignValuesToHero(Hero hero) {//should some of these be in Hero?
        hero.name = heroName;
        hero.speed = speed;
        AssignCardsToHero(hero);
    }

    void AssignCardsToHero(Hero hero) {
        
    }

    void AssignCardValues() {

    }
}
// we want a prefab for every Card
// Each card will have to find it's own ghost counters
