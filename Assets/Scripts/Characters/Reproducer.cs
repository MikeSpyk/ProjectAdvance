using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender{Male,Female}

public interface Reproducer
{
    public GameObject getGameObject();
    public float getAttractiveness();
    public Gender getGender();
    public void setPregnant(Reproducer parent);
}
