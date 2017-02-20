using UnityEngine;
using System.Collections;

enum command    // типы команд тача 
{
    UP,         // отжатие
    DOWN        // нажатие
}
// Класс обрабатывающий все пользовательские действия
public class GameInput : MonoBehaviour {
    //public float distToCameraRay = 11.0f; // для оптимизации
    MAIN main = MAIN.getMain;
    GameObject lastObjectUnderMouse = null; // последний объект под мышкой (на который нажимали)
	// Use this for initialization
    /*void Awake() {
        main = MAIN.getMain;
    }
	void Start () {
	
	}*/

    bool onMouse(command com) // при нажатии/отжатии кнопки мыши или тач по экрану
    {
        bool res = false;
        if (com == command.DOWN)    res = onMouseDown();
        else                        res = onMouseUp();

        if (res) return true;
        
        /*GameObject selected = main.getObjectUnderMouse();
        if (selected.name == "TicketCell(Clone)") {
            TicketCell tc = selected.GetComponent<TicketCell>();
            tc.changeMark();
        }*/
        return false;
    }
    bool onMouseDown() // нажатие
    {
        bool res = false;
        GameObject selected = GameInput.getObjectUnderMouse();
        if (selected) {
            if (selected.name == BaseController.backGroundName)
                selected = selected.transform.parent.gameObject;
            BaseController controller = selected.GetComponent<BaseController>();
            if (controller) {
                res = controller.onMouseDown();
                lastObjectUnderMouse = selected;
            }
        }
        return res;
    }
    bool onMouseUp(GameObject selected, bool overSelected) // отжатие, с передачей объекта для проверки, этот же элемент участвует в отжатии
    {
        if (selected.name == BaseController.backGroundName)
            selected = selected.transform.parent.gameObject;
        BaseController controller = selected.GetComponent<BaseController>();
        if (controller) {
            return controller.onMouseUp(overSelected);
        }
        return false;
    }
    bool onMouseUp() {
        //print("[onMouseUp]");
        bool res = false;
        GameObject selected = GameInput.getObjectUnderMouse();
        if (selected && selected == lastObjectUnderMouse) {
            res = onMouseUp(selected, true);
        } else if (lastObjectUnderMouse) {
            res = onMouseUp(lastObjectUnderMouse, false);
            lastObjectUnderMouse = null;
        }
        return res;
    }
    public static GameObject getObjectUnderMouse() // получить объект под мышью
    {
        //print("█ actualInputLayer:"+MAIN.getMain.actualInputLayer);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 11, (int)ScenesController.getScenesController.actualInputLayer);
        if (hit)
            return hit.collider.gameObject;
        return null;
    }
    void Update () // Опрос устройства на состояния ... ввода 
    {
        if (Input.GetMouseButtonDown(0))
            onMouseDown();
        else if (Input.GetMouseButtonUp(0))
            onMouseUp();
    }
}
