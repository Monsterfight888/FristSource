using UnityEngine;


[CreateAssetMenu(fileName = "New Class", menuName = "Class")]
public class Classes : ScriptableObject
{
    public float cameraHieght;


    public GameObject GFXPref;

    public Item[] loudout;

    //For when you accidentally model the class model backwards
    public bool isBackwards;

    public float bodyDamper;
}
