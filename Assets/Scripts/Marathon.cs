using UnityEngine;
using System.Collections;

public class Marathon {
    static Marathon marathon;
    public static Marathon getMarathon {
        get { 
            if (marathon == null) marathon = new Marathon();
            return marathon;
        }
    }

    TimeMarathon timeMaraphon;
    StarMarathon starMaraphon;

    public static void updateStar(StarMarathon newStarMaraphon)
    {
        getMarathon.starMaraphon = newStarMaraphon;
    }

    public static void updateTime(TimeMarathon newTimeMaraphon)
    {
        getMarathon.timeMaraphon = newTimeMaraphon;
    }
}

public class MarathonWindow : PopUpWindow {
    public static MarathonWindow create() {
        return null;
    }
}