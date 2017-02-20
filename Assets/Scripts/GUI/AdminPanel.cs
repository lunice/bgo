using UnityEngine;
using System.Collections;
// Недоделаная админ панель для дизайнера
// для разного рода изменения цветов, позиций, объектов розыгрыша
public class AdminPanel : MonoBehaviour {
    MAIN main = MAIN.getMain;

    enum Side {
        LEFT, RIGHT, TOP, BOTTOM,
        TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT
    }
    Side hideAtBorder = Side.LEFT;
    public Vector2 showPosition = new Vector2(0.0f, 0.0f);
    Vector2 hidePosition;
    Vector2 position;
    public Vector2 totalSize = new Vector2(100.0f,100.0f);

    void Awake() {
        //main.adminPanel = this;
    }
    void Start () {
        calculatePositions();
	}

    Vector2 calculatePositions() {
        if (hideAtBorder == Side.TOP || hideAtBorder == Side.TOP_LEFT || hideAtBorder == Side.TOP_LEFT)
            showPosition.y = 0;
        else showPosition.y = Screen.height - totalSize.y;

        
        return new Vector2(0, 0);
    }


    void Update () {
	
	}

    void OnGUI() {
        
        GUI.Box(new Rect(0.0f, 0.0f, 100.0f, 100.0f), "AdminPanel");
    }
}
