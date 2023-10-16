using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Client client = new(new byte[]{10, 2, 103, 130}, 11000);
        client.Send("BOB WAS FUCKING HERE");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
