using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    public void Move(Direction8 direction)
    {
        GetComponent<Transform>().transform.position += (Vector3Int)direction.Vector();
    }
}
