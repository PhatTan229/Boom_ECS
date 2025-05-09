using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSateData 
{
    private static AnimationSateData instance;
    public static AnimationSateData Instance
    {
        get
        {
            if(instance == null) instance = new AnimationSateData();
            return instance;
        }
    }

    public AnimationSateData()
    {

    }
}
